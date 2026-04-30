using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Validation", "BOBInputCheckbox")]
public class BOBInputCheckboxValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputCheckboxConsumer> cut = ctx.Render<TestBOBInputCheckboxConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_When_Required_Bool_Is_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputCheckboxConsumer> cut = ctx.Render<TestBOBInputCheckboxConsumer>();

        // Submit without checking
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
        cut.Find(".bob-field-helper--error").TextContent.Should().Contain("You must accept the terms");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_After_Checking(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputCheckboxConsumer> cut = ctx.Render<TestBOBInputCheckboxConsumer>();

        // Provoke error
        cut.Find("button.submit-btn").Click();
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");

        // Check the box
        cut.Find(".bob-checkbox").Click();

        // Re-submit
        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("false");
        cut.FindAll(".bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Checked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputCheckboxConsumer> cut = ctx.Render<TestBOBInputCheckboxConsumer>();

        // Check and submit
        cut.Find(".bob-checkbox").Click();
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_To_Bound_Model(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputCheckboxConsumer> cut = ctx.Render<TestBOBInputCheckboxConsumer>();

        cut.Instance.BoundModel.Accepted.Should().BeFalse();

        cut.Find(".bob-checkbox").Click();

        cut.Instance.BoundModel.Accepted.Should().BeTrue();
    }
}
