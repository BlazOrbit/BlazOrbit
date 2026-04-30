using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Accessibility", "BOBTimePicker")]
public class BOBTimePickerAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Label_Hour_Stepper_Buttons(BlazorScenario scenario)
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
    public async Task Should_Label_Minute_Stepper_Buttons(BlazorScenario scenario)
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
    public async Task Should_Render_Stepper_Buttons_With_Type_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert — type="button" stops accidental form submission when used inside <form>
        foreach (string label in new[] { "Increment hour", "Decrement hour", "Increment minute", "Decrement minute" })
        {
            IElement btn = cut.Find($"button[aria-label='{label}']");
            btn.TagName.Should().Be("BUTTON");
            btn.GetAttribute("type").Should().Be("button");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Limit_Hour_And_Minute_Inputs_To_Two_Characters(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert — maxlength keeps keyboard users from typing invalid 3-digit values
        IReadOnlyList<IElement> inputs = cut.FindAll("input");
        inputs.Should().HaveCountGreaterThanOrEqualTo(2);
        inputs.Should().OnlyContain(i => i.GetAttribute("maxlength") == "2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Stepper_Buttons_Keyboard_Focusable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert
        foreach (string label in new[] { "Increment hour", "Decrement hour", "Increment minute", "Decrement minute" })
        {
            cut.Find($"button[aria-label='{label}']").GetAttribute("tabindex").Should().Be("0");
        }
    }
}
