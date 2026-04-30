using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Validation", "BOBInputDateTime")]
public class BOBInputDateTimeValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_No_Error_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<TestBOBInputDateTimeValidationConsumer> cut =
            ctx.Render<TestBOBInputDateTimeValidationConsumer>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().NotBe("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_After_Submit_Without_Required_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBOBInputDateTimeValidationConsumer> cut =
            ctx.Render<TestBOBInputDateTimeValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Validation_Message_After_Invalid_Submit(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBOBInputDateTimeValidationConsumer> cut =
            ctx.Render<TestBOBInputDateTimeValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Find(".bob-field-helper--error").Should().NotBeNull();
        cut.Find(".bob-field-helper--error").TextContent.Should().Contain("Date is required");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Submit_When_Required_Value_Missing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBOBInputDateTimeValidationConsumer> cut =
            ctx.Render<TestBOBInputDateTimeValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Instance.WasSubmitted.Should().BeFalse();
        cut.Find(".submit-result").TextContent.Should().Be("invalid");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Validation_When_Value_Is_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — render with initial valid date so validation passes
        IRenderedComponent<TestBOBInputDateTimeValidationConsumer> cut =
            ctx.Render<TestBOBInputDateTimeValidationConsumer>(p => p
                .Add(c => c.InitialDate, new DateOnly(2024, 6, 15)));

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
        cut.Instance.WasSubmitted.Should().BeTrue();
    }
}
