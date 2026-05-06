using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Text;

[Trait("Component Rendering", "BOBInputText")]
public class BOBInputTextRenderingTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.ValueExpression, () => model.Value));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-text");
        root.GetAttribute("data-bob-input-base").Should().NotBeNull();
        root.GetAttribute("data-bob-variant").Should().Be("outlined");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-density").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Input_Field_With_Label_And_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = "hello" };
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Value, "hello")
            .Add(c => c.Label, "Full name")
            .Add(c => c.HelperText, "As it appears on your ID.")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("value").Should().Be("hello");
        cut.Find("label.bob-input__label").TextContent.Should().Contain("Full name");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("As it appears on your ID.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_False_When_Empty_And_Unfocused(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
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
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
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
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Label, "Website")
            .Add(c => c.PrefixText, "https://")
            .Add(c => c.SuffixText, ".com")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find(".bob-input__addon--prefix").TextContent.Should().Contain("https://");
        cut.Find(".bob-input__addon--suffix").TextContent.Should().Contain(".com");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Design_DataAttributes_And_InlineVars(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
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

        // Structural guard: the native <input> carries `bob-input__field` so the global
        // rule `bob-component[data-bob-input-base] .bob-input__field { color: var(--bob-inline-color, inherit) }`
        // matches it (audited by INPUT-COLOR-01).
        IElement field = cut.Find("input.bob-input__field");
        field.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Placeholder, "Write here")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.FindAll("label.bob-input__label").Should().BeEmpty();
        cut.Find("input.bob-input__field").GetAttribute("aria-label").Should().Be("Write here");
    }
}
