using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component Rendering", "BOBInputColor")]
public class BOBInputColorRenderingTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-color");
        root.GetAttribute("data-bob-input-base").Should().NotBeNull();
        root.GetAttribute("data-bob-variant").Should().Be("outlined");
        root.GetAttribute("data-bob-size").Should().Be("medium");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Text_Input_Field(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Preview_Swatch(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find(".bob-input-color__preview-chess").Should().NotBeNull();
        cut.Find(".bob-input-color__preview-color").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_With_Current_Color(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#ff0000") };
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueExpression, () => model.Value));

        IElement preview = cut.Find(".bob-input-color__preview-color");
        preview.GetAttribute("style").Should().Contain("background-color:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Display_Value_In_Hex_Format_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#ff0000") };
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueExpression, () => model.Value));

        string? inputValue = cut.Find("input.bob-input__field").GetAttribute("value");
        inputValue.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_True_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#aabbcc") };
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.Value, new CssColor("#aabbcc"))
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_False_When_NoValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Background")
            .Add(c => c.HelperText, "Pick a color")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("label.bob-input__label").TextContent.Should().Contain("Background");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Pick a color");
    }
}
