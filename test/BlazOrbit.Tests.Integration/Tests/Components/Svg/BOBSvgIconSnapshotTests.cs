using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Svg;

[Trait("Component Snapshots", "BOBSvgIcon")]
public class BOBSvgIconSnapshotTests
{
    private static readonly IconKey TriangleIcon = new("TriangleIcon") { SvgContent = "<path d=\"M12 2L2 22h20L12 2z\"/>" };
    private static readonly IconKey CircleIcon = new("CircleIcon") { SvgContent = "<circle cx=\"12\" cy=\"12\" r=\"10\"/>" };
    private static readonly IconKey StarIcon = new("StarIcon") { SvgContent = "<path d=\"M12 2l3 6 7 1-5 5 1 7-6-3-6 3 1-7-5-5 7-1z\"/>" };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Default_Triangle",
                Html = ctx.Render<BOBSvgIcon>(p => p
                    .Add(c => c.Icon, TriangleIcon)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Circle_With_Title",
                Html = ctx.Render<BOBSvgIcon>(p => p
                    .Add(c => c.Icon, CircleIcon)
                    .Add(c => c.Title, "Circle icon")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Star_Large",
                Html = ctx.Render<BOBSvgIcon>(p => p
                    .Add(c => c.Icon, StarIcon)
                    .Add(c => c.Size, BOBSize.Large)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Custom_ViewBox",
                Html = ctx.Render<BOBSvgIcon>(p => p
                    .Add(c => c.Icon, TriangleIcon)
                    .Add(c => c.ViewBox, "0 0 48 48")).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
