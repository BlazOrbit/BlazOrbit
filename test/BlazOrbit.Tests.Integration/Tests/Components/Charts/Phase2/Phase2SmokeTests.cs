using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Phase2;

[Trait("Component Rendering", "Charts.Other")]
public class Phase2SmokeTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Histogram_Should_Render_Bins(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBHistogramChart<double>> cut =
            ctx.Render<BOBHistogramChart<double>>(p => p
                .Add(c => c.Values, Enumerable.Range(0, 100).Select(i => (double)i)));
        cut.FindAll("rect.bob-histogram-chart__bar").Count.Should().BeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Candlestick_Should_Render_Bodies_And_Wicks(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBCandlestickChart<int>> cut =
            ctx.Render<BOBCandlestickChart<int>>(p => p
                .Add(c => c.Candles, new[]
                {
                    new BOBChartCandlePoint<int>(1, Open: 100, High: 110, Low: 95, Close: 108),
                    new BOBChartCandlePoint<int>(2, Open: 108, High: 112, Low: 102, Close: 104),
                    new BOBChartCandlePoint<int>(3, Open: 104, High: 115, Low: 100, Close: 113),
                }));
        cut.FindAll("rect.bob-candle-chart__body").Should().HaveCount(3);
        cut.FindAll("line.bob-candle-chart__wick").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Funnel_Should_Render_One_Path_Per_Step(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBFunnelChart<int>> cut =
            ctx.Render<BOBFunnelChart<int>>(p => p
                .Add(c => c.Steps, new[]
                {
                    new BOBChartFunnelStep<int> { Label = "Visits", Value = 1000 },
                    new BOBChartFunnelStep<int> { Label = "Signups", Value = 400 },
                    new BOBChartFunnelStep<int> { Label = "Purchases", Value = 80 },
                }));
        cut.FindAll("path.bob-funnel-chart__step").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Gauge_Should_Render_Track_And_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBGaugeChart> cut =
            ctx.Render<BOBGaugeChart>(p => p
                .Add(c => c.Value, 65)
                .Add(c => c.Caption, "CPU"));
        cut.FindAll("path.bob-gauge-chart__track").Should().HaveCount(1);
        cut.FindAll("path.bob-gauge-chart__value-arc").Should().HaveCount(1);
        cut.Find("text.bob-gauge-chart__value").TextContent.Should().Contain("65");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Radar_Should_Render_One_Polygon_Per_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBRadarChart<string, double>> cut =
            ctx.Render<BOBRadarChart<string, double>>(p => p
                .Add(c => c.Series, new[]
                {
                    new BOBChartSeries<string, double>
                    {
                        Label = "S1",
                        Points = new[]
                        {
                            new BOBChartPoint<string, double>("A", 8),
                            new BOBChartPoint<string, double>("B", 5),
                            new BOBChartPoint<string, double>("C", 9),
                            new BOBChartPoint<string, double>("D", 6),
                            new BOBChartPoint<string, double>("E", 7),
                        }
                    },
                    new BOBChartSeries<string, double>
                    {
                        Label = "S2",
                        Points = new[]
                        {
                            new BOBChartPoint<string, double>("A", 4),
                            new BOBChartPoint<string, double>("B", 9),
                            new BOBChartPoint<string, double>("C", 3),
                            new BOBChartPoint<string, double>("D", 8),
                            new BOBChartPoint<string, double>("E", 6),
                        }
                    },
                }));
        cut.FindAll("polygon.bob-radar-chart__polygon").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Heatmap_Should_Render_One_Rect_Per_Cell(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBHeatmapChart<string, string>> cut =
            ctx.Render<BOBHeatmapChart<string, string>>(p => p
                .Add(c => c.Cells, new[]
                {
                    new BOBChartHeatmapCell<string, string>("Mon", "AM", 4),
                    new BOBChartHeatmapCell<string, string>("Mon", "PM", 8),
                    new BOBChartHeatmapCell<string, string>("Tue", "AM", 2),
                    new BOBChartHeatmapCell<string, string>("Tue", "PM", 7),
                }));
        cut.FindAll("rect.bob-heatmap-chart__cell").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Boxplot_Should_Render_Box_Plus_Median(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBBoxplotChart<string>> cut =
            ctx.Render<BOBBoxplotChart<string>>(p => p
                .Add(c => c.Boxes, new[]
                {
                    BOBChartBoxStat<string>.FromValues("A", new[] { 1.0, 2, 3, 4, 5, 6, 7, 8, 9 }),
                    BOBChartBoxStat<string>.FromValues("B", new[] { 5.0, 6, 7, 8, 9, 10, 11, 12, 13 }),
                }));
        cut.FindAll("rect.bob-boxplot-chart__box").Should().HaveCount(2);
        cut.FindAll("line.bob-boxplot-chart__median").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PolarArea_Should_Render_Wedges(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBPolarAreaChart<double>> cut =
            ctx.Render<BOBPolarAreaChart<double>>(p => p
                .Add(c => c.Slices, new[]
                {
                    new BOBChartSlice<double> { Label = "M", Value = 30 },
                    new BOBChartSlice<double> { Label = "T", Value = 22 },
                    new BOBChartSlice<double> { Label = "W", Value = 45 },
                    new BOBChartSlice<double> { Label = "T", Value = 18 },
                    new BOBChartSlice<double> { Label = "F", Value = 38 },
                }));
        cut.FindAll("path.bob-polar-area-chart__wedge").Should().HaveCount(5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BarChart_Waterfall_Should_Render_One_Bar_Per_Delta(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBBarChart<string, double>> cut =
            ctx.Render<BOBBarChart<string, double>>(p => p
                .Add(c => c.Series, new[]
                {
                    new BOBChartSeries<string, double>
                    {
                        Label = "Bridge",
                        Points = new[]
                        {
                            new BOBChartPoint<string, double>("Start", 100),
                            new BOBChartPoint<string, double>("Adj1", -25),
                            new BOBChartPoint<string, double>("Adj2", 40),
                            new BOBChartPoint<string, double>("Adj3", -10),
                            new BOBChartPoint<string, double>("End", 15),
                        }
                    }
                })
                .Add(c => c.StackMode, BOBBarStackMode.Waterfall));
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task LineChart_Sparkline_Should_Hide_Axes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, new[]
                {
                    new BOBChartSeries<int, double>
                    {
                        Label = "S",
                        Points = Enumerable.Range(1, 10)
                            .Select(i => new BOBChartPoint<int, double>(i, Math.Sin(i * 0.5)))
                            .ToArray()
                    }
                })
                .Add(c => c.Sparkline, true)
                .Add(c => c.Width, 120)
                .Add(c => c.Height, 32));

        // Sparkline mode hides grid + axes + legend.
        cut.FindAll("g.bob-line-chart__grid").Should().BeEmpty();
        cut.FindAll("g.bob-line-chart__axis").Should().BeEmpty();
        cut.FindAll("path.bob-line-chart__line").Should().HaveCount(1);
    }
}
