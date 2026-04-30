using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Snapshots", "BOBDrawer")]
public class BOBDrawerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Closed",
                Html = ctx.Render<BOBDrawer>(p => p
                    .Add(c => c.Open, false)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Open_Right_Default",
                Html = ctx.Render<BOBDrawer>(p => p
                    .Add(c => c.Open, true)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "Drawer body"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Open_Left_Closable_With_Header",
                Html = ctx.Render<BOBDrawer>(p => p
                    .Add(c => c.Open, true)
                    .Add(c => c.Position, DrawerPosition.Left)
                    .Add(c => c.Closable, true)
                    .Add(c => c.Header, b => b.AddContent(0, "Drawer title"))
                    .Add(c => c.ChildContent, b => b.AddContent(0, "Drawer body"))).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
