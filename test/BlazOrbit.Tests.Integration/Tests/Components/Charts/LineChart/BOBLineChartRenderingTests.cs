using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.LineChart;

[Trait("Component Rendering", "BOBLineChart")]
public class BOBLineChartRenderingTests
{
    private static IEnumerable<BOBChartSeries<DateTime, decimal>> TemporalSeries()
    {
        yield return new BOBChartSeries<DateTime, decimal>
        {
            Label = "Sales",
            Points =
            [
                new BOBChartPoint<DateTime, decimal>(new DateTime(2026, 01, 01), 120m),
                new BOBChartPoint<DateTime, decimal>(new DateTime(2026, 02, 01), 95m),
                new BOBChartPoint<DateTime, decimal>(new DateTime(2026, 03, 01), 140m),
                new BOBChartPoint<DateTime, decimal>(new DateTime(2026, 04, 01), 180m)
            ]
        };
    }

    private static IEnumerable<BOBChartSeries<int, double>> NumericSeries()
    {
        yield return new BOBChartSeries<int, double>
        {
            Label = "Trend",
            Points =
            [
                new BOBChartPoint<int, double>(1, 10.0), new BOBChartPoint<int, double>(2, 15.5),
                new BOBChartPoint<int, double>(3, 12.0), new BOBChartPoint<int, double>(4, 18.7)
            ]
        };
    }

    private static IEnumerable<BOBChartSeries<string, decimal>> CategoricalSeries()
    {
        yield return new BOBChartSeries<string, decimal>
        {
            Label = "By region",
            Points =
            [
                new BOBChartPoint<string, decimal>("EMEA", 120m),
                new BOBChartPoint<string, decimal>("APAC", 95m),
                new BOBChartPoint<string, decimal>("Americas", 140m)
            ]
        };
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Family_And_Component_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<DateTime, decimal>> cut =
            ctx.Render<BOBLineChart<DateTime, decimal>>(p => p
                .Add(c => c.Series, TemporalSeries()));

        AngleSharp.Dom.IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("line-chart");
        root.HasAttribute("data-bob-data-visualization-base").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Path_Per_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries()));

        cut.FindAll("path.bob-line-chart__line").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Marker_Per_Point_When_ShowMarkers_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries()));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(4,
            "the sample series has 4 data points");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Markers_When_ShowMarkers_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries())
                .Add(c => c.ShowMarkers, false));

        cut.FindAll("circle.bob-line-chart__marker").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Cubic_Path_When_Smooth_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries())
                .Add(c => c.Smooth, true));

        // Smooth path uses 'C' (cubic) commands; polyline uses 'L' commands.
        string d = cut.Find("path.bob-line-chart__line").GetAttribute("d") ?? string.Empty;
        d.Should().Contain(" C ", "Smooth=true emits cubic bezier segments");
        d.Should().NotContain(" L ", "smooth path does not use polyline L commands");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Polyline_Path_When_Smooth_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries())
                .Add(c => c.Smooth, false));

        string d = cut.Find("path.bob-line-chart__line").GetAttribute("d") ?? string.Empty;
        d.Should().Contain(" L ");
        d.Should().NotContain(" C ");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Format_DateTime_X_Axis_Labels_With_Format(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<DateTime, decimal>> cut =
            ctx.Render<BOBLineChart<DateTime, decimal>>(p => p
                .Add(c => c.Series, TemporalSeries())
                .Add(c => c.XAxis, new BOBChartAxis { Format = "yyyy-MM" }));

        IEnumerable<string> labels = cut.FindAll(".bob-line-chart__axis--x text")
            .Select(t => t.TextContent);
        labels.Should().Contain(l => l.StartsWith("2026-"),
            "the X axis ticks are projected back to DateTime via Numeric.FormatContinuous");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Categorical_X_Axis_For_String_TX(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<string, decimal>> cut =
            ctx.Render<BOBLineChart<string, decimal>>(p => p
                .Add(c => c.Series, CategoricalSeries()));

        // Categorical scale → one label per distinct category.
        IEnumerable<string> labels = cut.FindAll(".bob-line-chart__axis--x text")
            .Select(t => t.TextContent);
        labels.Should().Contain("EMEA").And.Contain("APAC").And.Contain("Americas");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Native_Title_For_Each_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries()));

        cut.FindAll("circle.bob-line-chart__marker > title").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Series_Is_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>();

        cut.FindAll("path.bob-line-chart__line").Should().BeEmpty();
        cut.FindAll("circle.bob-line-chart__marker").Should().BeEmpty();
    }
}