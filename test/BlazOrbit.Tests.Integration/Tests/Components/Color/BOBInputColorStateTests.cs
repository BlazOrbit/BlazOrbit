using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component State", "BOBInputColor")]
public class BOBInputColorStateTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, true));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-readonly").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("readonly").Should().BeTrue();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, false));

        root.GetAttribute("data-bob-readonly").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-loading").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, true));

        root.GetAttribute("data-bob-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Preview_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.Value, new CssColor("#0000ff"))
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Open_Picker_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, true));

        cut.FindAll(".bob-input-color__dropdown").Should().BeEmpty();
        cut.FindAll(".bob-input-color__dropdown-overlay").Should().BeEmpty();
    }
}
