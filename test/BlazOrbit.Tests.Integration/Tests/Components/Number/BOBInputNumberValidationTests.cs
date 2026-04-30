using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Number;

[Trait("Component Validation", "BOBInputNumber")]
public class BOBInputNumberValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberValidationConsumer> cut = ctx.Render<TestBOBInputNumberValidationConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Surface_Error_State_When_Validation_Fails(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberValidationConsumer> cut = ctx.Render<TestBOBInputNumberValidationConsumer>();

        // Trigger validation on empty required field
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find(".bob-field-helper--error").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_ValidationMessage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberValidationConsumer> cut = ctx.Render<TestBOBInputNumberValidationConsumer>();

        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find(".bob-field-helper--error");
        errorHelper.TextContent.Should().Contain("Qty is required");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Range_ValidationMessage_For_Out_Of_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberValidationConsumer> cut = ctx.Render<TestBOBInputNumberValidationConsumer>();

        cut.Find("input.bob-input__field").Input("99");
        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find(".bob-field-helper--error");
        errorHelper.TextContent.Should().Contain("Qty must be between 1 and 10");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_State_After_Valid_Value_Entered(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberValidationConsumer> cut = ctx.Render<TestBOBInputNumberValidationConsumer>();

        // Provoke failure
        cut.Find("button.submit-btn").Click();
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");

        // Fix value and re-validate
        cut.Find("input.bob-input__field").Input("5");
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_To_Bound_Model(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberValidationConsumer> cut = ctx.Render<TestBOBInputNumberValidationConsumer>();

        cut.Find("input.bob-input__field").Input("7");

        cut.Instance.BoundModel.Qty.Should().Be(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Value_Passes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberValidationConsumer> cut = ctx.Render<TestBOBInputNumberValidationConsumer>();

        cut.Find("input.bob-input__field").Input("5");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }
}
