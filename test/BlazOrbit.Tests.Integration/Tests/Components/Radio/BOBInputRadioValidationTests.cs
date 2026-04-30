using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Radio;

[Trait("Component Validation", "BOBInputRadio")]
public class BOBInputRadioValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioValidationConsumer> cut =
            ctx.Render<TestBOBInputRadioValidationConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_When_Required_Selection_Is_Missing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioValidationConsumer> cut =
            ctx.Render<TestBOBInputRadioValidationConsumer>();

        // Submit without selection
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
        cut.Find(".bob-field-helper--error").TextContent.Should().Contain("You must pick an option");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_After_Valid_Selection(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioValidationConsumer> cut =
            ctx.Render<TestBOBInputRadioValidationConsumer>();

        // Provoke error
        cut.Find("button.submit-btn").Click();
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");

        // Pick first option
        cut.FindAll(".bob-radio__option")[0].Click();

        // Re-submit
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Selection_Is_Made(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioValidationConsumer> cut =
            ctx.Render<TestBOBInputRadioValidationConsumer>();

        cut.FindAll(".bob-radio__option")[1].Click();
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_To_Bound_Model(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioValidationConsumer> cut =
            ctx.Render<TestBOBInputRadioValidationConsumer>();

        cut.Instance.BoundModel.Choice.Should().BeNull();

        cut.FindAll(".bob-radio__option")[2].Click();

        cut.Instance.BoundModel.Choice.Should().Be("opt3");
    }
}
