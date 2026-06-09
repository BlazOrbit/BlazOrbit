using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.Brush")]
public class BrushTests
{
    private static IEnumerable<BOBChartSeries<int, double>> Series10() =>
    [
        new BOBChartSeries<int, double>
        {
            Label = "S",
            Points = Enumerable.Range(1, 10)
                .Select(i => new BOBChartPoint<int, double>(i, i * 1.0))
                .ToArray()
        }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_Should_Be_Off_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10()));

        cut.Instance.BrushEnabled.Should().BeFalse();
        cut.Instance.BrushAutoZoom.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_Should_Render_Rect_During_Drag(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.BrushEnabled, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");

        // No brush rect before drag.
        cut.FindAll("rect.bob-line-chart__brush").Should().BeEmpty();

        // mousedown + mousemove → brush rect renders.
        overlay.MouseDown(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        cut.FindAll("rect.bob-line-chart__brush").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_Rect_Should_Span_Drag_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.BrushEnabled, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseDown(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        AngleSharp.Dom.IElement brush = cut.Find("rect.bob-line-chart__brush");

        // Plot rect is offset by PlotLeft=56, so x = 100+56 = 156, width = 200.
        double x = double.Parse(brush.GetAttribute("x")!, System.Globalization.CultureInfo.InvariantCulture);
        double w = double.Parse(brush.GetAttribute("width")!, System.Globalization.CultureInfo.InvariantCulture);
        x.Should().BeApproximately(156, 1);
        w.Should().BeApproximately(200, 1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_MouseUp_Should_Fire_OnBrush(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartBrushArgs<int>? captured = null;

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.BrushEnabled, true)
                .Add(c => c.OnBrush, args => captured = args));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseDown(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });
        overlay.MouseUp(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        captured.Should().NotBeNull();
        captured!.MinX.Should().BeLessThan(captured.MaxX,
            "the args carry the X-domain values bracketing the drag");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_AutoZoom_Should_Apply_Selected_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.BrushEnabled, true)
                .Add(c => c.BrushAutoZoom, true));

        IReadOnlyList<AngleSharp.Dom.IElement> before = cut.FindAll("circle.bob-line-chart__marker");
        double firstCxBefore = double.Parse(before[0].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseDown(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });
        overlay.MouseUp(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        // After auto-zoom the markers reposition (the selected range maps to
        // the full plot width).
        double firstCxAfter = double.Parse(cut.FindAll("circle.bob-line-chart__marker")[0].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);
        firstCxAfter.Should().NotBe(firstCxBefore);

        // Brush rect should be cleared after release.
        cut.FindAll("rect.bob-line-chart__brush").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_AutoZoom_False_Should_Skip_Domain_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.BrushEnabled, true)
                .Add(c => c.BrushAutoZoom, false));

        string firstCxBefore = cut.Find("circle.bob-line-chart__marker").GetAttribute("cx") ?? string.Empty;

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseDown(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });
        overlay.MouseUp(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        // Domain unchanged when auto-zoom is off.
        cut.Find("circle.bob-line-chart__marker").GetAttribute("cx").Should().Be(firstCxBefore);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_Trivial_Click_Should_Not_Fire_OnBrush(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.BrushEnabled, true)
                .Add(c => c.OnBrush, _ => fired = true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        // Single click without movement (mousedown + mouseup at same X).
        overlay.MouseDown(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });
        overlay.MouseUp(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });

        fired.Should().BeFalse("trivial drags (< 2px) should not fire the brush event");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_Should_Take_Priority_Over_Pan(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartBrushArgs<int>? captured = null;

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true)
                .Add(c => c.BrushEnabled, true)
                .Add(c => c.OnBrush, args => captured = args));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseDown(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });
        overlay.MouseUp(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        // Brush rect should have rendered (pan would not produce one).
        captured.Should().NotBeNull("BrushEnabled wins over ZoomEnabled pan when both are set");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Brush_Should_Work_For_DateTime_X_Axis(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartBrushArgs<DateTime>? captured = null;

        IRenderedComponent<BOBLineChart<DateTime, double>> cut =
            ctx.Render<BOBLineChart<DateTime, double>>(p => p
                .Add(c => c.Series, [
                    new BOBChartSeries<DateTime, double>
                    {
                        Label = "T",
                        Points = Enumerable.Range(0, 10)
                            .Select(i => new BOBChartPoint<DateTime, double>(
                                new DateTime(2026, 01, 01).AddDays(i), i * 1.0))
                            .ToArray()
                    }
                ])
                .Add(c => c.BrushEnabled, true)
                .Add(c => c.OnBrush, args => captured = args));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.MouseDown(new MouseEventArgs { OffsetX = 100, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });
        overlay.MouseUp(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        captured.Should().NotBeNull();
        captured!.MinX.Year.Should().Be(2026);
        captured.MaxX.Should().BeAfter(captured.MinX);
    }
}
