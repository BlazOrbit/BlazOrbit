using FluentAssertions;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Mechanized invariants from <c>docs/ARCHITECTURE.md</c>. Each test guards a single rule whose
/// violation would otherwise drift back into the codebase silently:
///
/// <list type="bullet">
///   <item>ASYNC-01: never <c>_ = FooAsync()</c> — use <c>BOBAsyncHelper.SafeFireAndForget</c>.</item>
///   <item>ASYNC-02: never <c>ConfigureAwait(false)</c> in library code (Blazor renderer relies on the sync context).</item>
///   <item>ASYNC-03: any teardown-path try/catch on <c>JSDisconnectedException</c> must include the canonical 4-tuple.</item>
///   <item>TEST-TRAIT-01: every test class under <c>Tests.Components.*</c> must be tagged with a <c>Component &lt;Context&gt;</c> Trait.</item>
///   <item>CSS-MEDIA-01: <c>@media</c> queries in scoped <c>.razor.css</c> are reserved for the documented layout exceptions.</item>
///   <item>CACHE-PURE-01: <c>IPureBuiltComponent</c> hooks must not read instance fields (heuristic: no <c>_x</c> identifiers).</item>
/// </list>
/// </summary>
[Trait("Library", "ArchitectureAudit")]
public class ArchitectureAuditTests
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", ".."));

    private static readonly string SrcBlazOrbit = Path.Combine(RepoRoot, "src", "BlazOrbit");
    private static readonly string SrcCore = Path.Combine(RepoRoot, "src", "BlazOrbit.Core");

    private static IEnumerable<string> EnumerateLibrarySources(string extensionPattern)
    {
        IEnumerable<string> blazOrbit = Directory.GetFiles(SrcBlazOrbit, extensionPattern, SearchOption.AllDirectories);
        IEnumerable<string> core = Directory.GetFiles(SrcCore, extensionPattern, SearchOption.AllDirectories);

        return blazOrbit.Concat(core)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
    }

    /// <summary>
    /// ASYNC-01. The pattern <c>_ = FooAsync(...)</c> drops the returned <c>Task</c> with no exception
    /// handling and no logging — any failure becomes an <c>UnobservedTaskException</c>. Library code
    /// must route fire-and-forget through <c>BOBAsyncHelper.SafeFireAndForget</c>, which catches the
    /// 4-tuple silently and logs anything else.
    /// </summary>
    [Fact]
    public void Library_Should_Not_Use_Underscore_Discard_For_Async_Calls()
    {
        // Matches `_ = SomethingAsync(`. Allows whitespace but not other tokens between `_`, `=`, and the call.
        Regex pattern = new(@"\b_\s*=\s*\w+Async\s*\(", RegexOptions.Compiled);

        List<string> violations = [];

        foreach (string file in EnumerateLibrarySources("*.razor").Concat(EnumerateLibrarySources("*.cs")))
        {
            // BOBAsyncHelper itself is the legitimate sink (`_ = RunSafeAsync(...)` inside the helper).
            if (Path.GetFileName(file).Equals("BOBAsyncHelper.cs", StringComparison.Ordinal))
            {
                continue;
            }

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (pattern.IsMatch(lines[i]))
                {
                    string relative = Path.GetRelativePath(RepoRoot, file);
                    violations.Add($"{relative}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        violations.Should().BeEmpty(
            because: "fire-and-forget async calls must go through BOBAsyncHelper.SafeFireAndForget, " +
                     "which catches the 4-tuple silently and logs anything else. The `_ = FooAsync()` " +
                     "shortcut leaks failures to UnobservedTaskException. See ASYNC-01.\n\nViolations:\n  " +
                     string.Join("\n  ", violations));
    }

    /// <summary>
    /// ASYNC-02. Blazor Server's renderer requires the synchronization context to remain attached
    /// across awaits — <c>ConfigureAwait(false)</c> in library code can detach it and break
    /// downstream <c>StateHasChanged</c> / <c>InvokeAsync</c> calls. The only allowed exception is
    /// <c>BOBAsyncHelper</c>, which already isolates its work behind a fire-and-forget barrier.
    /// </summary>
    [Fact]
    public void Library_Should_Not_Use_ConfigureAwait_False()
    {
        Regex pattern = new(@"\.ConfigureAwait\s*\(", RegexOptions.Compiled);

        List<string> violations = [];

        foreach (string file in EnumerateLibrarySources("*.cs").Concat(EnumerateLibrarySources("*.razor")))
        {
            if (Path.GetFileName(file).Equals("BOBAsyncHelper.cs", StringComparison.Ordinal))
            {
                continue;
            }

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (pattern.IsMatch(lines[i]))
                {
                    string relative = Path.GetRelativePath(RepoRoot, file);
                    violations.Add($"{relative}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        violations.Should().BeEmpty(
            because: "library code must preserve the renderer's synchronization context across awaits. " +
                     "ConfigureAwait(false) detaches it and breaks subsequent StateHasChanged / " +
                     "InvokeAsync. See ASYNC-02.\n\nViolations:\n  " +
                     string.Join("\n  ", violations));
    }

    /// <summary>
    /// ASYNC-03. Any file that catches <c>JSDisconnectedException</c> on a teardown path must catch
    /// the full canonical 4-tuple (<c>JSDisconnectedException</c>, <c>ObjectDisposedException</c>,
    /// <c>InvalidOperationException</c>, <c>TaskCanceledException</c>). Module-load paths may add
    /// <c>JSException</c> on top — that is permitted because <c>JSException</c> is a strict superset
    /// of the 4-tuple constraint.
    ///
    /// The check is file-level: as long as the file has all four exception types referenced inside
    /// catch blocks, the test passes. This catches "I added one and forgot the others" drift.
    /// </summary>
    [Fact]
    public void Files_Catching_JSDisconnectedException_Should_Catch_The_4_Tuple()
    {
        string[] required =
        [
            "JSDisconnectedException",
            "ObjectDisposedException",
            "InvalidOperationException",
            "TaskCanceledException",
        ];

        Regex catchBlock = new(@"catch\s*\(\s*(\w+Exception)", RegexOptions.Compiled);

        List<string> violations = [];

        foreach (string file in EnumerateLibrarySources("*.cs").Concat(EnumerateLibrarySources("*.razor")))
        {
            string content = File.ReadAllText(file);
            HashSet<string> caught = [.. catchBlock.Matches(content).Select(m => m.Groups[1].Value)];

            if (!caught.Contains("JSDisconnectedException"))
            {
                continue;
            }

            string[] missing = [.. required.Where(r => !caught.Contains(r))];
            if (missing.Length > 0)
            {
                string relative = Path.GetRelativePath(RepoRoot, file);
                violations.Add($"{relative}  missing: {string.Join(", ", missing)}");
            }
        }

        violations.Should().BeEmpty(
            because: "any file that catches JSDisconnectedException is on a teardown path and must " +
                     "also catch ObjectDisposedException, InvalidOperationException, and " +
                     "TaskCanceledException — the canonical 4-tuple. Use BOBAsyncHelper.SafeFireAndForget " +
                     "instead of hand-writing teardown catches when the caller can't be async. " +
                     "See ASYNC-03.\n\nViolations:\n  " +
                     string.Join("\n  ", violations));
    }

    // Singular and plural forms are both accepted: the codebase uses plural for the test
    // contexts that produce multiple artifacts per class (Snapshots, Variants), singular
    // for everything else. Disposal / Security / Service are recognized contexts beyond
    // the eight in the original list. Add a new context here when a new test category
    // genuinely warrants it (and document it in ARCHITECTURE.md).
    private static readonly HashSet<string> AllowedTraitContexts = new(StringComparer.Ordinal)
    {
        "Rendering",
        "Snapshot", "Snapshots",
        "State",
        "Interaction",
        "Variant", "Variants",
        "Accessibility",
        "Validation",
        "Integration",
        "Disposal",
        "Security",
        "Service",
    };

    /// <summary>
    /// TEST-TRAIT-01. Every test class under <c>Tests.Components.&lt;Component&gt;.&lt;Class&gt;</c>
    /// must declare at least one <c>[Trait("Component &lt;Context&gt;", "&lt;ComponentName&gt;")]</c>
    /// where <c>&lt;Context&gt;</c> is one of the documented contexts (Rendering / Snapshot / State /
    /// Interaction / Variant / Accessibility / Validation / Integration) and the value matches the
    /// expected component name.
    /// </summary>
    [Fact]
    public void Component_Test_Classes_Should_Use_Component_Context_Trait()
    {
        Assembly testAssembly = typeof(ArchitectureAuditTests).Assembly;

        Type[] componentTestTypes = [.. testAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Namespace?.StartsWith("BlazOrbit.Tests.Integration.Tests.Components.", StringComparison.Ordinal) == true)
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttributes<FactAttribute>().Any() || m.GetCustomAttributes<TheoryAttribute>().Any()))];

        componentTestTypes.Should().NotBeEmpty("there must be component test classes to audit");

        List<string> violations = [];

        foreach (Type type in componentTestTypes)
        {
            TraitAttribute[] traits = [.. type.GetCustomAttributes<TraitAttribute>()];
            TraitAttribute? componentTrait = traits.FirstOrDefault(t =>
                t.Name?.StartsWith("Component ", StringComparison.Ordinal) == true);

            if (componentTrait is null)
            {
                violations.Add($"{type.FullName}  missing [Trait(\"Component <Context>\", \"<ComponentName>\")]");
                continue;
            }

            string context = componentTrait.Name!["Component ".Length..];
            if (!AllowedTraitContexts.Contains(context))
            {
                violations.Add($"{type.FullName}  unknown context '{context}' (allowed: {string.Join(", ", AllowedTraitContexts)})");
                continue;
            }

            if (string.IsNullOrWhiteSpace(componentTrait.Value))
            {
                violations.Add($"{type.FullName}  empty trait value");
            }
        }

        violations.Should().BeEmpty(
            because: "every component test class must be tagged [Trait(\"Component <Context>\", \"<ComponentName>\")] " +
                     "so 'dotnet test --filter \"Component Rendering=BOBButton\"' selects the right slice. " +
                     "See TEST-TRAIT-01.\n\nViolations:\n  " +
                     string.Join("\n  ", violations));
    }

    /// <summary>
    /// CSS-MEDIA-01. Sizing and density use multipliers, not breakpoints. The only public components
    /// allowed to declare <c>@media</c> queries in their scoped CSS are layout components whose
    /// <em>flow</em> changes categorically with viewport size (grid columns collapse, sidebar
    /// activates, toast host repositions). Everything else must use <c>--bob-size-multiplier</c> /
    /// <c>--bob-density-multiplier</c>.
    /// </summary>
    [Fact]
    public void Scoped_Css_Media_Queries_Should_Be_Limited_To_Layout_Exceptions()
    {
        HashSet<string> mediaAllowlist = new(StringComparer.OrdinalIgnoreCase)
        {
            "BOBGrid.razor.css",
            "BOBGridItem.razor.css",
            "BOBSidebarLayout.razor.css",
            "BOBStackedLayout.razor.css",
            "BOBToastHost.razor.css",
        };

        Regex pattern = new(@"@media\b", RegexOptions.Compiled);

        List<string> violations = [];

        foreach (string file in Directory.GetFiles(SrcBlazOrbit, "*.razor.css", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            string fileName = Path.GetFileName(file);
            if (mediaAllowlist.Contains(fileName))
            {
                continue;
            }

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (pattern.IsMatch(lines[i]))
                {
                    string relative = Path.GetRelativePath(SrcBlazOrbit, file);
                    violations.Add($"{relative}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        violations.Should().BeEmpty(
            because: "scoped CSS must scale via --bob-size-multiplier / --bob-density-multiplier, not " +
                     "@media breakpoints. Layout components whose flow changes categorically (grid, " +
                     "sidebar, stacked, toast host) are the documented exception. If a new component " +
                     "needs categorical viewport behavior, add it to the layout family and to the " +
                     "allowlist with a justification. See CSS-MEDIA-01.\n\nViolations:\n  " +
                     string.Join("\n  ", violations));
    }

    /// <summary>
    /// INPUT-BG-01: every variant in the input-family CSS bundle must route
    /// <c>--_wrapper-bg</c> through <c>var(--bob-inline-background, …)</c>. The base block defines
    /// the inline-aware default; variant blocks may override the fallback but must keep the
    /// <c>--bob-inline-background</c> consumer in place. A regression where a variant hardcodes
    /// e.g. <c>--_wrapper-bg: transparent;</c> silently drops the <c>BackgroundColor</c> parameter
    /// for any input-family component rendered with that variant.
    /// </summary>
    [Fact]
    public void Input_Family_Wrapper_Bg_Should_Consume_Inline_Background_Across_All_Variants()
    {
        string bundlePath = Path.Combine(SrcBlazOrbit, "CssBundle", "_input-family.css");
        File.Exists(bundlePath).Should().BeTrue("InputFamilyGenerator must regenerate the bundle before tests run");

        Regex wrapperBgSetter = new(@"--_wrapper-bg:\s*([^;]+);", RegexOptions.Compiled);
        string content = File.ReadAllText(bundlePath);

        List<string> violations = [];
        foreach (Match m in wrapperBgSetter.Matches(content))
        {
            string rhs = m.Groups[1].Value.Trim();
            if (!rhs.Contains("--bob-inline-background", StringComparison.Ordinal))
            {
                int line = content[..m.Index].Count(c => c == '\n') + 1;
                violations.Add($"_input-family.css:{line}  --_wrapper-bg: {rhs};");
            }
        }

        violations.Should().BeEmpty(
            because: "every assignment to --_wrapper-bg in the input-family bundle must consume " +
                     "--bob-inline-background so the BackgroundColor parameter applies regardless of " +
                     "variant. Wrap the literal in `var(--bob-inline-background, <fallback>)`. " +
                     "See INPUT-BG-01.\n\nViolations:\n  " +
                     string.Join("\n  ", violations));
    }

    /// <summary>
    /// INPUT-COLOR-01: the input-family CSS bundle must keep its <c>--bob-inline-color</c>
    /// consumer on <c>.bob-input__field</c> so the <c>Color</c> parameter paints the native
    /// <c>&lt;input&gt;</c> / <c>&lt;textarea&gt;</c> text. The rule has specificity (0,2,1)
    /// and overrides the user-agent default; if a refactor drops the consumer, the parameter
    /// becomes silently inert.
    /// </summary>
    [Fact]
    public void Input_Family_Field_Should_Consume_Inline_Color()
    {
        string bundlePath = Path.Combine(SrcBlazOrbit, "CssBundle", "_input-family.css");
        File.Exists(bundlePath).Should().BeTrue("InputFamilyGenerator must regenerate the bundle before tests run");

        string content = File.ReadAllText(bundlePath);

        Regex fieldColor = new(
            @"bob-component\[data-bob-input-base\][^{]*\.bob-input__field\b[^{]*\{[^}]*?color:\s*var\(--bob-inline-color",
            RegexOptions.Compiled | RegexOptions.Singleline);

        fieldColor.IsMatch(content).Should().BeTrue(
            because: "the input-family bundle must consume --bob-inline-color on .bob-input__field " +
                     "so the Color parameter paints the native <input>/<textarea> text. " +
                     "See INPUT-COLOR-01.");
    }

    /// <summary>
    /// CACHE-PURE-01: every type implementing <c>IPureBuiltComponent</c> must keep its
    /// <c>BuildComponentDataAttributes</c> and <c>BuildComponentCssVariables</c> hooks free of
    /// instance-field reads. The marker promises the hooks read only <c>[Parameter]</c> properties
    /// or pure getters; if a field crept in, the style-fingerprint cache would freeze the stale
    /// value and the component would render out of date.
    ///
    /// Heuristic: scan the hook bodies for identifiers matching <c>_[a-z]\w*</c> (the project's
    /// convention for private fields). False positives go in <c>PureFieldReadAllowlist</c> with a
    /// one-line justification.
    /// </summary>
    [Fact]
    public void Pure_Built_Components_Should_Not_Read_Instance_Fields_In_Hooks()
    {
        // Force-load the BlazOrbit assembly so AppDomain.CurrentDomain.GetAssemblies()
        // sees the Razor-generated implementer types. The Core assembly (where IPureBuiltComponent
        // lives) is already loaded by referencing the interface; BlazOrbit may not be unless
        // a test elsewhere in this run already touched it.
        _ = Assembly.Load("BlazOrbit");

        // Pull all source files (.razor + .cs) and prebuild a name → path map.
        Dictionary<string, string> sourceByTypeName = new(StringComparer.Ordinal);
        foreach (string razor in EnumerateLibrarySources("*.razor"))
        {
            sourceByTypeName[Path.GetFileNameWithoutExtension(razor)] = razor;
        }
        foreach (string cs in EnumerateLibrarySources("*.cs"))
        {
            string name = Path.GetFileNameWithoutExtension(cs);
            // Strip the generic-arity suffix some .cs files use in name (none today, but defensive).
            sourceByTypeName.TryAdd(name, cs);
        }

        Type[] pureTypes = [..
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name?.StartsWith("BlazOrbit", StringComparison.Ordinal) == true)
                .Where(a => a.GetName().Name?.Contains(".Tests", StringComparison.Ordinal) != true)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return []; }
                })
                .Where(t => typeof(BlazOrbit.Components.IPureBuiltComponent).IsAssignableFrom(t)
                            && t.IsClass && t != typeof(BlazOrbit.Components.IPureBuiltComponent))];

        pureTypes.Should().NotBeEmpty(
            "the migration to IPureBuiltComponent must yield at least one type (no implementers found)");

        // Strip the generic-arity ` suffix Type.Name uses ("BOBDataCollectionBase`3" → "BOBDataCollectionBase").
        static string TypeName(Type t)
        {
            int tick = t.Name.IndexOf('`');
            return tick >= 0 ? t.Name[..tick] : t.Name;
        }

        Regex hookOpener = new(@"public\s+(virtual\s+)?void\s+BuildComponent(DataAttributes|CssVariables)\s*\(",
            RegexOptions.Compiled);
        Regex fieldRead = new(@"\b_[a-z]\w*\b", RegexOptions.Compiled);
        // String literals contain CSS variable names like "--_accordion-gap" that match the
        // field-read regex but are payloads, not C# identifiers. Strip them before scanning.
        // Handles "..." with escapes; interpolated `$"..."` and verbatim `@"..."` are folded the same way.
        Regex stringLiteral = new(@"@?\$?""([^""\\]|\\.)*""", RegexOptions.Compiled);

        List<string> violations = [];

        foreach (Type t in pureTypes.OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            string name = TypeName(t);
            if (!sourceByTypeName.TryGetValue(name, out string? path))
            {
                violations.Add($"{t.FullName}  source file not found (looked for {name}.razor / {name}.cs)");
                continue;
            }

            string content = File.ReadAllText(path);
            int searchStart = 0;
            while (true)
            {
                Match opener = hookOpener.Match(content, searchStart);
                if (!opener.Success) break;

                // Find the matching closing brace by simple counting from the first `{` after the opener.
                int braceStart = content.IndexOf('{', opener.Index + opener.Length);
                if (braceStart < 0) break;
                int depth = 1;
                int i = braceStart + 1;
                while (i < content.Length && depth > 0)
                {
                    if (content[i] == '{') depth++;
                    else if (content[i] == '}') depth--;
                    i++;
                }
                int braceEnd = i;
                string body = content[(braceStart + 1)..(braceEnd - 1)];
                string scanBody = stringLiteral.Replace(body, "\"\"");

                foreach (Match fr in fieldRead.Matches(scanBody))
                {
                    // Allow `_` alone (discard) and the contribution-dict locals if any.
                    string ident = fr.Value;
                    if (ident == "_") continue;
                    string relative = Path.GetRelativePath(RepoRoot, path);
                    violations.Add($"{relative}  {t.Name}.BuildComponent{opener.Groups[2].Value} reads `{ident}`");
                }

                searchStart = braceEnd;
            }
        }

        violations.Should().BeEmpty(
            because: "IPureBuiltComponent hooks must not read instance fields — the cache folds the " +
                     "hook output into the fingerprint, so a non-parameter field would silently freeze " +
                     "the stale value. Either remove the field read, or drop the IPureBuiltComponent " +
                     "marker (revert to plain IBuiltComponent) to opt back out of the cache. " +
                     "See CACHE-PURE-01.\n\nViolations:\n  " +
                     string.Join("\n  ", violations));
    }
}
