using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Detects PascalCase attributes passed to BlazOrbit components in .razor markup that do not
/// correspond to a declared <c>[Parameter]</c> property (or a generic type parameter) on the
/// target component.
///
/// Because every public component ultimately inherits from <c>BOBComponentBase</c> (or an internal
/// component with <c>CaptureUnmatchedValues = true</c>), unknown attributes are silently swallowed
/// by <c>AdditionalAttributes</c> and forwarded to the DOM. That makes typos or stale parameters
/// invisible at compile time. This audit catches them.
///
/// Intentional non-parameter attributes (e.g. debug-only flags read from AdditionalAttributes) are
/// recorded in <see cref="AttributeAllowlist"/> with a justification.
/// </summary>
[Trait("Library", "ComponentAudit")]
public class ComponentParameterAuditTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();
    private static readonly string SrcBlazOrbit = Path.Combine(RepoRoot, "src", "BlazOrbit");
    private static readonly string SrcCore = Path.Combine(RepoRoot, "src", "BlazOrbit.Core");

    /// <summary>
    /// Attributes that intentionally have no <c>[Parameter]</c> declaration but are consumed via
    /// <c>AdditionalAttributes</c>. Each entry must explain why it is allowed.
    ///
    /// <para>Key format:
    ///   <c>"AttributeName"</c> — applies to every component.
    ///   <c>"ComponentName|AttributeName"</c> — applies to one component only.
    /// </para>
    /// </summary>
    private static readonly Dictionary<string, string> AttributeAllowlist = new(StringComparer.Ordinal)
    {
        ["TrackPerformanceEnabled"] =
            "Debug-only flag read from AdditionalAttributes by BOBComponentBase pipeline; not a [Parameter].",
    };

    [Fact]
    public void Should_Only_Pass_Declared_Parameters_To_Components()
    {
        Dictionary<string, Type> componentTypes = LoadComponentTypesByName();
        (Dictionary<string, HashSet<string>> parameterIndex, Dictionary<string, bool> hasCaptureUnmatchedValues) = BuildParameterIndex(componentTypes);
        List<string> violations = [];

        foreach (string file in EnumerateRazorFiles())
        {
            string content = File.ReadAllText(file);
            foreach ((string tagName, List<string> attrs, int lineNumber) in ExtractComponentUsages(content))
            {
                if (!componentTypes.TryGetValue(tagName, out Type? _))
                {
                    continue;
                }

                if (!parameterIndex.TryGetValue(tagName, out HashSet<string>? validParams))
                {
                    continue;
                }

                bool componentSupportsAdditionalAttributes = hasCaptureUnmatchedValues.GetValueOrDefault(tagName, false);

                foreach (string attr in attrs)
                {
                    if (validParams.Contains(attr))
                    {
                        continue;
                    }

                    string genericKey = attr;
                    string specificKey = $"{tagName}|{attr}";

                    if (AttributeAllowlist.ContainsKey(genericKey) || AttributeAllowlist.ContainsKey(specificKey))
                    {
                        continue;
                    }

                    // If the component supports AdditionalAttributes, the attribute will be swallowed silently.
                    // We only flag an error when the component does NOT support AdditionalAttributes,
                    // because Blazor will throw or ignore it in a way that breaks the component contract.
                    if (componentSupportsAdditionalAttributes)
                    {
                        continue;
                    }

                    string relativePath = file.Replace(RepoRoot + Path.DirectorySeparatorChar, "");
                    violations.Add($"{relativePath}({lineNumber}): <{tagName}> attribute '" + attr + "' is not a known parameter or type parameter, and the component does not support AdditionalAttributes (no CaptureUnmatchedValues).");
                }
            }
        }

        violations.Should().BeEmpty(
            because: "every PascalCase attribute passed to a BlazOrbit component that does NOT support " +
                     "AdditionalAttributes must be a declared [Parameter] (or a generic type parameter). " +
                     "If the component has CaptureUnmatchedValues=true (directly or via inheritance), " +
                     "unknown attributes are forwarded to the DOM and are therefore allowed. " +
                     "Components without CaptureUnmatchedValues will cause a runtime/compile-time error " +
                     "when passed unknown attributes.\n\n" +
                     string.Join("\n", violations));
    }

    /// <summary>
    /// Guards the allowlist from rotting. Every entry must still describe a real gap.
    /// </summary>
    [Fact]
    public void Allowlist_Should_Not_Contain_Stale_Entries()
    {
        Dictionary<string, Type> componentTypes = LoadComponentTypesByName();
        (Dictionary<string, HashSet<string>> parameterIndex, _) = BuildParameterIndex(componentTypes);
        List<string> stale = [];

        foreach (string key in AttributeAllowlist.Keys)
        {
            string[] parts = key.Split('|', 2);
            string attrName = parts[^1];
            string? tagName = parts.Length == 2 ? parts[0] : null;

            bool anyFileUsesIt = EnumerateRazorFiles()
                .Any(file => File.ReadAllText(file).Contains($" {attrName}=", StringComparison.Ordinal));

            if (!anyFileUsesIt)
            {
                stale.Add($"AttributeAllowlist[{key}] - no .razor file passes '{attrName}' anymore.");
                continue;
            }

            if (tagName is not null && parameterIndex.TryGetValue(tagName, out HashSet<string>? parameters))
            {
                if (parameters.Contains(attrName))
                {
                    stale.Add($"AttributeAllowlist[{key}] - {tagName} now declares '{attrName}' as a [Parameter]; remove the entry.");
                }
            }
        }

        stale.Should().BeEmpty(
            because: "stale allowlist entries hide future drift. Remove the entries listed below.\n\n" +
                     string.Join("\n", stale));
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static Dictionary<string, Type> LoadComponentTypesByName()
    {
        Dictionary<string, Type> result = new(StringComparer.Ordinal);

        string testAssemblyDir = Path.GetDirectoryName(typeof(ComponentParameterAuditTests).Assembly.Location)!;

        string[] assemblyPaths =
        [
            Path.Combine(testAssemblyDir, "BlazOrbit.dll"),
            Path.Combine(testAssemblyDir, "BlazOrbit.Core.dll"),
        ];

        foreach (string path in assemblyPaths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            Assembly assembly = Assembly.LoadFrom(path);
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t is not null).ToArray()!;
            }

            foreach (Type type in types)
            {
                if (!typeof(ComponentBase).IsAssignableFrom(type))
                {
                    continue;
                }

                string name = type.Name;
                int backtick = name.IndexOf('`');
                if (backtick >= 0)
                {
                    name = name[..backtick];
                }

                if (!result.ContainsKey(name))
                {
                    result.Add(name, type);
                }
            }
        }

        result.Should().NotBeEmpty("at least one component type must be loaded from BlazOrbit assemblies");
        return result;
    }

    private static (Dictionary<string, HashSet<string>> Parameters, Dictionary<string, bool> HasCaptureUnmatchedValues)
        BuildParameterIndex(Dictionary<string, Type> componentTypes)
    {
        Dictionary<string, HashSet<string>> parameters = new(StringComparer.Ordinal);
        Dictionary<string, bool> hasCapture = new(StringComparer.Ordinal);
        Type parameterAttrType = typeof(ParameterAttribute);

        foreach ((string name, Type type) in componentTypes)
        {
            HashSet<string> paramSet = new(StringComparer.Ordinal);

            foreach (Type typeParam in type.GetGenericArguments())
            {
                paramSet.Add(typeParam.Name);
            }

            bool componentHasCapture = false;
            Type? current = type;
            while (current is not null && current != typeof(object))
            {
                foreach (PropertyInfo prop in current.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    ParameterAttribute? paramAttr = prop.GetCustomAttributes(parameterAttrType, inherit: false)
                        .Cast<ParameterAttribute>()
                        .FirstOrDefault();
                    if (paramAttr is not null)
                    {
                        paramSet.Add(prop.Name);
                        if (paramAttr.CaptureUnmatchedValues)
                        {
                            componentHasCapture = true;
                        }
                    }
                }

                current = current.BaseType;
            }

            parameters[name] = paramSet;
            hasCapture[name] = componentHasCapture;
        }

        return (parameters, hasCapture);
    }

    private static IEnumerable<string> EnumerateRazorFiles()
    {
        foreach (string root in new[] { SrcBlazOrbit, SrcCore })
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (string file in Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories))
            {
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                yield return file;
            }
        }
    }

    private static IEnumerable<(string TagName, List<string> Attributes, int LineNumber)> ExtractComponentUsages(string content)
    {
        List<(string, List<string>, int)> results = [];

        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] != '<')
            {
                continue;
            }

            if (i + 1 < content.Length && content[i + 1] == '/')
            {
                continue;
            }

            if (!TryMatchTagName(content, i, out string tagName, out int tagStart))
            {
                continue;
            }

            int tagEnd = FindTagEnd(content, tagStart);
            if (tagEnd < 0)
            {
                continue;
            }

            string tagBody = content.Substring(tagStart, tagEnd - tagStart + 1);
            List<string> attributes = ExtractPascalCaseAttributes(tagBody);

            int lineNumber = 1;
            for (int j = 0; j < i; j++)
            {
                if (content[j] == '\n')
                {
                    lineNumber++;
                }
            }

            results.Add((tagName, attributes, lineNumber));
        }

        return results;
    }

    private static bool TryMatchTagName(string content, int pos, out string tagName, out int tagStart)
    {
        tagName = string.Empty;
        tagStart = pos;

        if (pos >= content.Length || content[pos] != '<')
        {
            return false;
        }

        // Reject if preceded by a character that suggests this is a generic type argument
        // (e.g. Func<BOBInputColor, RenderFragment>) rather than a component tag.
        if (pos > 0)
        {
            char prev = content[pos - 1];
            if (char.IsLetterOrDigit(prev) || prev == '_' || prev == '<')
            {
                return false;
            }
        }

        int nameStart = pos + 1;

        if (nameStart + 4 <= content.Length
            && content[nameStart] == '_'
            && content[nameStart + 1] == 'B'
            && content[nameStart + 2] == 'O'
            && content[nameStart + 3] == 'B')
        {
            int end = nameStart + 4;
            while (end < content.Length && IsTagNameChar(content[end]))
            {
                end++;
            }

            tagName = content[nameStart..end];
            return true;
        }

        if (nameStart + 3 <= content.Length
            && content[nameStart] == 'B'
            && content[nameStart + 1] == 'O'
            && content[nameStart + 2] == 'B')
        {
            int end = nameStart + 3;
            while (end < content.Length && IsTagNameChar(content[end]))
            {
                end++;
            }

            tagName = content[nameStart..end];
            return true;
        }

        return false;
    }

    private static bool IsTagNameChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static int FindTagEnd(string content, int start)
    {
        bool inDoubleQuotes = false;
        bool inSingleQuotes = false;

        for (int i = start; i < content.Length; i++)
        {
            char c = content[i];
            if (c == '"' && !inSingleQuotes)
            {
                inDoubleQuotes = !inDoubleQuotes;
            }
            else if (c == '\'' && !inDoubleQuotes)
            {
                inSingleQuotes = !inSingleQuotes;
            }
            else if (c == '>' && !inDoubleQuotes && !inSingleQuotes)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Extracts attribute names that look like component parameters (PascalCase) from the raw tag
    /// body, ignoring anything inside quoted strings and Blazor directives.
    /// </summary>
    private static List<string> ExtractPascalCaseAttributes(string tagBody)
    {
        List<string> result = [];
        bool inDoubleQuotes = false;
        bool inSingleQuotes = false;

        for (int i = 0; i < tagBody.Length; i++)
        {
            char c = tagBody[i];
            if (c == '"' && !inSingleQuotes)
            {
                inDoubleQuotes = !inDoubleQuotes;
                continue;
            }

            if (c == '\'' && !inDoubleQuotes)
            {
                inSingleQuotes = !inSingleQuotes;
                continue;
            }

            if (inDoubleQuotes || inSingleQuotes)
            {
                continue;
            }

            if (!char.IsUpper(c))
            {
                continue;
            }

            // Must be preceded by whitespace (not the tag name itself which is preceded by <)
            if (i > 0 && !char.IsWhiteSpace(tagBody[i - 1]))
            {
                continue;
            }

            int start = i;
            while (i < tagBody.Length && (char.IsLetterOrDigit(tagBody[i]) || tagBody[i] == '_'))
            {
                i++;
            }

            string name = tagBody[start..i];

            // Must be followed by =, whitespace, >, or />
            if (i < tagBody.Length)
            {
                char next = tagBody[i];
                if (next == '=' || char.IsWhiteSpace(next) || next == '>' || next == '/')
                {
                    result.Add(name);
                }
            }
        }

        return result;
    }

    private static string ResolveRepoRoot([CallerFilePath] string? thisFile = null)
    {
        DirectoryInfo? dir = new FileInfo(thisFile!).Directory;
        for (int i = 0; i < 4 && dir is not null; i++)
        {
            dir = dir.Parent;
        }

        return dir!.FullName;
    }
}

