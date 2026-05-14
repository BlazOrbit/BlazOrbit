using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.Crosshair")]
public class CrosshairTests
{
    private static IEnumerable<BOBChartSeries<int, double>> TwoSeries() =>
    [
        new BOBChartSeries<int, double>
        {
            Label = "A",
            Points =
            [
                new BOBChartPoint<int, double>(1, 10.0), new BOBChartPoint<int, double>(2, 22.5),
                new BOBChartPoint<int, double>(3, 18.0), new BOBChartPoint<int, double>(4, 30.7)
            ]
        },
        new BOBChartSeries<int, double>
        {
            Label = "B",
            Points =
            [
                new BOBChartPoint<int, double>(1, 5.0), new BOBChartPoint<int, double>(2, 12.5),
                new BOBChartPoint<int, double>(3, 14.0), new BOBChartPoint<int, double>(4, 8.0)
            ]
        }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Be_Off_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries()));

        cut.FindAll("rect.bob-line-chart__crosshair-overlay").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Render_Overlay_When_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries())
                .Add(c => c.ShowCrosshair, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.GetAttribute("fill").Should().Be("transparent");
        overlay.GetAttribute("pointer-events").Should().Be("all");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Be_Inactive_Until_MouseMove(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries())
                .Add(c => c.ShowCrosshair, true));

        cut.FindAll("line.bob-chart__crosshair").Should().BeEmpty();
        cut.FindAll(".bob-chart__crosshair-readout").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Activate_On_MouseMove(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries())
                .Add(c => c.ShowCrosshair, true));

        // Move mouse near the middle of the plot — the snap should pick the
        // point closest in pixel space.
        cut.Find("rect.bob-line-chart__crosshair-overlay")
            .MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        cut.FindAll("line.bob-chart__crosshair").Should().HaveCount(1);
        cut.FindAll("circle.bob-chart__crosshair-marker").Should().HaveCount(2,
            "two series → two highlighted nearest-point markers");
        cut.FindAll(".bob-chart__crosshair-readout").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_List_All_Series_In_Readout(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries())
                .Add(c => c.ShowCrosshair, true));

        cut.Find("rect.bob-line-chart__crosshair-overlay")
            .MouseMove(new MouseEventArgs { OffsetX = 100, OffsetY = 50 });

        IEnumerable<string> labels = cut
            .FindAll(".bob-chart__crosshair-readout-label")
            .Select(e => e.TextContent);
        labels.Should().Contain("A").And.Contain("B");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Clear_On_MouseLeave(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries())
                .Add(c => c.ShowCrosshair, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseMove(new MouseEventArgs { OffsetX = 250, OffsetY = 100 });
        cut.FindAll("line.bob-chart__crosshair").Should().HaveCount(1);

        overlay.MouseLeave();
        cut.FindAll("line.bob-chart__crosshair").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Snap_Marker_To_Nearest_Point_X(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Use a single series with known X projection to validate snap math.
        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartSeries<int, double>
                        {
                            Label = "S",
                            Points =
                            [
                                new BOBChartPoint<int, double>(1, 10.0),
                                new BOBChartPoint<int, double>(4, 20.0)
                            ]
                        }
                ])
                .Add(c => c.ShowCrosshair, true));

        // With default 600×400, plot width = 532, X domain = [1, 4],
        // X=1 projects to PlotLeft=56, X=4 projects to PlotRight=588.
        // Mouse at OffsetX=100 → nearest is X=1 (snap @ 56).
        cut.Find("rect.bob-line-chart__crosshair-overlay")
            .MouseMove(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });

        AngleSharp.Dom.IElement marker = cut.Find("circle.bob-chart__crosshair-marker");
        double cx = double.Parse(marker.GetAttribute("cx")!, System.Globalization.CultureInfo.InvariantCulture);
        cx.Should().BeApproximately(56, 1, "cursor 100 is closer to X=1 → marker snaps to PlotLeft");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Pin_Crosshair_Position_On_Subpixel_Jitter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries())
                .Add(c => c.ShowCrosshair, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseMove(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });
        string firstX = cut.Find("line.bob-chart__crosshair").GetAttribute("x1") ?? string.Empty;

        // Sub-pixel jitter (< 0.5 px) should NOT move the visible crosshair.
        // (Blazor's auto-render after the event handler still fires, but our
        // dedup ensures the rendered DOM position is identical.)
        overlay.MouseMove(new MouseEventArgs { OffsetX = 200.2, OffsetY = 100 });
        string secondX = cut.Find("line.bob-chart__crosshair").GetAttribute("x1") ?? string.Empty;

        secondX.Should().Be(firstX,
            "sub-pixel cursor jitter should not move the crosshair");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Crosshair_Should_Work_On_AreaChart_Inherited(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries())
                .Add(c => c.ShowCrosshair, true));

        cut.FindAll("rect.bob-line-chart__crosshair-overlay").Should().HaveCount(1);

        cut.Find("rect.bob-line-chart__crosshair-overlay")
            .MouseMove(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });

        cut.FindAll("circle.bob-chart__crosshair-marker").Should().HaveCount(2);
    }
}