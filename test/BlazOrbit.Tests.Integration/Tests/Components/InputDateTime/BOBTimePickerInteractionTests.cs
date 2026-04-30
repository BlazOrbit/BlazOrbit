using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Interaction", "BOBTimePicker")]
public class BOBTimePickerInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Increment_Hour(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly? captured = null;
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(10, 0))
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("button[aria-label='Increment hour']").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Value.Hour.Should().Be(11);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Decrement_Hour(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly? captured = null;
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(10, 0))
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("button[aria-label='Decrement hour']").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Value.Hour.Should().Be(9);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Increment_Minute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly? captured = null;
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(10, 30))
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("button[aria-label='Increment minute']").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Value.Minute.Should().Be(31);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Decrement_Minute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly? captured = null;
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(10, 30))
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("button[aria-label='Decrement minute']").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Value.Minute.Should().Be(29);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Format_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();
        string initialFormat = cut.FindAll("button").First().TextContent.Trim();

        // Act
        cut.FindAll("button").First().Click();

        // Assert
        string updatedFormat = cut.FindAll("button").First().TextContent.Trim();
        updatedFormat.Should().NotBe(initialFormat);
        updatedFormat.Should().BeOneOf("12h", "24h");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Wrap_Hour_From_23_To_0_On_Increment(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly? captured = null;
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(23, 0))
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("button[aria-label='Increment hour']").Click();

        // Assert
        captured!.Value.Hour.Should().Be(0);
    }
}
