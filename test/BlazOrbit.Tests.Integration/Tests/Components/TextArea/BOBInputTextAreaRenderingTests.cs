using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TextArea;

[Trait("Component Rendering", "BOBInputTextArea")]
public class BOBInputTextAreaRenderingTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-text-area");
        root.GetAttribute("data-bob-input-base").Should().NotBeNull();
        root.GetAttribute("data-bob-variant").Should().Be("outlined");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-density").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiResize_Vertical_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-resize").Should().Be("vertical");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiAutoResize_False_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-autoresize").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Textarea_With_Rows_And_MaxLength(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Rows, 6)
            .Add(c => c.MaxLength, 200)
            .Add(c => c.ValueExpression, () => model.Value));

        IElement textarea = cut.Find("textarea.bob-input__field");
        textarea.GetAttribute("rows").Should().Be("6");
        textarea.GetAttribute("maxlength").Should().Be("200");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Textarea_With_Label_And_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = "hello" };
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Value, "hello")
            .Add(c => c.Label, "Bio")
            .Add(c => c.HelperText, "A short bio.")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bob-input__field").GetAttribute("value").Should().Be("hello");
        cut.Find("label.bob-input__label").TextContent.Should().Contain("Bio");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("A short bio.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_False_When_Empty_And_Unfocused(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Label, "Empty")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_True_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = "prefilled" };
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Value, "prefilled")
            .Add(c => c.Label, "With value")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Prefix_And_Suffix_Addons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.PrefixText, "PRE")
            .Add(c => c.SuffixText, "SUF")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find(".bob-input__addon--prefix").TextContent.Should().Contain("PRE");
        cut.Find(".bob-input__addon--suffix").TextContent.Should().Contain("SUF");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Design_DataAttributes_And_InlineVars(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Label, "Styled")
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.Color, "rgba(10,20,30,1)")
            .Add(c => c.BackgroundColor, "rgba(40,50,60,1)")
            .Add(c => c.Shadow, BOBShadowPresets.Elevation(2))
            .Add(c => c.ValueExpression, () => model.Value));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-size").Should().Be("large");
        root.GetAttribute("data-bob-density").Should().Be("compact");
        root.GetAttribute("data-bob-shadow").Should().Be("true");

        string style = root.GetAttribute("style") ?? string.Empty;
        style.Should().Contain("--bob-inline-color: rgba(10,20,30,1)");
        style.Should().Contain("--bob-inline-background: rgba(40,50,60,1)");
        style.Should().Contain("--bob-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Placeholder, "Write here")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.FindAll("label.bob-input__label").Should().BeEmpty();
        // focus so placeholder is rendered (IsFloated = true when focused)
        cut.Find("textarea.bob-input__field").Focus();
        cut.Find("textarea.bob-input__field").GetAttribute("aria-label").Should().Be("Write here");
    }
}
