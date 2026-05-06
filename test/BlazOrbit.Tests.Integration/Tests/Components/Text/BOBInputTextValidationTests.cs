using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Text;

[Trait("Component Validation", "BOBInputText")]
public class BOBInputTextValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextConsumer> cut = ctx.Render<TestBOBInputTextConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().BeNull();
        cut.FindAll("[data-bob-error=\"true\"]").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Surface_Error_State_When_Validation_Fails(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextConsumer> cut = ctx.Render<TestBOBInputTextConsumer>();

        // Trigger validation on empty required field
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("[data-bob-error=\"true\"]").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_ValidationMessage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextConsumer> cut = ctx.Render<TestBOBInputTextConsumer>();

        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find("[data-bob-error=\"true\"]");
        errorHelper.TextContent.Should().Contain("Email is required");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_EmailAddress_ValidationMessage_For_Invalid_Format(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextConsumer> cut = ctx.Render<TestBOBInputTextConsumer>();

        cut.Find("input.bob-input__field").Change("not-an-email");
        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find("[data-bob-error=\"true\"]");
        errorHelper.TextContent.Should().Contain("Invalid email format");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_State_After_Valid_Value_Entered(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextConsumer> cut = ctx.Render<TestBOBInputTextConsumer>();

        // Provoke failure
        cut.Find("button.submit-btn").Click();
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");

        // Fix value and re-validate
        cut.Find("input.bob-input__field").Change("user@example.com");
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().BeNull();
        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("false");
        cut.FindAll("[data-bob-error=\"true\"]").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_To_Bound_Model(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextConsumer> cut = ctx.Render<TestBOBInputTextConsumer>();

        cut.Find("input.bob-input__field").Change("user@example.com");

        cut.Instance.BoundModel.Email.Should().Be("user@example.com");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Value_Passes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextConsumer> cut = ctx.Render<TestBOBInputTextConsumer>();

        cut.Find("input.bob-input__field").Change("user@example.com");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }
}
