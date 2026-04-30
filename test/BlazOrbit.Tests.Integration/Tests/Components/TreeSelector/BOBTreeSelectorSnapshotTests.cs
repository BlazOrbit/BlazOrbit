using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeSelector;

[Trait("Component Snapshots", "BOBTreeSelector")]
public class BOBTreeSelectorSnapshotTests
{
    private sealed record SelectItem(string Key, string Label, IEnumerable<SelectItem>? Children = null);

    private static IEnumerable<SelectItem> TreeItems
    => [
        new SelectItem("fruits", "Fruits", [
            new SelectItem("apple", "Apple"),
            new SelectItem("banana", "Banana"),
        ]),
        new SelectItem("vegs", "Vegetables"),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Collapsed_NoSelection",
                Html = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
                    .Add(c => c.Items, TreeItems)
                    .Add(c => c.KeySelector, m => m.Key)
                    .Add(c => c.ChildrenSelector, m => m.Children)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Expanded_WithSelection",
                Html = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
                    .Add(c => c.Items, TreeItems)
                    .Add(c => c.KeySelector, m => m.Key)
                    .Add(c => c.ChildrenSelector, m => m.Children)
                    .Add(c => c.ExpandAll, true)
                    .Add(c => c.SelectedKeys, ["apple"])
                    .Add(c => c.SelectionMode, TreeSelectionMode.Multiple)).GetNormalizedMarkup()
            },
            new
            {
                Name = "NoCheckboxes",
                Html = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
                    .Add(c => c.Items, TreeItems)
                    .Add(c => c.KeySelector, m => m.Key)
                    .Add(c => c.ChildrenSelector, m => m.Children)
                    .Add(c => c.ShowCheckboxes, false)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
