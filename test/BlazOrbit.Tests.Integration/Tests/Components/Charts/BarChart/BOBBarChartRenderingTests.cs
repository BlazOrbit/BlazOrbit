using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.BarChart;

[Trait("Component Rendering", "BOBBarChart")]
public class BOBBarChartRenderingTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> SampleSeries()
    {
        yield return new BOBChartSeries<string, decimal>
        {
            Label = "Sales",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 120m), new BOBChartPoint<string, decimal>("Q2", 95m),
                new BOBChartPoint<string, decimal>("Q3", 140m)
            ]
        };
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Family_And_Component_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleSeries()));

        // Assert
        AngleSharp.Dom.IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("bar-chart");
        root.HasAttribute("data-bob-data-visualization-base").Should().BeTrue(
            "every chart must opt into the data-visualization family for shared CSS");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Inner_Svg_Root_With_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleSeries()));

        // Assert
        AngleSharp.Dom.IElement svg = cut.Find("svg");
        svg.GetAttribute("role").Should().Be("img");
        svg.GetAttribute("aria-label").Should().Contain("Bar chart with");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Rect_Per_Data_Point(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleSeries()));

        // Assert
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(3,
            "three points were supplied in the single sample series");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Y_Axis_Grid_Lines_When_ShowGrid_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleSeries())
                .Add(c => c.YAxis, new BOBChartAxis { ShowGrid = true }));

        // Assert
        cut.FindAll(".bob-bar-chart__grid line").Should().NotBeEmpty(
            "the linear scale always emits at least 2 ticks (min + max)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Grid_When_ShowGrid_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleSeries())
                .Add(c => c.YAxis, new BOBChartAxis { ShowGrid = false }));

        // Assert
        cut.FindAll(".bob-bar-chart__grid").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Native_Title_For_Each_Bar(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleSeries()));

        // Assert
        // Native <title> children give us free, accessible hover tooltips.
        cut.FindAll("rect.bob-bar-chart__bar > title").Should().HaveCount(3);
        cut.FindAll("rect.bob-bar-chart__bar > title").First().TextContent
            .Should().Contain("Sales").And.Contain("Q1").And.Contain("120");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Series_Is_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>();

        // Assert
        cut.FindAll("rect.bob-bar-chart__bar").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Group_Multiple_Series_Side_By_Side(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartSeries<string, decimal>
                        {
                            Label = "A", Points = [new BOBChartPoint<string, decimal>("X", 10m)]
                        },
                        new BOBChartSeries<string, decimal>
                        {
                            Label = "B", Points = [new BOBChartPoint<string, decimal>("X", 20m)]
                        }
                ]));

        // Assert
        cut.FindAll(".bob-bar-chart__series").Should().HaveCount(2);
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(2);
    }
}