using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.PieChart;

[Trait("Component Rendering", "BOBPieChart")]
public class BOBPieChartRenderingTests
{
    private static IEnumerable<BOBChartSlice<decimal>> SampleSlices() =>
    [
        new BOBChartSlice<decimal> { Label = "EMEA", Value = 45m },
        new BOBChartSlice<decimal> { Label = "Americas", Value = 35m },
        new BOBChartSlice<decimal> { Label = "APAC", Value = 20m }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Family_And_Component_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        AngleSharp.Dom.IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("pie-chart");
        root.HasAttribute("data-bob-data-visualization-base").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Arc_Path_Per_Slice(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        cut.FindAll("path.bob-pie-chart__arc").Should().HaveCount(3);
        cut.FindAll(".bob-pie-chart__slice").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Wedge_Path_For_Pie(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        // Pie wedge: M(centre) L(outerStart) A(...) Z. The "L " segment is
        // the differentiator vs. a donut wedge (which has no centre line).
        string d = cut.FindAll("path.bob-pie-chart__arc").First().GetAttribute("d") ?? string.Empty;
        d.Should().StartWith("M ");
        d.Should().Contain(" L ", "pie wedges include a centre-to-rim L segment");
        d.Should().Contain(" A ", "the rim is drawn with an arc command");
        d.Should().EndWith(" Z");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Native_Title_With_Percentage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        // First slice: EMEA, 45 of 100 → 45.0%.
        string title = cut.FindAll("path.bob-pie-chart__arc > title").First().TextContent;
        title.Should().Contain("EMEA").And.Contain("45").And.Contain("%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Percentage_Labels_When_Slice_Above_5_Percent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        // All three slices (45/35/20) are well above the 5% threshold.
        cut.FindAll("text.bob-pie-chart__percentage").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Percentage_Labels_When_ShowPercentages_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices())
                .Add(c => c.ShowPercentages, false));

        cut.FindAll("text.bob-pie-chart__percentage").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Single_Circle_For_Single_Slice(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices, [new BOBChartSlice<decimal> { Label = "All", Value = 100m }]));

        // 360° wedge is degenerate in SVG → fallback to <circle>.
        cut.FindAll("circle").Should().NotBeEmpty();
        cut.FindAll("path.bob-pie-chart__arc").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_All_Values_Are_Zero(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices,
                [
                    new BOBChartSlice<decimal> { Label = "A", Value = 0m },
                        new BOBChartSlice<decimal> { Label = "B", Value = 0m }
                ]));

        cut.FindAll(".bob-pie-chart__slice").Should().BeEmpty();
    }
}