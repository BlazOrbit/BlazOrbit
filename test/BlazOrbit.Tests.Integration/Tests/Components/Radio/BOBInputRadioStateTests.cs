using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Radio;

[Trait("Component State", "BOBInputRadio")]
public class BOBInputRadioStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Value_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.SelectedValue, "opt1"));

        IReadOnlyList<IElement> options = cut.FindAll(".bob-radio__option");
        options[0].GetAttribute("aria-checked").Should().Be("true");

        cut.Render(p => p.Add(c => c.SelectedValue, "opt3"));

        options = cut.FindAll(".bob-radio__option");
        options[0].GetAttribute("aria-checked").Should().Be("false");
        options[2].GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().Be("false");

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        // All options disabled
        cut.FindAll(".bob-radio__option").Should().OnlyContain(o => o.GetAttribute("aria-disabled") == "true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Error, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().Be("false");

        cut.Render(p => p.Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Change_Orientation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Orientation, RadioOrientation.Vertical));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("vertical");

        cut.Render(p => p.Add(c => c.Orientation, RadioOrientation.Horizontal));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("horizontal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Single_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Option3Disabled, true));

        IReadOnlyList<IElement> options = cut.FindAll(".bob-radio__option");
        options[0].GetAttribute("aria-disabled").Should().Be("false");
        options[1].GetAttribute("aria-disabled").Should().Be("false");
        options[2].GetAttribute("aria-disabled").Should().Be("true");
    }
}
