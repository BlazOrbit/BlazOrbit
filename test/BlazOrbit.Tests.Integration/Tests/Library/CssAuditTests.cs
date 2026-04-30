using BlazOrbit.Components;
using FluentAssertions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
///
/// Builds three universes from the source tree:
///
///   <list type="bullet">
///     <item><description><b>DOM-emitted</b> — every <c>data-bob-*</c> string literal that lives in
///     <c>.razor</c>, <c>.razor.cs</c>, or <c>.cs</c> files under <c>src/BlazOrbit</c> and
///     <c>src/BlazOrbit.Core</c>. This is what <c>BOBComponentAttributesBuilder</c> (and any
///     consumer override of <c>BuildComponentDataAttributes</c>) eventually puts on the
///     <c>&lt;bob-component&gt;</c> root.</description></item>
///     <item><description><b>CSS-declared</b> — every <c>[data-bob-*]</c> attribute selector that
///     appears in <c>.css</c> or <c>.razor.css</c> under the same source tree.</description></item>
///     <item><description><b>Standard-prescribed</b> — every constant in
///     <see cref="FeatureDefinitions.InlineVariables"/>; the test asserts each one is referenced
///     via <c>var(--bob-inline-*)</c> at least once in CSS.</description></item>
///   </list>
///
/// Intentional gaps (purely informational attributes, JS-side data sinks, etc.) are recorded in the
/// allowlists below with a justification. Anything that escapes both the regex extractors and the
/// allowlists is treated as a real bug — either dead CSS or DOM that no rule selects on.
/// </summary>
[Trait("Library", "CssAudit")]
public class CssAuditTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();
    private static readonly string SrcBlazOrbit = Path.Combine(RepoRoot, "src", "BlazOrbit");
    private static readonly string SrcCore = Path.Combine(RepoRoot, "src", "BlazOrbit.Core");

    // Names must start with a letter and end with an alphanumeric — this rejects
    // partial captures that hit a placeholder like `--bob-inline-prefix-{...}` in
    // documentation or comments and would otherwise yield a bogus "prefix-" name.
    private const string NamePart = @"(?<name>[a-z](?:[a-z0-9-]*[a-z0-9])?)";

    private static readonly Regex DataAttrLiteral =
        new(@"data-bob-" + NamePart, RegexOptions.Compiled);
    private static readonly Regex DataAttrSelector =
        new(@"\[data-bob-" + NamePart, RegexOptions.Compiled);
    // TypeScript references can appear inside quotes, backticks (template literals),
    // or even concatenated strings. We use the same extractor as C#/Razor since the
    // literal prefix 'data-bob-' is unambiguous inside .ts files.
    private static readonly Regex DataAttrInTypeScript = DataAttrLiteral;
    private static readonly Regex InlineVarReference =
        new(@"var\(\s*--bob-inline-" + NamePart, RegexOptions.Compiled);
    private static readonly Regex InlineVarLiteral =
        new(@"--bob-inline-" + NamePart, RegexOptions.Compiled);

    // Allowlist keys are the SUFFIX after `data-bob-` (or `--bob-inline-`), matching the regex group.

    /// <summary>
    /// Attributes emitted by code on purpose without expecting any CSS to select on them.
    /// Each entry must have a one-line justification.
    ///
    /// <para>Since the audit now auto-detects <c>data-bob-*</c> references in TypeScript,
    /// this list should only contain attributes that have <b>no</b> CSS selector <b>and</b>
    /// no TS consumer. If an attribute is referenced in TS, the extractor discovers it
    /// automatically and no manual entry is required here.</para>
    /// </summary>
    /// <summary>
    /// Attributes emitted by code on purpose without expecting any CSS to select on them.
    /// Each entry must have a one-line justification.
    ///
    /// <para>Since the audit auto-detects <c>data-bob-*</c> references in TypeScript,
    /// this list should only contain attributes that have <b>no</b> CSS selector <b>and</b>
    /// no TS consumer. If an attribute is referenced in TS, the extractor discovers it
    /// automatically and no manual entry is required here.</para>
    /// </summary>
    private static readonly Dictionary<string, string> DomOnlyAllowlist = [];

    /// <summary>
    /// Selectors declared in CSS that intentionally have no in-tree code emitter — typically because
    /// the value is set by JS/3rd parties or is an opt-in surface for consumer apps.
    /// </summary>
    private static readonly Dictionary<string, string> CssOnlyAllowlist = [];

    /// <summary>
    /// FeatureDefinitions.InlineVariables entries that intentionally have no <c>var(--bob-inline-*)</c>
    /// reference in CSS today. Each is a frozen baseline finding from CSS-OPT-02 — clearing the entry
    /// requires either wiring a CSS reference or removing the constant.
    /// </summary>
    private static readonly Dictionary<string, string> InlineVarOrphanAllowlist = [];

    [Fact]
    public void Should_Have_Css_Selector_For_Each_DataAttribute_Emitted_By_Code()
    {
        HashSet<string> dom = ExtractFromCode();
        HashSet<string> css = ExtractFromCss();
        HashSet<string> ts = ExtractFromTypeScript();

        // Any attribute referenced in TS is considered a legitimate JS sink;
        // it does not need a CSS selector and does not need a manual allowlist entry.
        IEnumerable<string> orphans = dom
            .Except(css)
            .Except(ts)
            .Except(DomOnlyAllowlist.Keys)
            .OrderBy(s => s, StringComparer.Ordinal);

        orphans.Should().BeEmpty(
            because: "every data-bob-* the code writes onto the DOM must be selected by at least one " +
                     "CSS rule, or be consumed by TypeScript under src/BlazOrbit/Types. " +
                     "Otherwise the attribute is dead weight or styling is missing. Add the " +
                     "matching selector under src/BlazOrbit/CssBundle (global) or alongside " +
                     "the component's .razor.css (scoped). If the attribute is a pure JS sink, " +
                     "reference it in TypeScript so the auto-detector picks it up, or add a " +
                     "justified entry to DomOnlyAllowlist.\n\nOffending attributes:\n  " +
                     string.Join("\n  ", orphans.Select(o => $"data-bob-{o}")));
    }

    [Fact]
    public void Should_Have_Code_Source_For_Each_DataAttribute_Selected_By_Css()
    {
        HashSet<string> dom = ExtractFromCode();
        HashSet<string> css = ExtractFromCss();

        IEnumerable<string> orphans = css
            .Except(dom)
            .Except(CssOnlyAllowlist.Keys)
            .OrderBy(s => s, StringComparer.Ordinal);

        orphans.Should().BeEmpty(
            because: "every [data-bob-*] selector in CSS must have a code source that emits the " +
                     "attribute, otherwise the rule never matches in production. Either add the emitter " +
                     "(component override of BuildComponentDataAttributes / FeatureDefinitions constant) " +
                     "or drop the selector. JS-driven attributes belong in CssOnlyAllowlist with a " +
                     "justification.\n\nOffending selectors:\n  " +
                     string.Join("\n  ", orphans.Select(o => $"[data-bob-{o}]")));
    }

    [Fact]
    public void Should_Reference_Each_FeatureDefinitions_InlineVariable_From_Css()
    {
        HashSet<string> declared = GetFeatureDefinitionsInlineVariables();
        declared.Should().NotBeEmpty(
            because: "FeatureDefinitions.InlineVariables must expose the canonical --bob-inline-* names");

        HashSet<string> referenced = ExtractInlineVarReferencesFromCss();

        IEnumerable<string> orphans = declared
            .Except(referenced)
            .Except(InlineVarOrphanAllowlist.Keys)
            .OrderBy(s => s, StringComparer.Ordinal);

        orphans.Should().BeEmpty(
            because: "every FeatureDefinitions.InlineVariables constant must be consumed by at least " +
                     "one CSS rule via var(--bob-inline-*). An unconsumed constant is dead surface that " +
                     "components emit into 'style' for nothing. Either drop the constant or wire a " +
                     "CSS reference; document temporary exceptions in InlineVarOrphanAllowlist.\n\n" +
                     "Unreferenced inline variables:\n  " +
                     string.Join("\n  ", orphans.Select(o => $"--bob-inline-{o}")));
    }

    /// <summary>
    /// Guards the allowlists from rotting. Every entry must still describe a real gap; once the
    /// underlying issue is fixed, the entry must be removed in the same PR.
    /// </summary>
    [Fact]
    public void Allowlists_Should_Not_Contain_Stale_Entries()
    {
        HashSet<string> dom = ExtractFromCode();
        HashSet<string> css = ExtractFromCss();
        HashSet<string> declared = GetFeatureDefinitionsInlineVariables();
        HashSet<string> referenced = ExtractInlineVarReferencesFromCss();

        List<string> stale = [];

        HashSet<string> ts = ExtractFromTypeScript();

        foreach (string key in DomOnlyAllowlist.Keys)
        {
            // A DOM-only entry is stale if (a) the code no longer emits it, OR
            // (b) CSS now selects on it (gap closed), OR
            // (c) TS now references it (auto-detected JS sink — no manual entry needed).
            if (!dom.Contains(key))
            {
                stale.Add($"DomOnlyAllowlist[\"{key}\"] — code no longer emits data-bob-{key}");
            }
            else if (css.Contains(key))
            {
                stale.Add($"DomOnlyAllowlist[\"{key}\"] — CSS now selects on [data-bob-{key}]; remove the allowlist entry");
            }
            else if (ts.Contains(key))
            {
                stale.Add($"DomOnlyAllowlist[\"{key}\"] — TypeScript now references data-bob-{key}; remove the allowlist entry (auto-detected)");
            }
        }

        foreach (string key in CssOnlyAllowlist.Keys)
        {
            if (!css.Contains(key))
            {
                stale.Add($"CssOnlyAllowlist[\"{key}\"] — CSS no longer references [data-bob-{key}]");
            }
            else if (dom.Contains(key))
            {
                stale.Add($"CssOnlyAllowlist[\"{key}\"] — code now emits data-bob-{key}; remove the allowlist entry");
            }
        }

        foreach (string key in InlineVarOrphanAllowlist.Keys)
        {
            if (!declared.Contains(key))
            {
                stale.Add($"InlineVarOrphanAllowlist[\"{key}\"] — FeatureDefinitions no longer declares --bob-inline-{key}");
            }
            else if (referenced.Contains(key))
            {
                stale.Add($"InlineVarOrphanAllowlist[\"{key}\"] — CSS now references --bob-inline-{key}; remove the allowlist entry");
            }
        }

        stale.Should().BeEmpty(
            because: "stale allowlist entries hide future drift. Remove the entries listed below — " +
                     "either the underlying issue was fixed, or the surface no longer exists.\n\n" +
                     string.Join("\n", stale));
    }

    [Fact]
    public void Should_Have_FeatureDefinitions_Constant_For_Each_Inline_Variable_Used_In_Code_Or_Css()
    {
        HashSet<string> declared = GetFeatureDefinitionsInlineVariables();
        HashSet<string> usedInCode = ExtractInlineVarLiteralsFromCode();
        HashSet<string> usedInCss = ExtractInlineVarLiteralsFromCss();

        HashSet<string> all = [.. usedInCode];
        all.UnionWith(usedInCss);

        IEnumerable<string> hardcoded = all
            .Except(declared)
            .OrderBy(s => s, StringComparer.Ordinal);

        hardcoded.Should().BeEmpty(
            because: "every --bob-inline-* token in code or CSS must be declared in " +
                     "FeatureDefinitions.InlineVariables, so that the canonical name is the source of " +
                     "truth. Hard-coded tokens drift between emitters and consumers.\n\nMissing " +
                     "FeatureDefinitions entries:\n  " +
                     string.Join("\n  ", hardcoded.Select(o => $"--bob-inline-{o}")));
    }

    private static HashSet<string> ExtractFromCode()
    {
        IEnumerable<string> roots =
        [
            SrcBlazOrbit,
            SrcCore,
        ];

        HashSet<string> result = new(StringComparer.Ordinal);
        foreach (string root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (string ext in (string[])["*.razor", "*.cs"])
            {
                foreach (string file in Directory.EnumerateFiles(root, ext, SearchOption.AllDirectories))
                {
                    if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                    {
                        continue;
                    }

                    string content = File.ReadAllText(file);
                    foreach (Match m in DataAttrLiteral.Matches(content))
                    {
                        result.Add(m.Groups["name"].Value);
                    }
                }
            }
        }

        return result;
    }

    private static HashSet<string> ExtractFromCss()
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        if (!Directory.Exists(SrcBlazOrbit))
        {
            return result;
        }

        foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, "*.css", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match m in DataAttrSelector.Matches(content))
            {
                result.Add(m.Groups["name"].Value);
            }
        }

        return result;
    }

    private static HashSet<string> ExtractInlineVarReferencesFromCss()
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        if (!Directory.Exists(SrcBlazOrbit))
        {
            return result;
        }

        foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, "*.css", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match m in InlineVarReference.Matches(content))
            {
                result.Add(m.Groups["name"].Value);
            }
        }

        return result;
    }

    private static HashSet<string> ExtractInlineVarLiteralsFromCss()
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        if (!Directory.Exists(SrcBlazOrbit))
        {
            return result;
        }

        foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, "*.css", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match m in InlineVarLiteral.Matches(content))
            {
                result.Add(m.Groups["name"].Value);
            }
        }

        return result;
    }

    private static HashSet<string> ExtractFromTypeScript()
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        if (!Directory.Exists(SrcBlazOrbit))
        {
            return result;
        }

        foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, "*.ts", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match m in DataAttrLiteral.Matches(content))
            {
                result.Add(m.Groups["name"].Value);
            }
        }

        return result;
    }

    private static HashSet<string> ExtractInlineVarLiteralsFromCode()
    {
        IEnumerable<string> roots =
        [
            SrcBlazOrbit,
            SrcCore,
        ];
        HashSet<string> result = new(StringComparer.Ordinal);
        foreach (string root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (string ext in (string[])["*.razor", "*.cs"])
            {
                foreach (string file in Directory.EnumerateFiles(root, ext, SearchOption.AllDirectories))
                {
                    if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                    {
                        continue;
                    }

                    string content = File.ReadAllText(file);
                    foreach (Match m in InlineVarLiteral.Matches(content))
                    {
                        result.Add(m.Groups["name"].Value);
                    }
                }
            }
        }

        return result;
    }

    private static HashSet<string> GetFeatureDefinitionsInlineVariables()
    {
        // FeatureDefinitions is internal in BlazOrbit.Core; this assembly has InternalsVisibleTo.
        Type fd = typeof(FeatureDefinitions);
        Type? inline = fd.GetNestedType("InlineVariables", BindingFlags.Public | BindingFlags.NonPublic);
        inline.Should().NotBeNull("FeatureDefinitions.InlineVariables must exist");

        HashSet<string> result = new(StringComparer.Ordinal);
        foreach (FieldInfo field in inline!.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.IsLiteral && field.FieldType == typeof(string))
            {
                string value = (string)field.GetRawConstantValue()!;
                Match m = InlineVarLiteral.Match(value);
                if (m.Success)
                {
                    result.Add(m.Groups["name"].Value);
                }
            }
        }

        return result;
    }

    private static string ResolveRepoRoot([CallerFilePath] string? thisFile = null)
    {
        // thisFile = <repo>/test/BlazOrbit.Tests.Integration/Tests/Library/CssAuditTests.cs
        // Climb 4 levels: Library -> Tests -> BlazOrbit.Tests.Integration -> test -> <repo>
        DirectoryInfo? dir = new FileInfo(thisFile!).Directory;
        for (int i = 0; i < 4 && dir is not null; i++)
        {
            dir = dir.Parent;
        }

        return dir!.FullName;
    }
}
