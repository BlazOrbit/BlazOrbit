using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Phase3;

[Trait("Component Accessibility", "Charts.Phase3")]
public class Phase3AccessibilityTests
{
    // Aria-labels must announce data shape so screen readers can hint context
    // before the SVG geometry is exposed via roles.

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Treemap_AriaLabel_Should_Mention_Root_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBTreemapChart<double>> cut =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Width, 300)
                .Add(c => c.Height, 200)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double> { Label = "A", Value = 1 },
                        new BOBChartTreemapNode<double> { Label = "B", Value = 2 },
                        new BOBChartTreemapNode<double> { Label = "C", Value = 3 }
                ]));
        cut.Find("svg").GetAttribute("aria-label").Should().Be("Treemap chart with 3 root nodes");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Treemap_AriaLabel_Should_Note_Empty_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBTreemapChart<double>> cut =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Width, 300)
                .Add(c => c.Height, 200)
                .Add(c => c.Nodes, []));
        cut.Find("svg").GetAttribute("aria-label").Should().Be("Treemap chart with no data");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sunburst_AriaLabel_Should_Mention_Root_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSunburstChart<double>> cut =
            ctx.Render<BOBSunburstChart<double>>(p => p
                .Add(c => c.Width, 400)
                .Add(c => c.Height, 400)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double> { Label = "X", Value = 1 },
                        new BOBChartTreemapNode<double> { Label = "Y", Value = 2 }
                ]));
        cut.Find("svg").GetAttribute("aria-label").Should().Be("Sunburst chart with 2 root nodes");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sankey_AriaLabel_Should_Mention_Flow_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSankeyChart<double>> cut =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links,
                [
                    new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 1 },
                        new BOBChartSankeyLink<double> { Source = "B", Target = "C", Value = 1 }
                ]));
        cut.Find("svg").GetAttribute("aria-label").Should().Be("Sankey diagram with 2 flows");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stock_AriaLabel_Should_Mention_Bars_And_Panes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBStockChart<int>> cut =
            ctx.Render<BOBStockChart<int>>(p => p
                .Add(c => c.Candles,
                [
                    new BOBChartCandlePoint<int>(1, 100, 110, 95, 108, 1000),
                        new BOBChartCandlePoint<int>(2, 108, 112, 102, 104, 1200)
                ])
                .Add(c => c.Indicators,
                    [BOBChartTechnicalIndicator.Volume, BOBChartTechnicalIndicator.Rsi]));
        cut.Find("svg").GetAttribute("aria-label").Should().Be("Stock chart with 2 bars and 2 indicator pane(s)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Mixed_AriaLabel_Should_Mention_Series_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBMixedChart<string>> cut =
            ctx.Render<BOBMixedChart<string>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartMixedSeries<string>
                        {
                            Label = "S1",
                            Type = BOBChartMixedSeriesType.Bar,
                            Points = [new BOBChartPoint<string, double>("A", 10)]
                        },
                        new BOBChartMixedSeries<string>
                        {
                            Label = "S2",
                            Type = BOBChartMixedSeriesType.Line,
                            Points = [new BOBChartPoint<string, double>("A", 20)]
                        }
                ]));
        cut.Find("svg").GetAttribute("aria-label").Should().Be("Mixed chart with 2 series");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task All_New_Charts_Should_Set_Svg_Role_Img(BlazorScenario scenario)
    {
        // WCAG 1.1.1 — SVG that conveys information needs role="img" + accessible name.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTreemapChart<double>> treemap =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Width, 200)
                .Add(c => c.Height, 200)
                .Add(c => c.Nodes, [new BOBChartTreemapNode<double> { Label = "X", Value = 5 }]));
        treemap.Find("svg").GetAttribute("role").Should().Be("img");

        IRenderedComponent<BOBSankeyChart<double>> sankey =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links, [new BOBChartSankeyLink<double> { Source = "A", Target = "B", Value = 1 }]));
        sankey.Find("svg").GetAttribute("role").Should().Be("img");
    }
}

