using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.Legend")]
public class LegendTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> Series2() => new[]
    {
        new BOBChartSeries<string, decimal>
        {
            Label = "EMEA",
            Points = new[] { new BOBChartPoint<string, decimal>("Q1", 100m) }
        },
        new BOBChartSeries<string, decimal>
        {
            Label = "APAC",
            Points = new[] { new BOBChartPoint<string, decimal>("Q1", 60m) }
        }
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Legend_Item_Per_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2()));

        cut.FindAll("ul.bob-chart__legend > li.bob-chart__legend-item")
            .Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Legend_When_ShowLegend_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2())
                .Add(c => c.ShowLegend, false));

        cut.FindAll("ul.bob-chart__legend").Should().BeEmpty();
        cut.Find("bob-component").HasAttribute("data-bob-legend-position").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Legend_When_Position_None(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2())
                .Add(c => c.LegendPosition, BOBChartLegendPosition.None));

        cut.FindAll("ul.bob-chart__legend").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Position_Attribute_On_Host(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2())
                .Add(c => c.LegendPosition, BOBChartLegendPosition.Right));

        cut.Find("bob-component").GetAttribute("data-bob-legend-position").Should().Be("right");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Series_Visibility_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2()));

        // Both series visible: 2 bar rects (one per series at Q1).
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(2);

        // Click first legend button → first series hidden.
        cut.Find("button.bob-chart__legend-button").Click();

        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(1,
            because: "the first series was toggled hidden via the legend");
        cut.FindAll("li.bob-chart__legend-item[data-bob-hidden]").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnLegendToggle_Event(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? toggledLabel = null;

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2())
                .Add(c => c.OnLegendToggle, label => toggledLabel = label));

        cut.Find("button.bob-chart__legend-button").Click();

        toggledLabel.Should().Be("EMEA");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_AriaPressed_True_When_Visible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2()));

        cut.Find("button.bob-chart__legend-button")
            .GetAttribute("aria-pressed").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_AriaPressed_False_When_Hidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Series2()));

        cut.Find("button.bob-chart__legend-button").Click();

        cut.Find("li.bob-chart__legend-item[data-bob-hidden] button.bob-chart__legend-button")
            .GetAttribute("aria-pressed").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Legend_For_PieChart_From_Slices(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, new[]
                {
                    new BOBChartSlice<decimal> { Label = "A", Value = 30m },
                    new BOBChartSlice<decimal> { Label = "B", Value = 40m },
                    new BOBChartSlice<decimal> { Label = "C", Value = 30m },
                }));

        cut.FindAll("ul.bob-chart__legend > li.bob-chart__legend-item")
            .Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Slice_Visibility_On_Pie_Legend_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, new[]
                {
                    new BOBChartSlice<decimal> { Label = "A", Value = 30m },
                    new BOBChartSlice<decimal> { Label = "B", Value = 40m },
                    new BOBChartSlice<decimal> { Label = "C", Value = 30m },
                }));

        // 3 arcs visible.
        cut.FindAll("path.bob-pie-chart__arc").Should().HaveCount(3);

        cut.Find("button.bob-chart__legend-button").Click();

        // After hiding A, only B and C remain.
        cut.FindAll("path.bob-pie-chart__arc").Should().HaveCount(2);
    }
}
