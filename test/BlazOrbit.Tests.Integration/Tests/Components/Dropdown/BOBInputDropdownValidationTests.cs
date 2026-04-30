using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Validation", "BOBInputDropdown")]
public class BOBInputDropdownValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_No_Error_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<TestBOBInputDropdownValidationConsumer> cut =
            ctx.Render<TestBOBInputDropdownValidationConsumer>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().NotBe("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_After_Submit_Without_Selected_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBOBInputDropdownValidationConsumer> cut =
            ctx.Render<TestBOBInputDropdownValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Validation_Error_Message(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBOBInputDropdownValidationConsumer> cut =
            ctx.Render<TestBOBInputDropdownValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Find(".bob-field-helper--error").TextContent.Should().Contain("Please select an option");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Submit_When_No_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBOBInputDropdownValidationConsumer> cut =
            ctx.Render<TestBOBInputDropdownValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Instance.WasSubmitted.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Validation_When_Value_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBOBInputDropdownValidationConsumer> cut =
            ctx.Render<TestBOBInputDropdownValidationConsumer>();

        // Set model value directly (simulates selection)
        cut.Instance.BoundModel.Selected = "opt1";

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Instance.WasSubmitted.Should().BeTrue();
    }
}
