using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Variant", "Charts.ColorStability")]
public class ColorStabilityTests
{
    private static IEnumerable<BOBChartSeries<int, double>> ThreeLineSeries() =>
    [
        new BOBChartSeries<int, double>
        {
            Label = "A",
            Points = Enumerable.Range(1, 5).Select(i => new BOBChartPoint<int, double>(i, i * 1.0)).ToArray()
        },
        new BOBChartSeries<int, double>
        {
            Label = "B",
            Points = Enumerable.Range(1, 5).Select(i => new BOBChartPoint<int, double>(i, i * 2.0)).ToArray()
        },
        new BOBChartSeries<int, double>
        {
            Label = "C",
            Points = Enumerable.Range(1, 5).Select(i => new BOBChartPoint<int, double>(i, i * 3.0)).ToArray()
        }
    ];

    private static string SeriesStrokeColor(IRenderedComponent<BOBLineChart<int, double>> cut, string label) =>
        cut.FindAll("g.bob-line-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == label)
            .QuerySelector("path.bob-line-chart__line")!
            .GetAttribute("stroke") ?? string.Empty;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task LineChart_Hiding_FirstSeries_Should_Keep_Other_Colors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, ThreeLineSeries()));

        string colorBBefore = SeriesStrokeColor(cut, "B");
        string colorCBefore = SeriesStrokeColor(cut, "C");

        // Toggle the first legend chip → "A" hidden.
        cut.Find("button.bob-chart__legend-button").Click();

        string colorBAfter = SeriesStrokeColor(cut, "B");
        string colorCAfter = SeriesStrokeColor(cut, "C");

        colorBAfter.Should().Be(colorBBefore,
            "B keeps its palette slot independent of A's visibility");
        colorCAfter.Should().Be(colorCBefore,
            "C keeps its palette slot independent of A's visibility");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task LineChart_Hiding_MiddleSeries_Should_Keep_Other_Colors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, ThreeLineSeries()));

        string colorABefore = SeriesStrokeColor(cut, "A");
        string colorCBefore = SeriesStrokeColor(cut, "C");

        // Click the second legend button → "B" hidden.
        cut.FindAll("button.bob-chart__legend-button")[1].Click();

        SeriesStrokeColor(cut, "A").Should().Be(colorABefore);
        SeriesStrokeColor(cut, "C").Should().Be(colorCBefore);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BarChart_Hiding_FirstSeries_Should_Keep_Other_Colors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IEnumerable<BOBChartSeries<string, decimal>> series =
        [
            new BOBChartSeries<string, decimal>
            {
                Label = "A", Points = [new BOBChartPoint<string, decimal>("Q1", 10m)]
            },
            new BOBChartSeries<string, decimal>
            {
                Label = "B", Points = [new BOBChartPoint<string, decimal>("Q1", 20m)]
            },
            new BOBChartSeries<string, decimal>
            {
                Label = "C", Points = [new BOBChartPoint<string, decimal>("Q1", 30m)]
            }
        ];

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p.Add(c => c.Series, series));

        string fillBBefore = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "B")
            .QuerySelector("rect.bob-bar-chart__bar")!.GetAttribute("fill") ?? string.Empty;

        cut.Find("button.bob-chart__legend-button").Click();

        string fillBAfter = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "B")
            .QuerySelector("rect.bob-bar-chart__bar")!.GetAttribute("fill") ?? string.Empty;

        fillBAfter.Should().Be(fillBBefore);
    }
}
