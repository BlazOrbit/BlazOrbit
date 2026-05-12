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

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Bind_Free_Text_Through_StringToItem_On_Blur(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Resolver accepts any non-empty string verbatim — exercises the "free-text address"
        // pattern where the autocomplete suggests known matches but the field still accepts
        // novel entries the consumer wants stored on the bound model.
        IRenderedComponent<TestBOBAutoCompleteConsumer> cut = ctx.Render<TestBOBAutoCompleteConsumer>(p => p
            .Add(c => c.StringToItem, raw => raw));

        cut.Find("input.bob-input__field").Input("Atlantis");
        cut.Find("input.bob-input__field").Blur();

        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
        cut.Instance.BoundModel.Country.Should().Be("Atlantis");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Surface_Validation_When_StringToItem_Rejects(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Resolver rejects everything — should land the field in an error state once the
        // user commits via blur. The custom UnresolvedStringError surfaces as the validation
        // message (assertion is on the data-bob-error flag since the message DOM landing
        // is covered elsewhere).
        IRenderedComponent<TestBOBAutoCompleteConsumer> cut = ctx.Render<TestBOBAutoCompleteConsumer>(p => p
            .Add(c => c.StringToItem, _ => null)
            .Add(c => c.UnresolvedStringError, "Pick a known country"));

        cut.Find("input.bob-input__field").Input("Atlantis");
        cut.Find("input.bob-input__field").Blur();

        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("invalid");
    }
}
