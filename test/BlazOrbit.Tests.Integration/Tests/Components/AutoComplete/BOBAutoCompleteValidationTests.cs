using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.AutoComplete;

[Trait("Component Validation", "BOBAutoComplete")]
public class BOBAutoCompleteValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBAutoCompleteConsumer> cut = ctx.Render<TestBOBAutoCompleteConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_When_Required_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBAutoCompleteConsumer> cut = ctx.Render<TestBOBAutoCompleteConsumer>();

        cut.Find("button.submit-btn").Click();

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Item_Selected(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBAutoCompleteConsumer> cut = ctx.Render<TestBOBAutoCompleteConsumer>();

        cut.Find("input.bob-input__field").Input("Sp");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(1));
        cut.Find(".bob-autocomplete__option").MouseDown();

        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
        cut.Instance.BoundModel.Country.Should().Be("Spain");
    }
}
