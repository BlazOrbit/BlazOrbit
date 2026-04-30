using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Grid;

[Trait("Component Snapshots", "BOBGridItem")]
public class BOBGridItemSnapshotTests
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
                Name = "Default_Auto",
                Html = ctx.Render<BOBGridItem>(p => p
                    .Add(c => c.Auto, true)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "Auto"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Span_6",
                Html = ctx.Render<BOBGridItem>(p => p
                    .Add(c => c.Span, 6)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "Half width"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Responsive_Breakpoints",
                Html = ctx.Render<BOBGridItem>(p => p
                    .Add(c => c.Xs, 12)
                    .Add(c => c.Md, 6)
                    .Add(c => c.Lg, 4)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "Responsive"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Spacing_And_Order",
                Html = ctx.Render<BOBGridItem>(p => p
                    .Add(c => c.Span, 4)
                    .Add(c => c.Order, 2)
                    .Add(c => c.P, "1rem")
                    .Add(c => c.AlignSelf, GridAlignSelf.Center)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "With spacing"))).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
