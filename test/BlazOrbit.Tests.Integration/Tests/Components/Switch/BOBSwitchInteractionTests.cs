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

        bool? capturedValue = null;
        IRenderedComponent<BOBSwitch<bool>>[] cutHolder = new IRenderedComponent<BOBSwitch<bool>>[1];

        cutHolder[0] = ctx.Render<BOBSwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v =>
            {
                capturedValue = v;
                cutHolder[0].Render(p2 => p2
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Value, v));
            }));

        cutHolder[0].Find("bob-component").GetAttribute("data-bob-active").Should().BeNull();
        await cutHolder[0].Find("label").ClickAsync();

        capturedValue.Should().Be(true);
        cutHolder[0].Find("bob-component").GetAttribute("data-bob-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Back_To_Inactive_On_Second_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool? capturedValue = null;
        IRenderedComponent<BOBSwitch<bool>>[] cutHolder = new IRenderedComponent<BOBSwitch<bool>>[1];

        cutHolder[0] = ctx.Render<BOBSwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, true)
            .Add(c => c.ValueChanged, v =>
            {
                capturedValue = v;
                cutHolder[0].Render(p2 => p2
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Value, v));
            }));

        await cutHolder[0].Find("label").ClickAsync();

        capturedValue.Should().Be(false);
        cutHolder[0].Find("bob-component").GetAttribute("data-bob-active").Should().BeNull();
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