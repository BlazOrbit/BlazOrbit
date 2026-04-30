using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Rendering", "BOBTimePicker")]
public class BOBTimePickerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_TimePicker_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("time-picker");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Picker_Family_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-picker-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Hour_Increment_And_Decrement_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert
        cut.Find("button[aria-label='Increment hour']").Should().NotBeNull();
        cut.Find("button[aria-label='Decrement hour']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Minute_Increment_And_Decrement_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert
        cut.Find("button[aria-label='Increment minute']").Should().NotBeNull();
        cut.Find("button[aria-label='Decrement minute']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Colon_Separator(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert
        cut.Find(".bob-picker__separator").TextContent.Should().Be(":");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Format_Toggle_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert — format button shows either "12h" or "24h"
        string buttonText = cut.FindAll("button").First().TextContent.Trim();
        buttonText.Should().BeOneOf("12h", "24h");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Hour_And_Minute_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(14, 30)));

        // Assert — markup contains hour and minute
        cut.Markup.Should().Contain("30");
    }
}
