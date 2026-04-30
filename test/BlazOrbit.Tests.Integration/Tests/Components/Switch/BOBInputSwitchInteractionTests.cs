using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Switch;

[Trait("Component Interaction", "BOBInputSwitch")]
public class BOBInputSwitchInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act — click label wrapping the switch
        cut.Find("label.bob-switch").Click();

        // Assert
        captured.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Off_On_Second_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = true;
        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Value, true)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("label.bob-switch").Click();

        // Assert
        captured.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("label.bob-switch").Click();

        // Assert
        captured.Should().BeFalse();
    }
}
