using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TextArea;

[Trait("Component Validation", "BOBInputTextArea")]
public class BOBInputTextAreaValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextAreaConsumer> cut = ctx.Render<TestBOBInputTextAreaConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Surface_Error_State_When_Validation_Fails(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextAreaConsumer> cut = ctx.Render<TestBOBInputTextAreaConsumer>();

        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
        cut.Find("textarea.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find(".bob-field-helper--error").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_ValidationMessage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextAreaConsumer> cut = ctx.Render<TestBOBInputTextAreaConsumer>();

        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find(".bob-field-helper--error");
        errorHelper.TextContent.Should().Contain("Bio is required");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_StringLength_ValidationMessage_For_Too_Long(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextAreaConsumer> cut = ctx.Render<TestBOBInputTextAreaConsumer>();

        cut.Find("textarea.bob-input__field").Change(new string('x', 25));
        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find(".bob-field-helper--error");
        errorHelper.TextContent.Should().Contain("Too long");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_State_After_Valid_Value_Entered(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextAreaConsumer> cut = ctx.Render<TestBOBInputTextAreaConsumer>();

        cut.Find("button.submit-btn").Click();
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");

        cut.Find("textarea.bob-input__field").Change("short bio");
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.Find("textarea.bob-input__field").GetAttribute("aria-invalid").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_To_Bound_Model(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextAreaConsumer> cut = ctx.Render<TestBOBInputTextAreaConsumer>();

        cut.Find("textarea.bob-input__field").Change("hello");

        cut.Instance.BoundModel.Bio.Should().Be("hello");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Value_Passes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputTextAreaConsumer> cut = ctx.Render<TestBOBInputTextAreaConsumer>();

        cut.Find("textarea.bob-input__field").Change("valid bio");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }
}
