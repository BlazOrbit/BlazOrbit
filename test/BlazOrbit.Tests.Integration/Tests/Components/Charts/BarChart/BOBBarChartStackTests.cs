using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.BarChart;

[Trait("Component Variant", "BOBBarChart.Stacked")]
public class BOBBarChartStackTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> ThreeSeries() =>
    [
        new BOBChartSeries<string, decimal>
        {
            Label = "EMEA",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 30m),
                    new BOBChartPoint<string, decimal>("Q2", 50m)
            ]
        },
        new BOBChartSeries<string, decimal>
        {
            Label = "Americas",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 20m), new BOBChartPoint<string, decimal>("Q2", 40m)
            ]
        },
        new BOBChartSeries<string, decimal>
        {
            Label = "APAC",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 50m), new BOBChartPoint<string, decimal>("Q2", 10m)
            ]
        }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Default_Should_Be_None_StackMode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, ThreeSeries()));

        cut.Instance.StackMode.Should().Be(BOBBarStackMode.None);
        // 3 series × 2 categories = 6 bars side-by-side.
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(6);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Should_Render_Same_Number_Of_Rects(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.StackMode, BOBBarStackMode.Stacked));

        // Same 6 rects but stacked vertically per category instead of side-by-side.
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(6);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Bars_In_Same_Category_Should_Share_X(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.StackMode, BOBBarStackMode.Stacked));

        // The 3 bars at category Q1 (first across each series) share the
        // same x and the same width (full groupWidth — not split).
        IReadOnlyList<AngleSharp.Dom.IElement> q1Bars =
        [
            cut.FindAll("rect.bob-bar-chart__bar")[0], // EMEA Q1
            cut.FindAll("rect.bob-bar-chart__bar")[2], // Americas Q1
            cut.FindAll("rect.bob-bar-chart__bar")[4] // APAC Q1
        ];

        string firstX = q1Bars[0].GetAttribute("x") ?? string.Empty;
        string firstW = q1Bars[0].GetAttribute("width") ?? string.Empty;
        foreach (AngleSharp.Dom.IElement bar in q1Bars.Skip(1))
        {
            bar.GetAttribute("x").Should().Be(firstX);
            bar.GetAttribute("width").Should().Be(firstW);
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Should_Stack_Vertically_Without_Gaps(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.StackMode, BOBBarStackMode.Stacked));

        // Bars within Q1 (indices 0, 2, 4): each next series starts where the
        // previous ended — so previous.y > next.y (in pixel space, smaller y
        // = higher up since SVG Y inverts).
        AngleSharp.Dom.IElement first = cut.FindAll("rect.bob-bar-chart__bar")[0];
        AngleSharp.Dom.IElement second = cut.FindAll("rect.bob-bar-chart__bar")[2];

        double y1 = double.Parse(first.GetAttribute("y")!, System.Globalization.CultureInfo.InvariantCulture);
        double h1 = double.Parse(first.GetAttribute("height")!, System.Globalization.CultureInfo.InvariantCulture);
        double y2 = double.Parse(second.GetAttribute("y")!, System.Globalization.CultureInfo.InvariantCulture);
        double h2 = double.Parse(second.GetAttribute("height")!, System.Globalization.CultureInfo.InvariantCulture);

        // y1 ends at y1+h1 (bottom of the first segment).
        // y2 ends at y2+h2 (bottom of the second segment).
        // Stack contiguously: top of first = bottom of second (within rounding).
        (y2 + h2).Should().BeApproximately(y1, 0.5,
            "the segment above starts where the lower segment's top ends");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PercentStacked_Should_Fill_Plot_Area(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.StackMode, BOBBarStackMode.PercentStacked));

        // Sum of the 3 segments at Q1 should span the full plot height
        // (PlotTop=12 → PlotBottom=PlotHeight-32 with default 600×400 layout
        // gives PlotHeight=356). Total stacked height ≈ 356.
        double q1 = new[] { 0, 2, 4 }
            .Select(i => cut.FindAll("rect.bob-bar-chart__bar")[i])
            .Select(e => double.Parse(e.GetAttribute("height")!, System.Globalization.CultureInfo.InvariantCulture))
            .Sum();

        q1.Should().BeApproximately(356, 1,
            "PercentStacked normalises every category to fill the full plot height");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PercentStacked_Should_Render_Y_Axis_0_to_100(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.StackMode, BOBBarStackMode.PercentStacked));

        IEnumerable<string> ticks = cut.FindAll(".bob-bar-chart__axis--y text").Select(t => t.TextContent);
        ticks.Should().Contain("0").And.Contain("100");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stacked_Should_Skip_Hidden_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, ThreeSeries())
                .Add(c => c.StackMode, BOBBarStackMode.Stacked));

        // Toggle EMEA via legend → only 2 series stacked (Americas + APAC).
        cut.Find("button.bob-chart__legend-button").Click();

        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(4,
            "2 visible series × 2 categories = 4 segments");
    }
}