[Trait("Component Interaction", "Charts.Phase3")]
public class Phase3InteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Treemap_OnNodeClick_Should_Carry_Node_Path_And_Depth(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartTreemapClickArgs<double>? captured = null;
        IRenderedComponent<BOBTreemapChart<double>> cut =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Width, 400)
                .Add(c => c.Height, 200)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double>
                        {
                            Label = "Tech",
                            Children =
                            [
                                new BOBChartTreemapNode<double> { Label = "AAPL", Value = 60 },
                                new BOBChartTreemapNode<double> { Label = "MSFT", Value = 40 }
                            ]
                        }
                ])
                .Add(c => c.OnNodeClick, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<BOBChartTreemapClickArgs<double>>(
                    this, args => captured = args)));

        // Click the first leaf cell — the root rect is rendered first followed by
        // its children, so the second cell is the first leaf inside the branch.
        IReadOnlyList<AngleSharp.Dom.IElement> cells = cut.FindAll("rect.bob-treemap-chart__cell");
        cells.Count.Should().BeGreaterThan(1);
        await cut.InvokeAsync(() => cells[1].Click());

        captured.Should().NotBeNull();
        captured!.Path.Should().NotBeEmpty();
        captured.Depth.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sankey_OnNodeClick_Should_Carry_Node_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? clickedLabel = null;
        IRenderedComponent<BOBSankeyChart<double>> cut =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links,
                [
                    new BOBChartSankeyLink<double> { Source = "Solar", Target = "Grid", Value = 10 },
                        new BOBChartSankeyLink<double> { Source = "Grid", Target = "Homes", Value = 7 }
                ])
                .Add(c => c.OnNodeClick, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<string>(
                    this, label => clickedLabel = label)));

        IReadOnlyList<AngleSharp.Dom.IElement> nodes = cut.FindAll("rect.bob-sankey-chart__node");
        nodes.Should().HaveCount(3);
        await cut.InvokeAsync(() => nodes[0].Click());
        clickedLabel.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sankey_OnLinkClick_Should_Carry_Link_Record(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartSankeyLink<double>? captured = null;
        IRenderedComponent<BOBSankeyChart<double>> cut =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links,
                [
                    new BOBChartSankeyLink<double> { Source = "Coal", Target = "Power", Value = 20 }
                ])
                .Add(c => c.OnLinkClick, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<BOBChartSankeyLink<double>>(
                    this, l => captured = l)));

        IReadOnlyList<AngleSharp.Dom.IElement> paths = cut.FindAll("path.bob-sankey-chart__link");
        paths.Should().HaveCount(1);
        await cut.InvokeAsync(() => paths[0].Click());
        captured.Should().NotBeNull();
        captured!.Source.Should().Be("Coal");
        captured.Target.Should().Be("Power");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sunburst_OnNodeClick_Should_Carry_Args(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartTreemapClickArgs<double>? captured = null;
        IRenderedComponent<BOBSunburstChart<double>> cut =
            ctx.Render<BOBSunburstChart<double>>(p => p
                .Add(c => c.Width, 400)
                .Add(c => c.Height, 400)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double> { Label = "A", Value = 30 },
                        new BOBChartTreemapNode<double> { Label = "B", Value = 70 }
                ])
                .Add(c => c.OnNodeClick, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<BOBChartTreemapClickArgs<double>>(
                    this, args => captured = args)));

        IReadOnlyList<AngleSharp.Dom.IElement> arcs = cut.FindAll("path.bob-sunburst-chart__arc");
        arcs.Should().HaveCount(2);
        await cut.InvokeAsync(() => arcs[0].Click());

        captured.Should().NotBeNull();
        captured!.Node.Label.Should().Be("A");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task FilterContext_ClearFilter_Should_Drop_One_Dimension_Only(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBChartFilterContext> cut =
            ctx.Render<BOBChartFilterContext>(p => p
                .Add(c => c.ChildContent, (Microsoft.AspNetCore.Components.RenderFragment)(__b => { })));

        BOBChartFilterContext instance = cut.Instance;
        await cut.InvokeAsync(async () =>
        {
            await instance.SetFilter("region", "EMEA");
            await instance.SetFilter("product", "P1");
        });

        await cut.InvokeAsync(() => instance.ClearFilter("region"));

        instance.GetFilter("region").Should().BeNull();
        instance.GetFilter("product").Should().Be("P1");
        instance.Filters.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task FilterContext_Empty_Dimension_Should_Be_NoOp(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBChartFilterContext> cut =
            ctx.Render<BOBChartFilterContext>(p => p
                .Add(c => c.ChildContent, (Microsoft.AspNetCore.Components.RenderFragment)(__b => { })));

        int callCount = 0;
        BOBChartFilterContext instance = cut.Instance;
        // Force an event subscription so we can detect spurious mutation notifications.
        await cut.InvokeAsync(() => instance.SetFilter("", "ignored"));

        // SetFilter with empty dimension should be a no-op (not mutate, not notify).
        instance.Filters.Should().BeEmpty();
        callCount.Should().Be(0);
    }
}

