using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.AnimationsRefLines")]
public class AnimationsAndReferenceLinesTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> Series() =>
    [
        new BOBChartSeries<string, decimal>
        {
            Label = "Latency",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 100m),
                new BOBChartPoint<string, decimal>("Q2", 150m)
            ]
        }
    ];

    // ---------- Animations ----------

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Animations_Should_Be_On_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series()));

        AngleSharp.Dom.IElement host = cut.Find("bob-component");
        host.HasAttribute("data-bob-animated").Should().BeTrue();
        host.GetAttribute("style").Should().Contain("--bob-chart-anim-duration: 400ms");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Animations_Should_Be_Off_When_Animated_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.Animated, false));

        cut.Find("bob-component").HasAttribute("data-bob-animated").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Animations_Should_Honor_Custom_Duration(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.AnimationDuration, 1200));

        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-chart-anim-duration: 1200ms");
    }

    // ---------- Reference / threshold lines ----------

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ReferenceLine_Should_Render_With_Default_Dashed_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.ReferenceLines, [new BOBChartReferenceLine { Value = 120, Label = "SLO 99.9%" }]));

        AngleSharp.Dom.IElement line = cut.Find("line.bob-chart__reference-line");
        line.GetAttribute("stroke-dasharray").Should().Be("6,4",
            "Dashed is the default style");

        cut.Find("text.bob-chart__reference-label").TextContent.Should().Be("SLO 99.9%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ReferenceLine_Solid_Should_Have_No_Dasharray(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.ReferenceLines,
                    [new BOBChartReferenceLine { Value = 100, Style = BOBChartReferenceLineStyle.Solid }]));

        cut.Find("line.bob-chart__reference-line")
            .HasAttribute("stroke-dasharray").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ReferenceLine_Dotted_Should_Use_Dot_Pattern(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.ReferenceLines,
                    [new BOBChartReferenceLine { Value = 100, Style = BOBChartReferenceLineStyle.Dotted }]));

        cut.Find("line.bob-chart__reference-line")
            .GetAttribute("stroke-dasharray").Should().Be("2,3");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ReferenceLine_Should_Use_Custom_Color(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.ReferenceLines, [new BOBChartReferenceLine { Value = 100, Color = "red" }]));

        cut.Find("line.bob-chart__reference-line")
            .GetAttribute("stroke").Should().Be("red");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ReferenceLine_Should_Skip_Label_When_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.ReferenceLines, [
                    new BOBChartReferenceLine { Value = 100 } // no Label
                ]));

        cut.FindAll("text.bob-chart__reference-label").Should().BeEmpty();
        cut.FindAll("line.bob-chart__reference-line").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ReferenceLine_Should_Render_Multiple(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.ReferenceLines,
                [
                    new BOBChartReferenceLine { Value = 80, Label = "Low" },
                        new BOBChartReferenceLine { Value = 120, Label = "High" },
                        new BOBChartReferenceLine { Value = 150, Label = "Critical" }
                ]));

        cut.FindAll("line.bob-chart__reference-line").Should().HaveCount(3);
        cut.FindAll("text.bob-chart__reference-label").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task LineChart_Should_Render_Reference_Lines(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartSeries<int, double>
                        {
                            Label = "L", Points = [new BOBChartPoint<int, double>(1, 5.0)]
                        }
                ])
                .Add(c => c.ReferenceLines, [new BOBChartReferenceLine { Value = 10, Label = "Target" }]));

        cut.FindAll("line.bob-chart__reference-line").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task AreaChart_Should_Render_Reference_Lines_Inherited(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAreaChart<int, double>> cut =
            ctx.Render<BOBAreaChart<int, double>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartSeries<int, double>
                        {
                            Label = "A",
                            Points =
                            [
                                new BOBChartPoint<int, double>(1, 5.0),
                                new BOBChartPoint<int, double>(2, 10.0)
                            ]
                        }
                ])
                .Add(c => c.ReferenceLines, [new BOBChartReferenceLine { Value = 7.5, Label = "Avg" }]));

        cut.FindAll("line.bob-chart__reference-line").Should().HaveCount(1);
    }
}