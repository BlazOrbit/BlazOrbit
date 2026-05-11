using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Password;

[Trait("Component Validation", "BOBInputPassword")]
public class BOBInputPasswordValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputPasswordConsumer> cut = ctx.Render<TestBOBInputPasswordConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_When_Required_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputPasswordConsumer> cut = ctx.Render<TestBOBInputPasswordConsumer>();

        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_For_Short_Password(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputPasswordConsumer> cut = ctx.Render<TestBOBInputPasswordConsumer>();

        cut.Find("input.bob-input__field").Change("short");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("invalid");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Long_Enough(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputPasswordConsumer> cut = ctx.Render<TestBOBInputPasswordConsumer>();

        cut.Find("input.bob-input__field").Change("longenoughpwd");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
        cut.Instance.BoundModel.Password.Should().Be("longenoughpwd");
    }
}
