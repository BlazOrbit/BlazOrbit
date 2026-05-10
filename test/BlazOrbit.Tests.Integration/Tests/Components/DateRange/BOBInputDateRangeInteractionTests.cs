using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DateRange;

[Trait("Component Interaction", "BOBInputDateRange")]
public class BOBInputDateRangeInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Last7Days_Preset_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        global::BlazOrbit.Components.DateRange? captured = null;
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.ValueChanged, v => captured = v));

        // First preset button = "Last 7 days".
        cut.FindAll(".bob-daterange__presets button")[0].Click();

        captured.Should().NotBeNull();
        captured!.Value.DayCount.Should().Be(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Last30Days_Preset_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        global::BlazOrbit.Components.DateRange? captured = null;
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-daterange__presets button")[1].Click();

        captured!.Value.DayCount.Should().Be(30);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Value_On_Clear_Preset(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        DateOnly s = new(2026, 1, 1);
        DateOnly e = new(2026, 1, 7);
        global::BlazOrbit.Components.DateRange? captured = null;
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Value, new global::BlazOrbit.Components.DateRange(s, e))
            .Add(c => c.ValueChanged, v => captured = v));

        // Last preset = "Clear".
        cut.FindAll(".bob-daterange__presets button")[3].Click();

        captured!.Value.IsEmpty.Should().BeTrue();
    }
}
