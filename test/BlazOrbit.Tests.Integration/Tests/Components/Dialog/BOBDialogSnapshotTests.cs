using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Snapshots", "BOBDialog")]
public class BOBDialogSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Dialog_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BOBDialog>> Builder)[] testCases =
        [
            ("Closed", p => p
                .Add(c => c.Open, false)),
            ("Open_NoContent", p => p
                .Add(c => c.Open, true)),
            ("Open_WithTitle", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Title, "Dialog Title")
                .Add(c => c.Content, b => b.AddContent(0, "Body text"))),
            ("Open_WithFooter", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Content, b => b.AddContent(0, "Body"))
                .Add(c => c.Footer, b => b.AddContent(0, "OK | Cancel"))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Drawer_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BOBDrawer>> Builder)[] testCases =
        [
            ("Closed", p => p
                .Add(c => c.Open, false)),
            ("Open_Right", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Position, DrawerPosition.Right)
                .Add(c => c.ChildContent, b => b.AddContent(0, "Drawer content"))),
            ("Open_Left", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Position, DrawerPosition.Left)
                .Add(c => c.ChildContent, b => b.AddContent(0, "Left drawer"))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
