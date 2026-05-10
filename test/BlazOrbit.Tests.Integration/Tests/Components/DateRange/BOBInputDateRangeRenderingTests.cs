using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DateRange;

[Trait("Component Rendering", "BOBInputDateRange")]
public class BOBInputDateRangeRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>();

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("input-date-range");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Two_Date_Picker_Panes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>();

        cut.FindAll(".bob-daterange__pane").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Presets_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>();

        cut.FindAll(".bob-daterange__presets").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Presets_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.ShowPresets, false));

        cut.FindAll(".bob-daterange__presets").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Summary_When_Range_Complete(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        DateOnly start = new(2026, 1, 1);
        DateOnly end = new(2026, 1, 7);
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Value, new global::BlazOrbit.Components.DateRange(start, end)));

        cut.Find(".bob-daterange__summary").TextContent.Should().Contain("7");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Summary_When_Range_Incomplete(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>();

        cut.FindAll(".bob-daterange__summary").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Label, "Reporting period"));

        cut.Find(".bob-daterange__label").TextContent.Should().Contain("Reporting period");
    }
}
