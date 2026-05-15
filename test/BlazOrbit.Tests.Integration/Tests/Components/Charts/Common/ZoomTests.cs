using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.Zoom")]
public class ZoomTests
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
    public async Task Zoom_Should_Be_Off_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10()));

        cut.Instance.ZoomEnabled.Should().BeFalse();
        cut.FindAll("rect.bob-line-chart__crosshair-overlay").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Zoom_Should_Render_Overlay_Even_Without_Crosshair(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        // Same overlay rect serves both crosshair + zoom interactions.
        cut.FindAll("rect.bob-line-chart__crosshair-overlay").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task WheelUp_Should_Narrow_X_Domain_Around_Cursor(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        // Initial state: 10 markers across full domain.
        IReadOnlyList<AngleSharp.Dom.IElement> markers0 = cut.FindAll("circle.bob-line-chart__marker");
        markers0.Should().HaveCount(10);
        double initialFirstCx = double.Parse(markers0[0].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);
        double initialLastCx = double.Parse(markers0[9].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);

        // Wheel-up at the centre of the plot → 10% zoom in.
        cut.Find("rect.bob-line-chart__crosshair-overlay")
            .TriggerEvent("onwheel", new WheelEventArgs
            {
                DeltaY = -100,
                OffsetX = 266, // halfway across PlotWidth (532)
                OffsetY = 100
            });

        // After zoom, the rendered marker spread expands - points push out to
        // the new (narrower) axis edges. Compare the X delta of the same data
        // points before and after.
        IReadOnlyList<AngleSharp.Dom.IElement> markers1 = cut.FindAll("circle.bob-line-chart__marker");
        double newFirstCx = double.Parse(markers1[0].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);
        double newLastCx = double.Parse(markers1[9].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);

        double initialSpread = initialLastCx - initialFirstCx;
        double newSpread = newLastCx - newFirstCx;

        newSpread.Should().BeGreaterThan(initialSpread,
            "wheel-in shrinks the visible domain, so the same data X values map to a wider pixel spread");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task WheelDown_Should_Widen_X_Domain(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        // Pre-zoom in twice so we have something to zoom out from.
        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });

        IReadOnlyList<AngleSharp.Dom.IElement> markersIn = cut.FindAll("circle.bob-line-chart__marker");
        double inSpread = double.Parse(markersIn[9].GetAttribute("cx")!,
                              System.Globalization.CultureInfo.InvariantCulture)
                          - double.Parse(markersIn[0].GetAttribute("cx")!,
                              System.Globalization.CultureInfo.InvariantCulture);

        // Now wheel-down to zoom out.
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = 100, OffsetX = 266, OffsetY = 100 });

        IReadOnlyList<AngleSharp.Dom.IElement> markersOut = cut.FindAll("circle.bob-line-chart__marker");
        double outSpread = double.Parse(markersOut[9].GetAttribute("cx")!,
                               System.Globalization.CultureInfo.InvariantCulture)
                           - double.Parse(markersOut[0].GetAttribute("cx")!,
                               System.Globalization.CultureInfo.InvariantCulture);

        outSpread.Should().BeLessThan(inSpread,
            "wheel-out widens the visible domain, compressing the same data into less pixel spread");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task DoubleClick_Should_Reset_Zoom(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        IReadOnlyList<AngleSharp.Dom.IElement> initial = cut.FindAll("circle.bob-line-chart__marker");
        string initialFirst = initial[0].GetAttribute("cx") ?? string.Empty;
        string initialLast = initial[9].GetAttribute("cx") ?? string.Empty;

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });

        // Position changed after zoom.
        cut.Find("circle.bob-line-chart__marker").GetAttribute("cx").Should().NotBe(initialFirst);

        // Double-click resets.
        overlay.DoubleClick();

        IReadOnlyList<AngleSharp.Dom.IElement> reset = cut.FindAll("circle.bob-line-chart__marker");
        reset[0].GetAttribute("cx").Should().Be(initialFirst);
        reset[9].GetAttribute("cx").Should().Be(initialLast);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ResetZoom_Method_Should_Be_Public_For_External_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 100, OffsetY = 100 });

        // Programmatic reset (e.g. external "Reset zoom" button).
        await cut.InvokeAsync(() => cut.Instance.ResetZoom());

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(10);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Zoom_Should_Work_On_AreaChart_Inherited(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        cut.FindAll("rect.bob-line-chart__crosshair-overlay").Should().HaveCount(1);

        cut.Find("rect.bob-line-chart__crosshair-overlay")
            .TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });

        // Area still renders 1 fill path post-zoom.
        cut.FindAll("path.bob-area-chart__fill").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Pan_Should_Translate_Visible_Domain(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");

        // Zoom in first so we have something to pan within.
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });

        IReadOnlyList<AngleSharp.Dom.IElement> beforePan = cut.FindAll("circle.bob-line-chart__marker");
        double cxBefore = double.Parse(beforePan[0].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);

        // Drag right by 100px → domain shifts left (we see lower data X).
        overlay.MouseDown(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });
        overlay.MouseUp(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        IReadOnlyList<AngleSharp.Dom.IElement> afterPan = cut.FindAll("circle.bob-line-chart__marker");
        double cxAfter = double.Parse(afterPan[0].GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);

        cxAfter.Should().NotBe(cxBefore,
            "drag-to-pan should translate the visible X domain, repositioning every marker");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Pan_Without_MouseDown_Should_Not_Translate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true)
                .Add(c => c.ShowCrosshair, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");

        // Zoom in first.
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });

        IReadOnlyList<AngleSharp.Dom.IElement> before = cut.FindAll("circle.bob-line-chart__marker");
        string firstCxBefore = before[0].GetAttribute("cx") ?? string.Empty;

        // mousemove without prior mousedown → crosshair fires, NOT pan.
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        IReadOnlyList<AngleSharp.Dom.IElement> after = cut.FindAll("circle.bob-line-chart__marker");
        after[0].GetAttribute("cx").Should().Be(firstCxBefore,
            "mousemove without an active drag should not pan the domain");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Pan_Should_End_On_MouseLeave(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 266, OffsetY = 100 });
        overlay.MouseDown(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });

        // mouseleave clears the pan state (otherwise re-entering with a
        // moved cursor would jump as if drag continued).
        overlay.MouseLeave();

        IReadOnlyList<AngleSharp.Dom.IElement> beforeMove = cut.FindAll("circle.bob-line-chart__marker");
        string cxBefore = beforeMove[0].GetAttribute("cx") ?? string.Empty;

        // Subsequent mousemove WITHOUT a fresh mousedown should not pan.
        overlay.MouseMove(new MouseEventArgs { OffsetX = 400, OffsetY = 100 });

        cut.FindAll("circle.bob-line-chart__marker")[0]
            .GetAttribute("cx").Should().Be(cxBefore);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Pan_Should_Suppress_Crosshair_During_Drag(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true)
                .Add(c => c.ShowCrosshair, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");

        // Activate crosshair via mousemove first.
        overlay.MouseMove(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });
        cut.FindAll("line.bob-chart__crosshair").Should().HaveCount(1);
        string crossXBefore = cut.Find("line.bob-chart__crosshair").GetAttribute("x1") ?? string.Empty;

        // Begin drag and move - pan takes priority, crosshair stays at last pos.
        overlay.MouseDown(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });
        overlay.MouseMove(new MouseEventArgs { OffsetX = 300, OffsetY = 100 });

        // Crosshair line is still rendered but its X did not chase the cursor.
        cut.Find("line.bob-chart__crosshair").GetAttribute("x1").Should().Be(crossXBefore,
            "during a drag the crosshair freezes - pan owns the mousemove");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Zoom_Should_Coexist_With_Crosshair(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.ZoomEnabled, true)
                .Add(c => c.ShowCrosshair, true));

        AngleSharp.Dom.IElement overlay = cut.Find("rect.bob-line-chart__crosshair-overlay");

        // Crosshair fires on mousemove.
        overlay.MouseMove(new MouseEventArgs { OffsetX = 200, OffsetY = 100 });
        cut.FindAll("line.bob-chart__crosshair").Should().HaveCount(1);

        // Wheel still zooms; crosshair stays alive.
        overlay.TriggerEvent("onwheel", new WheelEventArgs { DeltaY = -100, OffsetX = 200, OffsetY = 100 });
        cut.FindAll("rect.bob-line-chart__crosshair-overlay").Should().HaveCount(1);
    }
}
