using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Snapshots", "BOBTreeMenu")]
public class BOBTreeMenuSnapshotTests
{
    private sealed record MenuItem(string Key, string Label, IEnumerable<MenuItem>? Children = null);

    private static IEnumerable<MenuItem> TreeItems
    => [
        new MenuItem("file", "File", [
            new MenuItem("new", "New"),
            new MenuItem("open", "Open"),
        ]),
        new MenuItem("edit", "Edit"),
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
                Name = "Collapsed",
                Html = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
                    .Add(c => c.Items, TreeItems)
                    .Add(c => c.KeySelector, m => m.Key)
                    .Add(c => c.ChildrenSelector, m => m.Children)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Expanded",
                Html = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
                    .Add(c => c.Items, TreeItems)
                    .Add(c => c.KeySelector, m => m.Key)
                    .Add(c => c.ChildrenSelector, m => m.Children)
                    .Add(c => c.ExpandAll, true)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Horizontal",
                Html = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
                    .Add(c => c.Items, TreeItems)
                    .Add(c => c.KeySelector, m => m.Key)
                    .Add(c => c.ChildrenSelector, m => m.Children)
                    .Add(c => c.Orientation, TreeMenuOrientation.Horizontal)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
