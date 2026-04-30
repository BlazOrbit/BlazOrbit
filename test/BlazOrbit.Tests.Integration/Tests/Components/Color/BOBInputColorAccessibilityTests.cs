using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component Accessibility", "BOBInputColor")]
public class BOBInputColorAccessibilityTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Associate_Label_With_Input_Via_For_Id(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Background color")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement input = cut.Find("input.bob-input__field");
        IElement label = cut.Find("label.bob-input__label");

        string? id = input.GetAttribute("id");
        id.Should().NotBeNullOrWhiteSpace();
        label.GetAttribute("for").Should().Be(id);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_AriaLabel_From_Placeholder_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Placeholder, "#000000")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("aria-label").Should().Be("#000000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_AriaLabel_When_Label_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("aria-label").Should().BeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaInvalid_Matching_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, false));

        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, true));

        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaRequired_Matching_Required_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.Required, true)
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("aria-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper_Id(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.HelperText, "Pick a color")
            .Add(c => c.ValueExpression, () => model.Value));

        string? describedBy = cut.Find("input.bob-input__field").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find(".bob-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_AriaLabel_On_Color_Picker_Sliders(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Open the picker to render BOBColorPicker inline
        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.DisplayMode, ColorPickerDisplayMode.Dropdown));

        cut.Find("[aria-label='Open color picker']").Click();

        // BOBColorPicker's RGB inputs have aria-label
        cut.FindAll("input[aria-label]").Count.Should().BeGreaterThan(0);
    }
}
