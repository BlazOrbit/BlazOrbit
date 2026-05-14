using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Phase3;

[Trait("Component Snapshots", "Charts.Phase3")]
public class Phase3SnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Treemap_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Flat",
                Html = ctx.Render<BOBTreemapChart<double>>(p => p
                    .Add(c => c.Width, 320)
                    .Add(c => c.Height, 200)
                    .Add(c => c.Nodes,
                    [
                        new BOBChartTreemapNode<double> { Label = "A", Value = 40 },
                            new BOBChartTreemapNode<double> { Label = "B", Value = 30 },
                            new BOBChartTreemapNode<double> { Label = "C", Value = 20 }
                    ])).GetNormalizedMarkup()
            },
            new
            {
                Name = "Hierarchy",
                Html = ctx.Render<BOBTreemapChart<double>>(p => p
                    .Add(c => c.Width, 320)
                    .Add(c => c.Height, 200)
                    .Add(c => c.Padding, 3.0)
                    .Add(c => c.Nodes,
                    [
                        new BOBChartTreemapNode<double>
                            {
                                Label = "Root",
                                Children =
                                [
                                    new BOBChartTreemapNode<double> { Label = "L1", Value = 60 },
                                    new BOBChartTreemapNode<double> { Label = "L2", Value = 40 }
                                ]
                            }
                    ])).GetNormalizedMarkup()
            }
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Sunburst_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Solid",
                Html = ctx.Render<BOBSunburstChart<double>>(p => p
                    .Add(c => c.Width, 240)
                    .Add(c => c.Height, 240)
                    .Add(c => c.Nodes,
                    [
                        new BOBChartTreemapNode<double> { Label = "A", Value = 30 },
                            new BOBChartTreemapNode<double> { Label = "B", Value = 70 }
                    ])).GetNormalizedMarkup()
            },
            new
            {
                Name = "DonutHole",
                Html = ctx.Render<BOBSunburstChart<double>>(p => p
                    .Add(c => c.Width, 240)
                    .Add(c => c.Height, 240)
                    .Add(c => c.InnerRadius, 0.4)
                    .Add(c => c.Nodes,
                    [
                        new BOBChartTreemapNode<double> { Label = "X", Value = 1 },
                            new BOBChartTreemapNode<double> { Label = "Y", Value = 2 }
                    ])).GetNormalizedMarkup()
            }
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Sankey_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "TwoColumn",
                Html = ctx.Render<BOBSankeyChart<double>>(p => p
                    .Add(c => c.Links,
                    [
                        new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 10 },
                            new BOBChartSankeyLink<double> { Source = "A", Target = "C", Value = 5 }
                    ])).GetNormalizedMarkup()
            },
            new
            {
                Name = "ThreeColumn",
                Html = ctx.Render<BOBSankeyChart<double>>(p => p
                    .Add(c => c.Links,
                    [
                        new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 10 },
                            new BOBChartSankeyLink<double> { Source = "B", Target = "C", Value = 8 }
                    ])).GetNormalizedMarkup()
            }
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Stock_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartCandlePoint<int>[] candles =
        [
            new(1, 100, 110, 95, 108, 1000),
            new(2, 108, 112, 102, 104, 1200),
            new(3, 104, 115, 100, 113, 900)
        ];

        var testCases = new[]
        {
            new
            {
                Name = "PriceOnly",
                Html = ctx.Render<BOBStockChart<int>>(p => p
                    .Add(c => c.Candles, candles)).GetNormalizedMarkup()
            },
            new
            {
                Name = "WithVolume",
                Html = ctx.Render<BOBStockChart<int>>(p => p
                    .Add(c => c.Candles, candles)
                    .Add(c => c.Indicators, [BOBChartTechnicalIndicator.Volume])).GetNormalizedMarkup()
            }
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Mixed_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "BarPlusLine",
                Html = ctx.Render<BOBMixedChart<string>>(p => p
                    .Add(c => c.Series,
                    [
                        new BOBChartMixedSeries<string>
                            {
                                Label = "B",
                                Type = BOBChartMixedSeriesType.Bar,
                                Points =
                                [
                                    new BOBChartPoint<string, double>("A", 5),
                                    new BOBChartPoint<string, double>("B", 8)
                                ]
                            },
                            new BOBChartMixedSeries<string>
                            {
                                Label = "L",
                                Type = BOBChartMixedSeriesType.Line,
                                Points =
                                [
                                    new BOBChartPoint<string, double>("A", 6),
                                    new BOBChartPoint<string, double>("B", 7)
                                ]
                            }
                    ])).GetNormalizedMarkup()
            },
            new
            {
                Name = "DualAxis",
                Html = ctx.Render<BOBMixedChart<string>>(p => p
                    .Add(c => c.SecondaryAxisLabel, "%")
                    .Add(c => c.Series,
                    [
                        new BOBChartMixedSeries<string>
                            {
                                Label = "Bar",
                                Type = BOBChartMixedSeriesType.Bar,
                                Points = [new BOBChartPoint<string, double>("A", 100)]
                            },
                            new BOBChartMixedSeries<string>
                            {
                                Label = "Line",
                                Type = BOBChartMixedSeriesType.Line,
                                UseSecondaryAxis = true,
                                Points = [new BOBChartPoint<string, double>("A", 0.5)]
                            }
                    ])).GetNormalizedMarkup()
            }
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
