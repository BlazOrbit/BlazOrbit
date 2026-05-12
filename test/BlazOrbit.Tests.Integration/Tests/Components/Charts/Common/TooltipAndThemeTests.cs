using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.TooltipAndTheme")]
public class TooltipAndThemeTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> Series() =>
    [
        new BOBChartSeries<string, decimal>
        {
            Label = "Sales", Points = [new BOBChartPoint<string, decimal>("Q1", 100m)]
        }
    ];

    // ---------- Theme parameter ----------

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Theme_Inherit_Should_Not_Emit_Theme_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series()));

        cut.Find("bob-component").HasAttribute("data-bob-theme").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Theme_Light_Should_Emit_Light_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.Theme, BOBChartTheme.Light));

        cut.Find("bob-component").GetAttribute("data-bob-theme").Should().Be("light");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Theme_Dark_Should_Emit_Dark_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.Theme, BOBChartTheme.Dark));

        cut.Find("bob-component").GetAttribute("data-bob-theme").Should().Be("dark");
    }

    // ---------- Custom tooltip overlay ----------

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Tooltip_Should_Be_Hidden_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series()));

        cut.FindAll(".bob-chart__tooltip").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Tooltip_Should_Show_On_Hover_When_Format_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.TooltipFormat, "{0} → {1}: {2:N0} EUR"));

        cut.FindAll(".bob-chart__tooltip").Should().BeEmpty();

        cut.Find("rect.bob-bar-chart__bar").MouseEnter();

        AngleSharp.Dom.IElement tooltip = cut.Find(".bob-chart__tooltip");
        tooltip.GetAttribute("role").Should().Be("tooltip");
        tooltip.QuerySelector(".bob-chart__tooltip-text")!.TextContent
            .Should().Contain("Sales").And.Contain("Q1").And.Contain("100");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Tooltip_Should_Hide_On_MouseLeave(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.TooltipFormat, "{0}: {1}={2}"));

        AngleSharp.Dom.IElement bar = cut.Find("rect.bob-bar-chart__bar");
        bar.MouseEnter();
        cut.FindAll(".bob-chart__tooltip").Should().HaveCount(1);

        bar.MouseLeave();
        cut.FindAll(".bob-chart__tooltip").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Tooltip_Should_Render_Custom_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        RenderFragment<BOBChartTooltipContext<string, decimal>> template = c => b =>
        {
            b.OpenElement(0, "strong");
            b.AddAttribute(1, "class", "custom-tip");
            b.AddContent(2, $"💰 {c.SeriesLabel}@{c.X} = {c.Y}");
            b.CloseElement();
        };

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.TooltipTemplate, template));

        cut.Find("rect.bob-bar-chart__bar").MouseEnter();

        // Custom template wins over default formatted text.
        AngleSharp.Dom.IElement custom = cut.Find(".bob-chart__tooltip strong.custom-tip");
        custom.TextContent.Should().Contain("💰").And.Contain("Sales").And.Contain("Q1");
        cut.FindAll(".bob-chart__tooltip-text").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Tooltip_Should_Replace_Native_Title_When_Configured(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series())
                .Add(c => c.TooltipFormat, "{0}: {1}"));

        // When custom tooltip is on, native <title> is omitted to avoid two
        // tooltip systems competing on hover.
        cut.FindAll("rect.bob-bar-chart__bar > title").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Line_Tooltip_Should_Show_On_Marker_Hover(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IEnumerable<BOBChartSeries<int, double>> lineSeries =
        [
            new BOBChartSeries<int, double>
            {
                Label = "L",
                Points =
                [
                    new BOBChartPoint<int, double>(1, 5.5), new BOBChartPoint<int, double>(2, 7.2)
                ]
            }
        ];

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, lineSeries)
                .Add(c => c.TooltipFormat, "{0}: x={1} y={2}"));

        cut.FindAll("circle.bob-line-chart__marker")[1].MouseEnter();

        cut.Find(".bob-chart__tooltip-text").TextContent
            .Should().Contain("L").And.Contain("x=2").And.Contain("y=7.2");
    }
}