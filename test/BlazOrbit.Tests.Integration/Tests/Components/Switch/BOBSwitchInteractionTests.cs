using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Switch;

[Trait("Component Interaction", "BOBSwitch")]
public class BOBSwitchInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_To_Active_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? capturedValue = null;
        IRenderedComponent<BOBSwitch<bool>> cut = null!;
        cut = ctx.Render<BOBSwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v =>
            {
                capturedValue = v;
                cut.Render(p2 => p2
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Value, v));
            }));

        cut.Find("bob-component").GetAttribute("data-bob-active").Should().BeNull();

        // Act
        cut.Find("label").Click();

        // Assert
        capturedValue.Should().Be(true);
        cut.Find("bob-component").GetAttribute("data-bob-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Back_To_Inactive_On_Second_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? capturedValue = null;
        IRenderedComponent<BOBSwitch<bool>> cut = null!;
        cut = ctx.Render<BOBSwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, true)
            .Add(c => c.ValueChanged, v =>
            {
                capturedValue = v;
                cut.Render(p2 => p2
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Value, v));
            }));

        // Act
        cut.Find("label").Click();

        // Assert
        capturedValue.Should().Be(false);
        cut.Find("bob-component").GetAttribute("data-bob-active").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_ValueChanged_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool fired = false;
        IRenderedComponent<BOBSwitch<bool>> cut = ctx.Render<BOBSwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, _ => fired = true));

        // Act
        cut.Find("label").Click();

        // Assert
        fired.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Work_With_String_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedValue = null;
        IRenderedComponent<BOBSwitch<string>> cut = ctx.Render<BOBSwitch<string>>(p => p
            .Add(c => c.OptionInactive, "off")
            .Add(c => c.OptionActive, "on")
            .Add(c => c.Value, "off")
            .Add(c => c.ValueChanged, v => capturedValue = v));

        // Act
        cut.Find("label").Click();

        // Assert
        capturedValue.Should().Be("on");
    }
}
