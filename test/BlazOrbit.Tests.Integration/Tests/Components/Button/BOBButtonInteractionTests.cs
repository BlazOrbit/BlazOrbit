using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Button;

[Trait("Component Interaction", "BOBButton")]
public class BOBButtonInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Click_Events(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int clickCount = 0;
        IRenderedComponent<TestBOBButtonConsumer> cut = ctx.Render<TestBOBButtonConsumer>(p => p
            .Add(c => c.ButtonText, "Click Me")
            .Add(c => c.OnButtonClicked, count => clickCount = count));

        // Act
        cut.Find("button").Click();

        // Assert
        clickCount.Should().Be(1);
        cut.Find(".click-count").TextContent.Should().Contain("1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_Click_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool clicked = false;
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Disabled")
            .Add(c => c.Disabled, true)
            .Add(c => c.OnClick, _ => clicked = true));

        // Act
        cut.Find("button").Click();

        // Assert
        clicked.Should().BeFalse();
    }
}