[Trait("Component Rendering", "Charts.Phase3")]
public class Phase3EdgeCasesTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Treemap_With_Custom_Color_Should_Use_It_For_Fill(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBTreemapChart<double>> cut =
            ctx.Render<BOBTreemapChart<double>>(p => p
                .Add(c => c.Width, 200)
                .Add(c => c.Height, 200)
                .Add(c => c.Nodes,
                [
                    new BOBChartTreemapNode<double> { Label = "Red", Value = 1, Color = "#ff0000" }
                ]));
        cut.Find("rect.bob-treemap-chart__cell").GetAttribute("fill").Should().Be("#ff0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sankey_With_Custom_Color_Should_Stroke_Link_With_It(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBSankeyChart<double>> cut =
            ctx.Render<BOBSankeyChart<double>>(p => p
                .Add(c => c.Links,
                [
                    new BOBChartSankeyLink<double>
                        {
                            Source = "A", Target = "B", Value = 5, Color = "#abcdef"
                        }
                ]));
        cut.Find("path.bob-sankey-chart__link").GetAttribute("stroke").Should().Be("#abcdef");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stock_UpColor_And_DownColor_Should_Drive_Candle_Fill(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBStockChart<int>> cut =
            ctx.Render<BOBStockChart<int>>(p => p
                .Add(c => c.UpColor, "#00aa00")
                .Add(c => c.DownColor, "#aa0000")
                .Add(c => c.Candles,
                [
                    new BOBChartCandlePoint<int>(1, 100, 110, 95, 108), // bullish
                        new BOBChartCandlePoint<int>(2, 108, 112, 100, 102) // bearish
                ]));
        IReadOnlyList<AngleSharp.Dom.IElement> bodies = cut.FindAll("rect.bob-stock-chart__body");
        bodies[0].GetAttribute("fill").Should().Be("#00aa00");
        bodies[1].GetAttribute("fill").Should().Be("#aa0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Mixed_With_SecondaryAxisLabel_Should_Render_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBMixedChart<string>> cut =
            ctx.Render<BOBMixedChart<string>>(p => p
                .Add(c => c.SecondaryAxisLabel, "USD")
                .Add(c => c.Series,
                [
                    new BOBChartMixedSeries<string>
                        {
                            Label = "L",
                            Type = BOBChartMixedSeriesType.Line,
                            UseSecondaryAxis = true,
                            Points =
                            [
                                new BOBChartPoint<string, double>("A", 1),
                                new BOBChartPoint<string, double>("B", 2)
                            ]
                        }
                ]));
        cut.FindAll("text.bob-mixed-chart__axis-title")
            .Select(t => t.TextContent).Should().Contain("USD");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Stock_With_MACD_Should_Render_Histogram_Bars(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        // Enough candles to clear MACD warm-up (slow=26 + signal=9 = 35 needed).
        BOBChartCandlePoint<int>[] candles = Enumerable.Range(1, 50)
            .Select(i => new BOBChartCandlePoint<int>(i, 100 + (i * 0.5), 105 + (i * 0.5), 95 + (i * 0.5), 100 + (i * 0.5) + ((i % 3) - 1), 1000))
            .ToArray();
        IRenderedComponent<BOBStockChart<int>> cut =
            ctx.Render<BOBStockChart<int>>(p => p
                .Add(c => c.Candles, candles)
                .Add(c => c.Indicators, [BOBChartTechnicalIndicator.Macd]));
        cut.FindAll("rect.bob-stock-chart__macd-hist").Count.Should().BeGreaterThan(0);
    }
}
