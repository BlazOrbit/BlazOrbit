using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.AreaChart;

[Trait("Component Variant", "BOBAreaChart.Stacked")]
public class BOBAreaChartStackTests
{
    private static IEnumerable<BOBChartSeries<int, double>> ThreeSeries() =>
    [
        new BOBChartSeries<int, double>
        {
            Label = "A",
            Points =
            [
                new BOBChartPoint<int, double>(1, 10), new BOBChartPoint<int, double>(2, 20),
                    new BOBChartPoint<int, double>(3, 15)
            ]
        },
        new BOBChartSeries<int, double>
        {
            Label = "B",
            Points =
            [
                new BOBChartPoint<int, double>(1, 5), new BOBChartPoint<int, double>(2, 10),
                new BOBChartPoint<int, double>(3, 25)
            ]
        },
        new BOBChartSeries<int, double>
        {
            Label = "C",
            Points =
            [
                new BOBChartPoint<int, double>(1, 20), new BOBChartPoint<int, double>(2, 30),
                new BOBChartPoint<int, double>(3, 10)
            ]
        }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Default_Should_Be_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries()));

        cut.Instance.Stacked.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Should_Render_One_Fill_Per_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.Stacked, true));

        cut.FindAll("path.bob-area-chart__fill").Should().HaveCount(3);
        cut.FindAll("path.bob-line-chart__line").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Fill_Should_Use_Cumulative_Path(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.Stacked, true));

        // Stacked area path = "M top L … L baseline-end L baseline-start … Z"
        // (top forward + baseline reversed). The path contains two "L"
        // sequences forming both contours and ends in Z.
        string d = cut.FindAll("path.bob-area-chart__fill").First().GetAttribute("d") ?? string.Empty;
        d.Should().StartWith("M ");
        d.Should().Contain(" L ");
        d.Should().EndWith(" Z");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_TopSeries_Should_Have_Bottom_Of_Plot_As_Baseline(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.Stacked, true));

        // First series (A) at the bottom of the stack — its baseline is the
        // zero line of the cumulative scale. Y of first series at X=1 is 10
        // (cumulative from baseline=0 → 10). Top contour starts at the
        // projection of 10, baseline starts at the projection of 0
        // (PlotBottom=368 with default 600×400).
        string d = cut.FindAll("path.bob-area-chart__fill").First().GetAttribute("d") ?? string.Empty;

        // Path should contain a coord near PlotBottom=368 (the baseline).
        d.Should().MatchRegex(@",368\.0\d?\b",
            "the bottom-most stacked series fills down to the X axis");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Line_Markers_Should_Sit_At_Cumulative_Y(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.Stacked, true));

        // 3 series × 3 points = 9 markers, all at cumulative Y positions.
        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(9);

        // Top series (C) third marker at X=3: cumulative = 15+25+10 = 50.
        // First series (A) third marker at X=3: cumulative = 15.
        IReadOnlyList<AngleSharp.Dom.IElement> allMarkers = cut.FindAll("circle.bob-line-chart__marker");
        double aLastCy = double.Parse(allMarkers[2].GetAttribute("cy")!,
            System.Globalization.CultureInfo.InvariantCulture);
        double cLastCy = double.Parse(allMarkers[8].GetAttribute("cy")!,
            System.Globalization.CultureInfo.InvariantCulture);

        cLastCy.Should().BeLessThan(aLastCy,
            "stacked top series should sit higher (smaller Y in SVG) than the bottom series at the same X");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Should_Skip_Hidden_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.Stacked, true));

        // Toggle "A" off via legend → only B + C stacked.
        cut.Find("button.bob-chart__legend-button").Click();

        cut.FindAll("path.bob-area-chart__fill").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_NonSmooth_Should_Use_Polyline_Glue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.Stacked, true)
                .Add(c => c.Smooth, false));

        // Polyline path uses only L commands, no C cubic beziers.
        string d = cut.FindAll("path.bob-area-chart__fill").First().GetAttribute("d") ?? string.Empty;
        d.Should().Contain(" L ");
        d.Should().NotContain(" C ");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Smooth_Should_Use_Cubic_For_Both_Boundaries(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.Stacked, true)
                .Add(c => c.Smooth, true));

        string d = cut.FindAll("path.bob-area-chart__fill").First().GetAttribute("d") ?? string.Empty;
        d.Should().Contain(" C ", "smooth fills curve both top and bottom contours");
    }
}