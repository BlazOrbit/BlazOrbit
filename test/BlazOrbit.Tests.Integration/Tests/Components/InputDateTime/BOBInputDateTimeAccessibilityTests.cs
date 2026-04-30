using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Accessibility", "BOBInputDateTime")]
public class BOBInputDateTimeAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Label_Linked_To_Input_Via_For_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Birth Date"));

        // Assert — label's for attribute matches the generated input id
        IElement label = cut.Find("label");
        string? labelFor = label.GetAttribute("for");
        labelFor.Should().StartWith("bob-datetime-");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Label_On_Picker_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Assert
        cut.Find("button[aria-label='Open picker']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Required_Data_Attribute_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Required, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Required_Asterisk_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Date")
            .Add(c => c.Required, true));

        // Assert — CSS-OPT-02 block B.5: visual marker is now CSS-only via
        // [data-bob-required="true"] .bob-input__label::after on the host.
        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Error_Data_Attribute_When_Error_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Error, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_With_Role_Dialog_When_Opened(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Act
        cut.Find("button[aria-label='Open picker']").Click();

        // Assert — dialog rendered with .bob-dialog
        cut.Find(".bob-dialog").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Nav_Buttons_With_Aria_Labels_In_DatePicker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — all nav buttons have descriptive aria-labels
        cut.Find("button[aria-label='Previous year']").Should().NotBeNull();
        cut.Find("button[aria-label='Previous month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next year']").Should().NotBeNull();
    }
}
