using FluentAssertions;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Core;

/// <summary>
/// CSS-SCOPED-LINT-01: BEM state modifiers must not appear on component roots.
///
/// Per COMP-STATE-CLASS-01 decision (option b), BEM modifiers like --selected,
/// --focused, --active, --disabled are acceptable on child elements inside a
/// component, but never on the root &lt;bob-component&gt; or its canonical
/// [data-bob-component="..."] selector. Root state is expressed exclusively
/// via data-bob-* attributes emitted by BOBComponentAttributesBuilder.
/// </summary>
[Trait("Core", "CssLint")]
public class CssArchitectureLintTests
{
    private static readonly string[] ExcludedFiles =
    [
        // Internal components — outside public contract
        "_BOB",
        // Hosts — documented exceptions in CSS-SCOPED-04
        "BOBModalHost",
        "BOBModalContainer",
        "BOBThemeGenerator",
    ];

    /// <summary>
    /// Verifies that no public component's scoped CSS uses a BEM modifier
    /// on the canonical root selector ([data-bob-component="..."] or
    /// bob-component[data-bob-component="..."]).
    /// </summary>
    [Fact]
    public void RazorCss_Should_Not_Use_Bem_State_Modifiers_On_Root()
    {
        string srcPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "src", "BlazOrbit");

        srcPath = Path.GetFullPath(srcPath);

        string[] cssFiles = Directory.GetFiles(srcPath, "*.razor.css", SearchOption.AllDirectories);
        cssFiles.Should().NotBeEmpty("there must be scoped CSS files to lint");

        List<string> violations = [];

        // Matches root selectors with BEM modifiers:
        //   [data-bob-component="foo"]--selected
        //   bob-component[data-bob-component="foo"]--selected
        // We intentionally do NOT match .bob-block__element--modifier on
        // descendants — those are permitted by COMP-STATE-CLASS-01 option b.
        Regex rootModifierPattern = new(
            @"^\s*(\[data-bob-component=""[^""]+""\]|bob-component\[data-bob-component=""[^""]+""\])--[\w-]+",
            RegexOptions.Multiline | RegexOptions.Compiled);

        foreach (string file in cssFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (ExcludedFiles.Any(ex => fileName.Contains(ex, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match match in rootModifierPattern.Matches(content))
            {
                violations.Add($"{fileName}: {match.Value.Trim()}");
            }
        }

        violations.Should().BeEmpty(
            because: "root state must be expressed via data-bob-* attributes, never BEM modifiers. " +
                     "Decision: COMP-STATE-CLASS-01 option b (BEM modifiers acceptable on children, not on root).");
    }

    /// <summary>
    /// A11Y-02: the global CSS bundle must declare a `@media (prefers-reduced-motion: reduce)`
    /// override that neutralizes animation/transition durations and `scroll-behavior`. WCAG 2.2 AA
    /// 2.3.3 Animation from Interactions.
    /// </summary>
    [Fact]
    public void Base_Css_Should_Declare_Prefers_Reduced_Motion_Override()
    {
        string baseCssPath = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "src", "BlazOrbit", "CssBundle", "_base.css"));

        File.Exists(baseCssPath).Should().BeTrue(
            because: "BaseComponentGenerator must regenerate _base.css before tests run");

        string content = File.ReadAllText(baseCssPath);

        content.Should().Contain("@media (prefers-reduced-motion: reduce)");
        content.Should().MatchRegex(@"animation-duration:\s*0\.01ms\s*!important");
        content.Should().MatchRegex(@"transition-duration:\s*0\.01ms\s*!important");
        content.Should().MatchRegex(@"scroll-behavior:\s*auto\s*!important");
    }
}
