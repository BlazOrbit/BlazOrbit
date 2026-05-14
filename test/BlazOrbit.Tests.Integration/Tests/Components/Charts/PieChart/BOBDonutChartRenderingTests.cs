using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.PieChart;

[Trait("Component Rendering", "BOBDonutChart")]
public class BOBDonutChartRenderingTests
{
    private static IEnumerable<BOBChartSlice<decimal>> SampleSlices() =>
    [
        new BOBChartSlice<decimal> { Label = "Active", Value = 60m },
        new BOBChartSlice<decimal> { Label = "Idle", Value = 25m },
        new BOBChartSlice<decimal> { Label = "Error", Value = 15m }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Family_And_Component_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDonutChart<decimal>> cut =
            ctx.Render<BOBDonutChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        AngleSharp.Dom.IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("donut-chart");
        root.HasAttribute("data-bob-data-visualization-base").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_InnerRadius_To_06(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDonutChart<decimal>> cut =
            ctx.Render<BOBDonutChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        cut.Instance.InnerRadius.Should().Be(0.6,
            "donut differs from pie by defaulting InnerRadius to 0.6 in its constructor");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Annular_Wedge_Path(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDonutChart<decimal>> cut =
            ctx.Render<BOBDonutChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        // Donut wedge: M(outerStart) A(outerEnd) L(innerEnd) A(innerStart) Z.
        // Two arc commands (outer + inner), no centre L.
        string d = cut.FindAll("path.bob-pie-chart__arc").First().GetAttribute("d") ?? string.Empty;

        // Two arcs in the path: outer + inner.
        int arcCount = System.Text.RegularExpressions.Regex.Matches(d, @"\bA\b").Count;
        arcCount.Should().Be(2, "donut wedges have two arcs (outer rim + inner rim)");

        d.Should().Contain(" L ", "the L segment connects outer-end to inner-end");
        d.Should().EndWith(" Z");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Native_Title_With_Percentage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDonutChart<decimal>> cut =
            ctx.Render<BOBDonutChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices()));

        string title = cut.FindAll("path.bob-pie-chart__arc > title").First().TextContent;
        title.Should().Contain("Active").And.Contain("60").And.Contain("%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_Custom_InnerRadius_Override(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDonutChart<decimal>> cut =
            ctx.Render<BOBDonutChart<decimal>>(p => p
                .Add(c => c.Slices, SampleSlices())
                .Add(c => c.InnerRadius, 0.3));

        cut.Instance.InnerRadius.Should().Be(0.3);
        cut.FindAll("path.bob-pie-chart__arc").Should().HaveCount(3);
    }
}