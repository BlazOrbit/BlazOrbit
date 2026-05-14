using BlazOrbit.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Core;

/// <summary>
/// COMP-ARCH-01: EventCallback parameters in Razor markup must bind via EventCallback.Factory.Create.
///
/// Reflection discovers every [Parameter] of type EventCallback / EventCallback&lt;T&gt; in the
/// BlazOrbit assembly (component → param-name set). The lint then scans every tag in
/// src/BlazOrbit/**/*.razor and, for the typed-EventCallback parameters of that component, asserts
/// the binding is one of:
///   • <c>@(EventCallback.Factory.Create…)</c>
///   • <c>@(lambda)</c> — Razor emits Factory.Create with `this` for lambdas
///   • <c>@AttrName</c> — same-name parameter forwarding
///   • empty / not bound
///
/// Bare method references (<c>OnClick="Handle"</c> / <c>OnClick="@Handle"</c>) and unwrapped
/// expressions are rejected. Non-EventCallback params (Func&lt;&gt;, bool, etc.) are skipped.
/// </summary>
[Trait("Core", "ComponentArchitectureLint")]
public class ComponentArchitectureLintTests
{
    private static readonly string SrcBlazOrbitPath = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..",
        "src", "BlazOrbit"));

    [Fact]
    public void RazorSrc_Should_Bind_EventCallback_Parameters_Via_Factory_Create()
    {
        // Stage 1 — reflect over the BlazOrbit assembly and build {componentName → EventCallback param names}.
        Dictionary<string, HashSet<string>> ecParamsByComponent =
            DiscoverEventCallbackParameters(typeof(BOBButton).Assembly);

        ecParamsByComponent.Should().NotBeEmpty(
            "BlazOrbit assembly must expose components with [Parameter] EventCallback properties");

        // Stage 2 — scan every src/BlazOrbit Razor file and validate the bindings to those parameters.
        string[] razorFiles = Directory.GetFiles(SrcBlazOrbitPath, "*.razor", SearchOption.AllDirectories);
        razorFiles.Should().NotBeEmpty("there must be Razor source files to lint");

        List<string> violations = [];

        foreach (string file in razorFiles)
        {
            string content = File.ReadAllText(file);
            string relativePath = Path.GetRelativePath(SrcBlazOrbitPath, file);

            foreach (ComponentUsage usage in FindComponentUsages(content, ecParamsByComponent.Keys))
            {
                if (!ecParamsByComponent.TryGetValue(usage.ComponentName, out HashSet<string>? ecParams))
                    continue;

                foreach (AttributeBinding attr in usage.Attributes)
                {
                    if (!ecParams.Contains(attr.Name))
                        continue;

                    if (IsValidEventCallbackBinding(attr.Name, attr.Value))
                        continue;

                    violations.Add(
                        $"{relativePath}:{usage.LineNumber} <{usage.ComponentName} {attr.Name}=\"{attr.Value}\">");
                }
            }
        }

        violations.Should().BeEmpty(
            "EventCallback parameters must use @(EventCallback.Factory.Create<T>(this, handler)) " +
            "or a @(lambda). Bare method references omit the explicit receiver (COMP-ARCH-01). " +
            "Same-name forwarding (OnFoo=\"@OnFoo\") is allowed.");
    }

    // ── reflection ───────────────────────────────────────────────────────────

    private static Dictionary<string, HashSet<string>> DiscoverEventCallbackParameters(
        params Assembly[] assemblies)
    {
        Dictionary<string, HashSet<string>> result = new(StringComparer.Ordinal);

        foreach (Assembly asm in assemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null).ToArray()!; }

            foreach (Type type in types)
            {
                if (type.IsAbstract || type.IsInterface) continue;

                HashSet<string> ecParams = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttribute<ParameterAttribute>() is not null)
                    .Where(p => IsEventCallbackType(p.PropertyType))
                    .Select(p => p.Name)
                    .ToHashSet(StringComparer.Ordinal);

                if (ecParams.Count == 0) continue;

                string shortName = type.Name;
                int backtick = shortName.IndexOf('`');
                if (backtick > 0) shortName = shortName[..backtick];

                if (result.TryGetValue(shortName, out HashSet<string>? existing))
                    existing.UnionWith(ecParams);
                else
                    result[shortName] = ecParams;
            }
        }

        return result;
    }

    private static bool IsEventCallbackType(Type type)
    {
        if (type == typeof(EventCallback)) return true;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EventCallback<>)) return true;
        return false;
    }

    // ── razor parsing ────────────────────────────────────────────────────────

    private sealed record ComponentUsage(string ComponentName, int LineNumber, IReadOnlyList<AttributeBinding> Attributes);
    private sealed record AttributeBinding(string Name, string Value);

    /// <summary>Matches an opening component tag <c>&lt;Name</c> where <c>Name</c> is PascalCase
    /// (with optional <c>_</c> prefix for internal components). The lookahead ensures the captured
    /// name is followed by whitespace, <c>/</c>, or <c>&gt;</c> so we don't match prefixes.</summary>
    private static readonly Regex TagOpenPattern = new(
        @"<(?<name>[_A-Z]\w*)(?=[\s/>])",
        RegexOptions.Compiled);

    /// <summary>Matches a Razor attribute <c>Name="value"</c>. Value is anything up to the next
    /// double quote (Razor doesn't embed unescaped <c>"</c> inside double-quoted attribute values).</summary>
    private static readonly Regex AttributePattern = new(
        @"\b(?<name>[A-Z_]\w*)\s*=\s*""(?<value>[^""]*)""",
        RegexOptions.Compiled);

    private static List<ComponentUsage> FindComponentUsages(string content, IEnumerable<string> knownComponents)
    {
        HashSet<string> nameSet = new(knownComponents, StringComparer.Ordinal);
        List<ComponentUsage> result = [];

        foreach (Match m in TagOpenPattern.Matches(content))
        {
            string name = m.Groups["name"].Value;
            if (!nameSet.Contains(name)) continue;

            int bodyStart = m.Index + m.Length;
            int tagEnd = FindTagEnd(content, bodyStart);
            if (tagEnd < 0) continue;

            string body = content[bodyStart..tagEnd];
            List<AttributeBinding> attrs = AttributePattern
                .Matches(body)
                .Select(am => new AttributeBinding(am.Groups["name"].Value, am.Groups["value"].Value))
                .ToList();

            int lineNumber = content[..m.Index].Count(c => c == '\n') + 1;
            result.Add(new ComponentUsage(name, lineNumber, attrs));
        }

        return result;
    }

    /// <summary>Scans forward for the closing <c>&gt;</c> of a tag, treating <c>"…"</c> as opaque
    /// so quoted <c>&gt;</c> characters (inside attribute values / generics like <c>List&lt;T&gt;</c>) are ignored.</summary>
    private static int FindTagEnd(string content, int start)
    {
        bool inQuote = false;
        for (int i = start; i < content.Length; i++)
        {
            char c = content[i];
            if (inQuote)
            {
                if (c == '"') inQuote = false;
            }
            else if (c == '"')
            {
                inQuote = true;
            }
            else if (c == '>')
            {
                return i;
            }
        }
        return -1;
    }

    // ── binding validation ───────────────────────────────────────────────────

    private static bool IsValidEventCallbackBinding(string attrName, string attrValue)
    {
        if (string.IsNullOrWhiteSpace(attrValue)) return true;

        string trimmed = attrValue.Trim();

        // Same-name parameter forwarding: @AttrName (no parens). Forwards a property by reference;
        // the receiver was set wherever the property was originally constructed.
        if (trimmed.StartsWith('@') && trimmed.Length > 1 && trimmed[1] != '(')
        {
            return string.Equals(trimmed[1..], attrName, StringComparison.Ordinal);
        }

        // @(expression): require explicit Factory.Create or a lambda (=> Razor emits Factory).
        if (trimmed.StartsWith("@(", StringComparison.Ordinal))
        {
            if (trimmed.Contains("EventCallback.Factory.Create", StringComparison.Ordinal)) return true;
            if (trimmed.Contains("=>", StringComparison.Ordinal)) return true;
            return false;
        }

        return false;
    }
}
