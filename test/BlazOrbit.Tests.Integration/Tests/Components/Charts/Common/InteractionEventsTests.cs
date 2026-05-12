using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.Events")]
public class InteractionEventsTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> BarSeries() =>
    [
        new BOBChartSeries<string, decimal>
        {
            Label = "Sales",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 100m),
                new BOBChartPoint<string, decimal>("Q2", 150m)
            ]
        }
    ];

    private static IEnumerable<BOBChartSeries<int, double>> LineSeries() =>
    [
        new BOBChartSeries<int, double>
        {
            Label = "Latency",
            Points =
            [
                new BOBChartPoint<int, double>(1, 10.0), new BOBChartPoint<int, double>(2, 22.5),
                new BOBChartPoint<int, double>(3, 18.0)
            ]
        }
    ];

    private static IEnumerable<BOBChartSlice<decimal>> Slices() =>
    [
        new BOBChartSlice<decimal> { Label = "EMEA", Value = 60m },
        new BOBChartSlice<decimal> { Label = "Americas", Value = 40m }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bar_Should_Fire_OnPointClick_With_Args(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartClickArgs<string, decimal>? captured = null;

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, BarSeries())
                .Add(c => c.OnPointClick, args => captured = args));

        // Click first bar (Q1=100).
        cut.Find("rect.bob-bar-chart__bar").Click();

        captured.Should().NotBeNull();
        captured!.SeriesLabel.Should().Be("Sales");
        captured.X.Should().Be("Q1");
        captured.Y.Should().Be(100m);
        captured.PointIndex.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bar_Should_Fire_OnDataHover_On_MouseEnter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartHoverArgs<string, decimal>? captured = null;

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, BarSeries())
                .Add(c => c.OnDataHover, args => captured = args));

        cut.FindAll("rect.bob-bar-chart__bar")[1].MouseEnter();

        captured.Should().NotBeNull();
        captured!.X.Should().Be("Q2");
        captured.Y.Should().Be(150m);
        captured.PointIndex.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bar_Should_Skip_Click_Wiring_When_No_Handler(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, BarSeries()));

        // No OnPointClick handler → no cursor:pointer attribute on the rect.
        cut.Find("rect.bob-bar-chart__bar")
            .HasAttribute("cursor").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Line_Should_Fire_OnPointClick_On_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartClickArgs<int, double>? captured = null;

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, LineSeries())
                .Add(c => c.OnPointClick, args => captured = args));

        cut.FindAll("circle.bob-line-chart__marker")[2].Click();

        captured.Should().NotBeNull();
        captured!.X.Should().Be(3);
        captured.Y.Should().Be(18.0);
        captured.PointIndex.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Line_OnPointClick_Should_Be_Inert_When_Markers_Off(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool clicked = false;

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, LineSeries())
                .Add(c => c.ShowMarkers, false)
                .Add(c => c.OnPointClick, _ => clicked = true));

        cut.FindAll("circle.bob-line-chart__marker").Should().BeEmpty();
        clicked.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Pie_Should_Fire_OnSliceClick_With_Percentage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartSliceClickArgs<decimal>? captured = null;

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, Slices())
                .Add(c => c.OnSliceClick, args => captured = args));

        cut.Find("path.bob-pie-chart__arc").Click();

        captured.Should().NotBeNull();
        captured!.SliceLabel.Should().Be("EMEA");
        captured.Value.Should().Be(60m);
        captured.SliceIndex.Should().Be(0);
        captured.Percentage.Should().BeApproximately(60.0, 0.01);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Pie_Should_Fire_OnSliceHover(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartSliceHoverArgs<decimal>? captured = null;

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, Slices())
                .Add(c => c.OnSliceHover, args => captured = args));

        cut.FindAll("path.bob-pie-chart__arc")[1].MouseEnter();

        captured.Should().NotBeNull();
        captured!.SliceLabel.Should().Be("Americas");
        captured.SliceIndex.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Donut_Should_Fire_OnSliceClick_Inherited_From_Base(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartSliceClickArgs<decimal>? captured = null;

        IRenderedComponent<BOBDonutChart<decimal>> cut =
            ctx.Render<BOBDonutChart<decimal>>(p => p
                .Add(c => c.Slices, Slices())
                .Add(c => c.OnSliceClick, args => captured = args));

        cut.Find("path.bob-pie-chart__arc").Click();

        captured.Should().NotBeNull();
        captured!.SliceLabel.Should().Be("EMEA");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Area_Should_Inherit_Line_OnPointClick(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartClickArgs<int, double>? captured = null;

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, LineSeries())
                .Add(c => c.ShowMarkers, true) // Area defaults markers off; turn on for click.
                .Add(c => c.OnPointClick, args => captured = args));

        cut.Find("circle.bob-line-chart__marker").Click();

        captured.Should().NotBeNull();
        captured!.SeriesLabel.Should().Be("Latency");
        captured.PointIndex.Should().Be(0);
    }
}