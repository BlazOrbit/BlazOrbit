using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Otp;

[Trait("Component Validation", "BOBInputOtp")]
public class BOBInputOtpValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputOtpConsumer> cut = ctx.Render<TestBOBInputOtpConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_When_Required_Code_Is_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputOtpConsumer> cut = ctx.Render<TestBOBInputOtpConsumer>();

        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_For_Partial_Code(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputOtpConsumer> cut = ctx.Render<TestBOBInputOtpConsumer>();

        cut.FindAll(".bob-otp__slot")[0].Input("12");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("invalid");
        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Code_Is_Complete(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputOtpConsumer> cut = ctx.Render<TestBOBInputOtpConsumer>();

        // Paste a valid 6-digit code into the first slot.
        cut.FindAll(".bob-otp__slot")[0].Input("123456");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
        cut.Instance.BoundModel.Code.Should().Be("123456");
    }
}