using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Phase3;

[Trait("Component Rendering", "Charts.Phase3")]
public class Phase3SmokeTests
{
    // ─────────── Treemap ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Treemap_Should_Render_One_Rect_Per_Leaf(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBTreemapChart<double>> cut =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double> { Label = "A", Value = 40 },
                        new BOBChartTreemapNode<double> { Label = "B", Value = 30 },
                        new BOBChartTreemapNode<double> { Label = "C", Value = 20 },
                        new BOBChartTreemapNode<double> { Label = "D", Value = 10 }
                ]));
        cut.FindAll("rect.bob-treemap-chart__cell").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Treemap_Should_Recurse_Into_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBTreemapChart<double>> cut =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Width, 400)
                .Add(c => c.Height, 200)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double>
                        {
                            Label = "Root",
                            Children =
                            [
                                new BOBChartTreemapNode<double> { Label = "L1", Value = 50 },
                                new BOBChartTreemapNode<double> { Label = "L2", Value = 50 }
                            ]
                        }
                ]));
        // Branch node + 2 leaves rendered as nested rects.
        cut.FindAll("rect.bob-treemap-chart__cell").Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Treemap_Should_Skip_Render_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBTreemapChart<double>> cut =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Nodes, []));
        cut.FindAll("rect.bob-treemap-chart__cell").Should().BeEmpty();
    }

    // ─────────── Sunburst ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sunburst_Should_Render_One_Arc_Per_Node(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSunburstChart<double>> cut =
            ctx.Render<BOBSunburstChart<double>>(p => p
                .Add(c => c.Width, 400)
                .Add(c => c.Height, 400)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double>
                        {
                            Label = "A",
                            Children =
                            [
                                new BOBChartTreemapNode<double> { Label = "A1", Value = 10 },
                                new BOBChartTreemapNode<double> { Label = "A2", Value = 20 }
                            ]
                        },
                        new BOBChartTreemapNode<double> { Label = "B", Value = 30 }
                ]));
        // 2 roots + 2 children rendered as arcs. Branch nodes without an explicit
        // Value aggregate their children - Sunburst should treat the branch + leaves
        // as separate rings.
        cut.FindAll("path.bob-sunburst-chart__arc").Count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sunburst_With_InnerRadius_Should_Still_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSunburstChart<double>> cut =
            ctx.Render<BOBSunburstChart<double>>(p => p
                .Add(c => c.Width, 400)
                .Add(c => c.Height, 400)
                .Add(c => c.InnerRadius, 0.4)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double> { Label = "X", Value = 5 },
                        new BOBChartTreemapNode<double> { Label = "Y", Value = 7 }
                ]));
        cut.FindAll("path.bob-sunburst-chart__arc").Should().HaveCount(2);
    }

    // ─────────── Sankey ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sankey_Should_Render_One_Path_Per_Link(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSankeyChart<double>> cut =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links,
                [
                    new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 10 },
                        new BOBChartSankeyLink<double> { Source = "A", Target = "C", Value = 6 },
                        new BOBChartSankeyLink<double> { Source = "B", Target = "D", Value = 8 }
                ]));
        cut.FindAll("path.bob-sankey-chart__link").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sankey_Should_Render_Node_Rect_Per_Unique_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSankeyChart<double>> cut =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links,
                [
                    new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 5 },
                        new BOBChartSankeyLink<double> { Source = "B", Target = "C", Value = 5 }
                ]));
        // A, B, C - 3 distinct nodes.
        cut.FindAll("rect.bob-sankey-chart__node").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sankey_Should_Skip_Zero_Or_Empty_Links(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSankeyChart<double>> cut =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links,
                [
                    new BOBChartSankeyLink<double> { Source = "", Target = "B", Value = 5 },
                        new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 0 },
                        new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 5 }
                ]));
        cut.FindAll("path.bob-sankey-chart__link").Should().HaveCount(1);
    }

    // ─────────── Stock ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stock_Should_Render_One_Body_Per_Candle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBStockChart<int>> cut =
            ctx.Render<BOBStockChart<int>>(p => p
                .Add(c => c.Candles,
                [
                    new BOBChartCandlePoint<int>(1, 100, 110, 95, 108, 1000),
                        new BOBChartCandlePoint<int>(2, 108, 112, 102, 104, 1200),
                        new BOBChartCandlePoint<int>(3, 104, 115, 100, 113, 900)
                ]));
        cut.FindAll("rect.bob-stock-chart__body").Should().HaveCount(3);
        cut.FindAll("line.bob-stock-chart__wick").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stock_With_Volume_Indicator_Should_Render_Volume_Pane(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBStockChart<int>> cut =
            ctx.Render<BOBStockChart<int>>(p => p
                .Add(c => c.Candles,
                [
                    new BOBChartCandlePoint<int>(1, 100, 110, 95, 108, 1000),
                        new BOBChartCandlePoint<int>(2, 108, 112, 102, 104, 1200)
                ])
                .Add(c => c.Indicators, [BOBChartTechnicalIndicator.Volume]));
        cut.FindAll("rect.bob-stock-chart__volume").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stock_With_Bollinger_Should_Render_Three_Band_Lines(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        // Need enough candles to clear the Bollinger warm-up window (period=20).
        BOBChartCandlePoint<int>[] candles = Enumerable.Range(1, 30)
            .Select(i => new BOBChartCandlePoint<int>(i, 100, 105, 95, 100 + (i % 5), 1000))
            .ToArray();

        IRenderedComponent<BOBStockChart<int>> cut =
            ctx.Render<BOBStockChart<int>>(p => p
                .Add(c => c.Candles, candles)
                .Add(c => c.Indicators, [BOBChartTechnicalIndicator.BollingerBands]));
        cut.FindAll("path.bob-stock-chart__bb-mid").Should().HaveCount(1);
        cut.FindAll("path.bob-stock-chart__bb-upper").Should().HaveCount(1);
        cut.FindAll("path.bob-stock-chart__bb-lower").Should().HaveCount(1);
    }

    // ─────────── Mixed ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Mixed_Should_Render_Bars_For_Bar_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBMixedChart<string>> cut =
            ctx.Render<BOBMixedChart<string>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartMixedSeries<string>
                        {
                            Label = "Bars",
                            Type = BOBChartMixedSeriesType.Bar,
                            Points =
                            [
                                new BOBChartPoint<string, double>("A", 5),
                                new BOBChartPoint<string, double>("B", 8),
                                new BOBChartPoint<string, double>("C", 3)
                            ]
                        }
                ]));
        cut.FindAll("rect.bob-mixed-chart__bar").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Mixed_Should_Render_Each_Series_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBMixedChart<string>> cut =
            ctx.Render<BOBMixedChart<string>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartMixedSeries<string>
                        {
                            Label = "B",
                            Type = BOBChartMixedSeriesType.Bar,
                            Points = [new BOBChartPoint<string, double>("X", 10)]
                        },
                        new BOBChartMixedSeries<string>
                        {
                            Label = "A",
                            Type = BOBChartMixedSeriesType.Area,
                            Points =
                            [
                                new BOBChartPoint<string, double>("X", 5),
                                new BOBChartPoint<string, double>("Y", 7)
                            ]
                        },
                        new BOBChartMixedSeries<string>
                        {
                            Label = "L",
                            Type = BOBChartMixedSeriesType.Line,
                            Points =
                            [
                                new BOBChartPoint<string, double>("X", 8),
                                new BOBChartPoint<string, double>("Y", 9)
                            ]
                        }
                ]));
        cut.FindAll("rect.bob-mixed-chart__bar").Should().HaveCount(1);
        cut.FindAll("path.bob-mixed-chart__area").Should().HaveCount(1);
        cut.FindAll("path.bob-mixed-chart__line").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Mixed_Should_Render_Secondary_Axis_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBMixedChart<string>> cut =
            ctx.Render<BOBMixedChart<string>>(p => p
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
                ])
                .Add(c => c.SecondaryAxisLabel, "%"));
        // At least one secondary axis label modifier on the right edge.
        cut.FindAll("text.bob-mixed-chart__axis-label--secondary").Count.Should().BeGreaterThan(0);
    }

    // ─────────── FilterContext ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task FilterContext_Should_Set_And_Read_Active_Filter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBChartFilterContext> cut =
            ctx.Render<BOBChartFilterContext>(p => p
                .Add(c => c.ChildContent, (RenderFragment)(__b => { })));

        BOBChartFilterContext instance = cut.Instance;
        await cut.InvokeAsync(() => instance.SetFilter("region", "EMEA"));

        instance.GetFilter("region").Should().Be("EMEA");
        instance.Matches("region", "EMEA").Should().BeTrue();
        instance.Matches("region", "Americas").Should().BeFalse();
        instance.Matches("unset-dim", "anything").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task FilterContext_ClearAll_Should_Drop_All_Filters(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBChartFilterContext> cut =
            ctx.Render<BOBChartFilterContext>(p => p
                .Add(c => c.ChildContent, (RenderFragment)(__b => { })));

        BOBChartFilterContext instance = cut.Instance;
        await cut.InvokeAsync(async () =>
        {
            await instance.SetFilter("region", "EMEA");
            await instance.SetFilter("product", "P1");
        });

        instance.Filters.Should().HaveCount(2);

        await cut.InvokeAsync(() => instance.ClearAll());
        instance.Filters.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task FilterContext_Null_Value_Should_Clear_Dimension(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBChartFilterContext> cut =
            ctx.Render<BOBChartFilterContext>(p => p
                .Add(c => c.ChildContent, (RenderFragment)(__b => { })));

        BOBChartFilterContext instance = cut.Instance;
        await cut.InvokeAsync(async () =>
        {
            await instance.SetFilter("region", "EMEA");
            await instance.SetFilter("region", null);
        });

        instance.GetFilter("region").Should().BeNull();
        instance.Filters.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task FilterContext_Should_Raise_OnFiltersChanged(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        int callCount = 0;
        IReadOnlyDictionary<string, object?>? lastSnapshot = null;

        IRenderedComponent<BOBChartFilterContext> cut =
            ctx.Render<BOBChartFilterContext>(p => p
                .Add(c => c.ChildContent, (RenderFragment)(__b => { }))
                .Add(c => c.OnFiltersChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<IReadOnlyDictionary<string, object?>>(
                    this,
                    snapshot =>
                    {
                        callCount++;
                        lastSnapshot = snapshot;
                    })));

        BOBChartFilterContext instance = cut.Instance;
        await cut.InvokeAsync(() => instance.SetFilter("region", "EMEA"));

        callCount.Should().Be(1);
        lastSnapshot.Should().NotBeNull();
        lastSnapshot!["region"].Should().Be("EMEA");
    }
}
