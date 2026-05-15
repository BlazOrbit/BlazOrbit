using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.ScatterChart;

[Trait("Component Rendering", "BOBScatterChart")]
public class BOBScatterChartTests
{
    private static IEnumerable<BOBChartSeries<double, double>> ScatterSeries() =>
    [
        new BOBChartSeries<double, double>
        {
            Label = "Cluster A",
            Points =
            [
                new BOBChartPoint<double, double>(1.2, 3.4), new BOBChartPoint<double, double>(2.6, 4.1),
                new BOBChartPoint<double, double>(3.8, 5.5), new BOBChartPoint<double, double>(4.5, 4.9)
            ]
        },
        new BOBChartSeries<double, double>
        {
            Label = "Cluster B",
            Points =
            [
                new BOBChartPoint<double, double>(2.0, 1.0), new BOBChartPoint<double, double>(3.5, 2.3),
                new BOBChartPoint<double, double>(4.0, 1.7)
            ]
        }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Marker_Per_Point(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBScatterChart<double, double>> cut =
            ctx.Render<BOBScatterChart<double, double>>(p => p.Add(c => c.Series, ScatterSeries()));

        // 4 + 3 = 7 markers total.
        cut.FindAll("circle.bob-scatter-chart__marker").Should().HaveCount(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_MarkerRadius_For_Each_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBScatterChart<double, double>> cut =
            ctx.Render<BOBScatterChart<double, double>>(p => p
                .Add(c => c.Series, ScatterSeries())
                .Add(c => c.MarkerRadius, 9.0));

        IReadOnlyList<AngleSharp.Dom.IElement> markers = cut.FindAll("circle.bob-scatter-chart__marker");
        foreach (AngleSharp.Dom.IElement m in markers)
        {
            double r = double.Parse(m.GetAttribute("r")!, System.Globalization.CultureInfo.InvariantCulture);
            r.Should().Be(9);
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bubble_Mode_Should_Vary_Radius_By_Size(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBScatterChart<double, double>> cut =
            ctx.Render<BOBScatterChart<double, double>>(p => p
                .Add(c => c.BubbleSeries,
                [
                    new BOBChartBubbleSeries<double, double>
                        {
                            Label = "Markets",
                            Points =
                            [
                                new BOBChartBubblePoint<double, double>(1.0, 2.0, 5),
                                new BOBChartBubblePoint<double, double>(2.0, 4.0, 50),
                                new BOBChartBubblePoint<double, double>(3.0, 3.0, 100)
                            ]
                        }
                ]));

        IReadOnlyList<AngleSharp.Dom.IElement> bubbles = cut.FindAll("circle.bob-scatter-chart__bubble");
        bubbles.Should().HaveCount(3);

        IEnumerable<double> radii = bubbles.Select(b =>
            double.Parse(b.GetAttribute("r")!, System.Globalization.CultureInfo.InvariantCulture));
        radii.Distinct().Count().Should().Be(3, "different Size values map to different radii");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task OnPointClick_Should_Carry_Series_And_Coords(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartClickArgs<double, double>? captured = null;

        IRenderedComponent<BOBScatterChart<double, double>> cut =
            ctx.Render<BOBScatterChart<double, double>>(p => p
                .Add(c => c.Series, ScatterSeries())
                .Add(c => c.OnPointClick, args => captured = args));

        cut.Find("circle.bob-scatter-chart__marker").Click();

        captured.Should().NotBeNull();
        captured!.SeriesLabel.Should().Be("Cluster A");
        captured.X.Should().Be(1.2);
        captured.Y.Should().Be(3.4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Hidden_Series_Should_Skip_Markers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBScatterChart<double, double>> cut =
            ctx.Render<BOBScatterChart<double, double>>(p => p.Add(c => c.Series, ScatterSeries()));

        cut.FindAll("circle.bob-scatter-chart__marker").Should().HaveCount(7);

        cut.Find("button.bob-chart__legend-button").Click();
        cut.FindAll("circle.bob-scatter-chart__marker").Should().HaveCount(3,
            "Cluster A hidden → only Cluster B's 3 points remain");
    }
}
