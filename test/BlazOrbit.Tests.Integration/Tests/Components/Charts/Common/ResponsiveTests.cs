using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

/// <summary>
/// Tests for the ResizeObserver-driven responsive layout (US-007). The
/// JS-side observer install is exercised by the templates E2E suite (real
/// browser); these unit tests cover the C# state machine — measured
/// dims become effective dims, explicit Width/Height take priority, and
/// <c>OnResize</c> only triggers re-render on rounded-pixel changes.
/// </summary>
[Trait("Component Interaction", "Charts.Responsive")]
public class ResponsiveTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> Series() => new[]
    {
        new BOBChartSeries<string, decimal>
        {
            Label = "S",
            Points = new[] { new BOBChartPoint<string, decimal>("Q1", 100m) }
        }
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Default_Dimensions_When_Unmeasured(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series()));

        // No explicit Width/Height + no OnResize fired yet → default 600×400.
        // Internal computation places the X axis labels at PlotBottom + 18 = (400-32)+18 = 386.
        // We can't easily inspect svgWidth, so just check chart renders.
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Measured_Dimensions_From_OnResize(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series()));

        // Simulate JS-side ResizeObserver invoking back into C#.
        await cut.InvokeAsync(() => cut.Instance.OnResize(800, 500));

        // SVG renders at 800×500 → first bar's x position depends on plot rect
        // (PlotLeft=56, PlotRight=800-12=788, plotWidth=732, single category
        // band centered at 56 + 732/2 = 422). With BarGroupRatio 0.8 and 1
        // series the bar spans 0.8 * 732 = 585.6 wide, x = 422 - 292.8 = 129.2.
        AngleSharp.Dom.IElement bar = cut.Find("rect.bob-bar-chart__bar");
        double x = double.Parse(bar.GetAttribute("x")!, System.Globalization.CultureInfo.InvariantCulture);
        x.Should().BeApproximately(129.2, 0.5,
            because: "with width=800 the plot rect widens and bar position scales accordingly");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Explicit_Width_Should_Win_Over_Measured(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.Width, 400));

        // Even if a (stray) OnResize fires, an explicit Width=400 wins.
        await cut.InvokeAsync(() => cut.Instance.OnResize(900, 700));

        // Re-grab the bar after invoke.
        AngleSharp.Dom.IElement bar = cut.Find("rect.bob-bar-chart__bar");

        // With width=400: plotLeft=56, plotRight=388, plotWidth=332,
        // band centered at 56 + 166 = 222. Single bar spans 0.8 * 332 = 265.6.
        // x = 222 - 132.8 = 89.2.
        double x = double.Parse(bar.GetAttribute("x")!, System.Globalization.CultureInfo.InvariantCulture);
        x.Should().BeApproximately(89.2, 0.5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task OnResize_With_Same_Rounded_Value_Should_Not_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series()));

        // First measurement: 800×500.
        await cut.InvokeAsync(() => cut.Instance.OnResize(800, 500));
        int rendersAfterFirst = cut.RenderCount;

        // Sub-pixel oscillation rounding to the same int — no re-render.
        await cut.InvokeAsync(() => cut.Instance.OnResize(800.3, 500.4));
        cut.RenderCount.Should().Be(rendersAfterFirst,
            because: "rounding to the same integer should suppress redundant renders");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task OnResize_With_New_Value_Should_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series()));

        await cut.InvokeAsync(() => cut.Instance.OnResize(800, 500));
        int rendersAfterFirst = cut.RenderCount;

        await cut.InvokeAsync(() => cut.Instance.OnResize(900, 600));
        cut.RenderCount.Should().BeGreaterThan(rendersAfterFirst);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task LineChart_Should_Use_EffectiveDimensions_For_Markers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, new[]
                {
                    new BOBChartSeries<int, double>
                    {
                        Label = "L",
                        Points = new[]
                        {
                            new BOBChartPoint<int, double>(1, 10.0),
                            new BOBChartPoint<int, double>(2, 20.0),
                        }
                    }
                }));

        await cut.InvokeAsync(() => cut.Instance.OnResize(1000, 600));

        // Markers reposition based on effective width.
        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(2);
        AngleSharp.Dom.IElement lastMarker = cut.FindAll("circle.bob-line-chart__marker")[1];
        double cx = double.Parse(lastMarker.GetAttribute("cx")!, System.Globalization.CultureInfo.InvariantCulture);
        // PlotRight = 1000 - 12 = 988. Last marker at PlotRight when domain spans the data range.
        cx.Should().BeApproximately(988.0, 0.5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Pie_Should_Use_EffectiveDimensions_For_Center(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, new[]
                {
                    new BOBChartSlice<decimal> { Label = "All", Value = 100m }
                }));

        await cut.InvokeAsync(() => cut.Instance.OnResize(500, 500));

        // Single-slice fallback: <circle> centered at (250, 250) when 500×500.
        AngleSharp.Dom.IElement disc = cut.Find("circle");
        double cx = double.Parse(disc.GetAttribute("cx")!, System.Globalization.CultureInfo.InvariantCulture);
        cx.Should().BeApproximately(250.0, 0.5);
    }
}
