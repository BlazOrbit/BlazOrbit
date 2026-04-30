using BlazOrbit.Components;
using FluentAssertions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// CSS-VAR-01: every <c>var(--name)</c> reference in scoped CSS (<c>*.razor.css</c>) must resolve
/// to a declaration that exists in the same file, in the global CssBundle, or in
/// <see cref="FeatureDefinitions"/> constants. Catches typos, stale prefixes from renames,
/// copy-paste drift, and forgotten private-var updates.
/// </summary>
[Trait("Library", "CssVarAudit")]
public class CssVarDeclarationAuditTests
{
    private static readonly string SrcBlazOrbit = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..",
        "src", "BlazOrbit"));

    private static readonly Regex VarReference = new(
        @"var\(\s*(?<name>--[a-zA-Z_][a-zA-Z0-9_-]*)",
        RegexOptions.Compiled);

    private static readonly Regex VarDeclaration = new(
        @"(?<name>--[a-zA-Z_][a-zA-Z0-9_-]*)\s*:",
        RegexOptions.Compiled);

    /// <summary>
    /// Variables that are intentionally injected at runtime (JS interop, dynamic theming,
    /// consumer overrides, or component-generated inline styles) and therefore have no
    /// static CSS declaration in the scoped file.
    /// Each entry must have a one-line justification.
    /// </summary>
    private static readonly Dictionary<string, string> Allowlist = new(StringComparer.Ordinal)
    {
        // Color picker canvas/alpha slider reads the current colour value from inline styles.
        ["--color"] = "Injected by BOBColorPicker.razor via inline style on the alpha slider element.",
    };

    /// <summary>
    /// Layout components generate per-element inline styles that carry these variables.
    /// They are consumed via var(--name, fallback) in the scoped CSS.
    /// </summary>
    private static readonly Regex LayoutDynamicVarPattern = new(
        @"^--(row-gap|col-gap|gap|p|pt|pr|pb|pl|m|mt|mr|mb|ml|max-w|order|span|columns|offset|offset-xs|offset-sm|offset-md|offset-lg|offset-xl|order-xs|order-sm|order-md|order-lg|order-xl|xs|sm|md|lg|xl|gap-xs)$",
        RegexOptions.Compiled);

    [Fact]
    public void Should_Declare_Every_Css_Variable_Referenced_In_Scoped_Css()
    {
        HashSet<string> globalDecls = CollectDeclarationsFromCssBundle();
        globalDecls.UnionWith(CollectFeatureDefinitionsVariableNames());

        List<string> orphans = [];

        foreach (string file in EnumerateScopedCssFiles())
        {
            string content = File.ReadAllText(file);
            string fileName = Path.GetFileName(file);
            HashSet<string> localDecls = CollectDeclarations(content);
            bool isLayoutComponent = file.Contains($"{Path.DirectorySeparatorChar}Layout{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

            foreach (Match m in VarReference.Matches(content))
            {
                string name = m.Groups["name"].Value;
                if (localDecls.Contains(name) || globalDecls.Contains(name) || Allowlist.ContainsKey(name))
                {
                    continue;
                }

                if (isLayoutComponent && LayoutDynamicVarPattern.IsMatch(name))
                {
                    continue;
                }

                int line = LineOf(content, m.Index);
                orphans.Add($"{fileName}:{line}: var({name}) references undeclared variable");
            }
        }

        orphans.Should().BeEmpty(
            because: "every CSS custom property referenced via var() in scoped CSS must be declared in the same file, " +
                     "in a global CssBundle file, or as a FeatureDefinitions constant. " +
                     "Undeclared variables silently fall back to inherited/invalid values, hiding typos " +
                     "and stale prefixes. See CLAUDE.md §CSS architecture rule 4.\n\n" +
                     string.Join("\n", orphans));
    }

    /// <summary>
    /// Guards the allowlist from rotting. Every entry must still describe a real runtime sink;
    /// once the variable gains a static declaration, the entry must be removed.
    /// </summary>
    [Fact]
    public void Allowlist_Should_Not_Contain_Stale_Entries()
    {
        HashSet<string> globalDecls = CollectDeclarationsFromCssBundle();
        globalDecls.UnionWith(CollectFeatureDefinitionsVariableNames());

        List<string> stale = [];

        foreach (string key in Allowlist.Keys)
        {
            if (globalDecls.Contains(key))
            {
                stale.Add($"Allowlist[{key}] — now declared in CSS or FeatureDefinitions; remove the entry");
            }
        }

        stale.Should().BeEmpty(
            because: "stale allowlist entries hide future drift. Remove the entries listed below.\n\n" +
                     string.Join("\n", stale));
    }

    private static IEnumerable<string> EnumerateScopedCssFiles()
    {
        if (!Directory.Exists(SrcBlazOrbit))
        {
            yield break;
        }

        foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, "*.razor.css", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            yield return file;
        }
    }

    private static HashSet<string> CollectDeclarationsFromCssBundle()
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        string bundlePath = Path.Combine(SrcBlazOrbit, "CssBundle");
        if (!Directory.Exists(bundlePath))
        {
            return result;
        }

        foreach (string file in Directory.EnumerateFiles(bundlePath, "*.css", SearchOption.TopDirectoryOnly))
        {
            result.UnionWith(CollectDeclarations(File.ReadAllText(file)));
        }

        return result;
    }

    private static HashSet<string> CollectDeclarations(string content)
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        foreach (Match m in VarDeclaration.Matches(content))
        {
            result.Add(m.Groups["name"].Value);
        }

        return result;
    }

    private static HashSet<string> CollectFeatureDefinitionsVariableNames()
    {
        Type fd = typeof(FeatureDefinitions);
        HashSet<string> result = new(StringComparer.Ordinal);
        CollectStringConstants(fd, result);
        return result;
    }

    private static void CollectStringConstants(Type type, HashSet<string> into)
    {
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (field.IsLiteral && field.FieldType == typeof(string))
            {
                string? value = (string?)field.GetRawConstantValue();
                if (!string.IsNullOrEmpty(value) && value.StartsWith("--", StringComparison.Ordinal))
                {
                    into.Add(value);
                }
            }
        }

        foreach (Type nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            CollectStringConstants(nested, into);
        }
    }

    private static int LineOf(string content, int index)
    {
        int line = 1;
        for (int i = 0; i < index && i < content.Length; i++)
        {
            if (content[i] == '\n')
            {
                line++;
            }
        }

        return line;
    }
}
