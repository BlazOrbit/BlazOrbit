using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// CSS-SCOPED-09 (mecanizado): verifies that every public component ships a scoped
/// CSS file (.razor.css) unless it is explicitly allow-listed as CSS-free.
///
/// A component without scoped CSS either:
///   (a) is a pure logic container / host with no visual surface of its own, or
///   (b) accidentally forgot to add the .razor.css file.
///
/// This test prevents case (b) from going unnoticed.
/// </summary>
[Trait("Library", "CssAudit")]
public class CssScopedAuditTests
{
    private static readonly string SrcBlazOrbit = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..",
        "src", "BlazOrbit"));

    /// <summary>
    /// Components that intentionally have no scoped CSS because they are pure
    /// containers, hosts, or internal helpers.
    /// </summary>
    private static readonly HashSet<string> CssFreeAllowlist = new(StringComparer.Ordinal)
    {
        // Internal helpers — no public visual surface
        "_BOBCheckMark",
        "_BOBFieldHelper",
        "_BOBInputLoading",
        "_BOBInputOutline",
        "_BOBInputPrefix",
        "_BOBInputSuffix",

        // Hosts / containers — visual surface is rendered by children
        "BOBModalHost",
        "BOBModalContainer",
        "BOBInitializer",
        "BOBBlazorLayout",

        // Input-family helpers — styled by global _input-family.css
        "BOBInputLoading",
        "BOBInputOutline",
        "BOBInputPrefix",
        "BOBInputSuffix",

        // Components whose styling is fully family-driven or render inside another component
        "BOBTab",
        "BOBAccordionItem",
        "BOBCarouselItem",
        "BOBTreeMenuItem",
        "BOBTreeSelectorItem",
        "BOBDataColumn",
        "BOBPerformanceDashboard",
        "BOBInputDropdown",
        "BOBInputDropdownTree",
        "BOBDropdownContainer",
        "BOBDateTimePattern",
    };

    [Fact]
    public void Public_Components_Should_Have_Scoped_Css_File()
    {
        string[] razorFiles = Directory.GetFiles(SrcBlazOrbit, "*.razor", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).StartsWith("_"))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        razorFiles.Should().NotBeEmpty("there must be public .razor files to audit");

        List<string> violations = [];

        foreach (string razor in razorFiles)
        {
            string name = Path.GetFileNameWithoutExtension(razor);

            // Skip non-components and allow-listed entries
            if (name == "_Imports" || CssFreeAllowlist.Contains(name))
            {
                continue;
            }

            string cssPath = Path.ChangeExtension(razor, ".razor.css");
            if (!File.Exists(cssPath))
            {
                violations.Add(name);
            }
        }

        violations.Should().BeEmpty(
            because: "every public component with a visual surface must ship a scoped .razor.css file. " +
                     "If a component intentionally has no CSS, add it to CssFreeAllowlist with a justification. " +
                     "See CSS-SCOPED-09.\n\nMissing .razor.css files:\n  " +
                     string.Join("\n  ", violations));
    }
}
