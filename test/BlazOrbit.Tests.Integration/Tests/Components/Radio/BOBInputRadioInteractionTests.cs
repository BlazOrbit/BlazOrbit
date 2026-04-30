using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Radio;

[Trait("Component Interaction", "BOBInputRadio")]
public class BOBInputRadioInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_Option_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>();

        // Act — click second option
        cut.FindAll(".bob-radio__option")[1].Click();

        // Assert
        cut.Find(".selected-value").TextContent.Should().Be("opt2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Switch_Selection_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.SelectedValue, "opt1"));

        // Act — click third option
        cut.FindAll(".bob-radio__option")[2].Click();

        // Assert
        cut.Find(".selected-value").TextContent.Should().Be("opt3");
        cut.FindAll(".bob-radio__option")[0].GetAttribute("aria-checked").Should().Be("false");
        cut.FindAll(".bob-radio__option")[2].GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Select_When_Group_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Disabled, true));

        // Act
        cut.FindAll(".bob-radio__option")[0].Click();

        // Assert — no selection
        cut.Find(".selected-value").TextContent.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Select_Disabled_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Option3Disabled, true));

        // Act — click disabled option
        cut.FindAll(".bob-radio__option")[2].Click();

        // Assert
        cut.Find(".selected-value").TextContent.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_On_Space_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>();

        // Act
        cut.FindAll(".bob-radio__option")[0].KeyDown(key: " ");

        // Assert
        cut.Find(".selected-value").TextContent.Should().Be("opt1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_On_Enter_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>();

        // Act
        cut.FindAll(".bob-radio__option")[1].KeyDown(key: "Enter");

        // Assert
        cut.Find(".selected-value").TextContent.Should().Be("opt2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Clear_Button_When_Clearable_And_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Clearable, true)
            .Add(c => c.SelectedValue, "opt1"));

        // Assert — clear button visible
        cut.Markup.Should().Contain("Clear");
    }
}
