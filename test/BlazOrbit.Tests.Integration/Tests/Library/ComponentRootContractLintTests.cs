using FluentAssertions;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// COMP-LINT-01: detecta componentes públicos que participan en el design system BOB
/// (heredan de BOBComponentBase/BOBInputComponentBase/BOBVariantComponentBase/BOBDataCollectionBase)
/// pero no renderizan &lt;bob-component&gt; como root. Los modales, helpers internos, layouts
/// y nodos de árbol se excluyen automáticamente porque no heredan de la familia BOB o
/// su clase base directa controla el render tree.
/// </summary>
[Trait("Library", "ComponentContract")]
public class ComponentRootContractLintTests
{
    private static readonly string SrcBlazOrbit = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..",
        "src", "BlazOrbit"));

    [Fact]
    public void Components_Deriving_From_BOBComponentBase_Family_Should_Have_BobComponent_Root()
    {
        string[] razorFiles = Directory.GetFiles(SrcBlazOrbit, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(f => Path.GetFileName(f) != "_Imports.razor")
            .ToArray();

        razorFiles.Should().NotBeEmpty();

        List<string> violations = [];

        foreach (string file in razorFiles)
        {
            string content = File.ReadAllText(file);
            string fileName = Path.GetFileName(file);

            // Skip internal components (underscore prefix)
            if (fileName.StartsWith('_'))
            {
                continue;
            }

            // Extract @inherits declaration
            Match inheritsMatch = Regex.Match(content, @"@inherits\s+(\S+)");
            if (!inheritsMatch.Success)
            {
                continue; // Default ComponentBase — not part of BOB design system
            }

            string baseClass = inheritsMatch.Groups[1].Value;

            // Only components that participate in the BOB design system
            bool participatesInBOB =
                baseClass.Contains("BOBComponentBase") ||
                baseClass.Contains("BOBInputComponentBase") ||
                baseClass.Contains("BOBVariantComponentBase") ||
                baseClass.Contains("BOBDataCollectionBase");

            if (!participatesInBOB)
            {
                continue;
            }

            // Exclusion: BOBTreeNodeBase controls the render tree for its children.
            // The container (BOBTreeMenu/BOBTreeSelector) owns the <bob-component> root;
            // individual nodes are rendered as children inside the tree structure.
            bool baseControlsMarkup = baseClass.Contains("BOBTreeNodeBase");
            if (baseControlsMarkup)
            {
                continue;
            }

            // Verify the contract
            if (!content.Contains("<bob-component"))
            {
                violations.Add($"{fileName}: inherits {baseClass} but missing <bob-component> root");
            }
        }

        violations.Should().BeEmpty(
            because: "all components that participate in the BOB design system must render " +
                     "<bob-component> as their root element. See CLAUDE.md §Component architecture.\n\n" +
                     string.Join("\n", violations));
    }
}
