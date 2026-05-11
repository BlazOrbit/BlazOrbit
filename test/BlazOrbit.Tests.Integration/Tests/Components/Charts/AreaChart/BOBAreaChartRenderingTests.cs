using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.AreaChart;

[Trait("Component Rendering", "BOBAreaChart")]
public class BOBAreaChartRenderingTests
{
    private static IEnumerable<BOBChartSeries<int, double>> NumericSeries() => new[]
    {
        new BOBChartSeries<int, double>
        {
            Label = "Volume",
            Points = new[]
            {
                new BOBChartPoint<int, double>(1, 10.0),
                new BOBChartPoint<int, double>(2, 22.5),
                new BOBChartPoint<int, double>(3, 18.0),
                new BOBChartPoint<int, double>(4, 30.7),
            }
        }
    };

    private static IEnumerable<BOBChartSeries<int, double>> TwoSeries() => new[]
    {
        new BOBChartSeries<int, double>
        {
            Label = "A",
            Points = new[]
            {
                new BOBChartPoint<int, double>(1, 10),
                new BOBChartPoint<int, double>(2, 20),
            }
        },
        new BOBChartSeries<int, double>
        {
            Label = "B",
            Points = new[]
            {
                new BOBChartPoint<int, double>(1, 15),
                new BOBChartPoint<int, double>(2, 25),
            }
        }
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Family_And_Component_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries()));

        AngleSharp.Dom.IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("area-chart");
        root.HasAttribute("data-bob-data-visualization-base").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Filled_Area_Path_And_Line_Stroke(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries()));

        // Each series gets a fill path + a line stroke (inherited from Line).
        cut.FindAll("path.bob-area-chart__fill").Should().HaveCount(1);
        cut.FindAll("path.bob-line-chart__line").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_ShowMarkers_To_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries()));

        cut.Instance.ShowMarkers.Should().BeFalse(
            because: "area charts emphasize filled volume, not individual points");
        cut.FindAll("circle.bob-line-chart__marker").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_ShowMarkers_Override(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries())
                .Add(c => c.ShowMarkers, true));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_FillOpacity_To_Area_Path(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries())
                .Add(c => c.FillOpacity, 0.5));

        string opacity = cut.Find("path.bob-area-chart__fill").GetAttribute("fill-opacity") ?? string.Empty;
        opacity.Should().Be("0.5");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Area_Path_With_Z_Command(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries()));

        // The fill path drops to baseline, runs back, closes with Z.
        string d = cut.Find("path.bob-area-chart__fill").GetAttribute("d") ?? string.Empty;
        d.Should().StartWith("M ");
        d.Should().EndWith(" Z");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Fill_Per_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, TwoSeries()));

        cut.FindAll("path.bob-area-chart__fill").Should().HaveCount(2);
        cut.FindAll("path.bob-line-chart__line").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Series_Is_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>();

        cut.FindAll("path.bob-area-chart__fill").Should().BeEmpty();
        cut.FindAll("path.bob-line-chart__line").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Smooth_Curves_When_Smooth_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series, NumericSeries())
                .Add(c => c.Smooth, true));

        // Both stroke and fill share the curve engine: 'C' commands appear in both.
        string stroke = cut.Find("path.bob-line-chart__line").GetAttribute("d") ?? string.Empty;
        string fill = cut.Find("path.bob-area-chart__fill").GetAttribute("d") ?? string.Empty;

        stroke.Should().Contain(" C ");
        fill.Should().Contain(" C ");
    }
}
