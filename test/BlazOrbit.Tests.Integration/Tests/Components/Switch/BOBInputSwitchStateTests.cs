using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Switch;

[Trait("Component State", "BOBInputSwitch")]
public class BOBInputSwitchStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Value_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Value, false));

        cut.Find("input.bob-switch__input").GetAttribute("aria-checked").Should().Be("false");

        cut.Render(p => p.Add(c => c.Value, true));

        cut.Find("input.bob-switch__input").GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().Be("false");
        cut.Find("input.bob-switch__input").HasAttribute("disabled").Should().BeFalse();

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("input.bob-switch__input").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.ReadOnly, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-readonly").Should().Be("false");

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        root.GetAttribute("data-bob-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Error, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().Be("false");

        cut.Render(p => p.Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Label, "Old")
            .Add(c => c.HelperText, "Old help"));

        cut.Find(".bob-switch__label").TextContent.Should().Contain("Old");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Old help");

        cut.Render(p => p
            .Add(c => c.Label, "New")
            .Add(c => c.HelperText, "New help"));

        cut.Find(".bob-switch__label").TextContent.Should().Contain("New");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("New help");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Dictionary<string, object> extra = new()
        {
            { "data-testid", "toggle-switch" },
            { "class", "my-switch" },
            { "style", "margin: 4px;" }
        };

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.AdditionalAttributes, extra));

        cut.Render(p => p
            .Add(c => c.AdditionalAttributes, extra)
            .Add(c => c.Disabled, true));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-testid").Should().Be("toggle-switch");
        root.ClassList.Should().Contain("my-switch");
        root.GetAttribute("style").Should().Contain("margin: 4px;");
    }
}
