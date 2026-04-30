using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component Accessibility", "BOBColorPicker")]
public class BOBColorPickerAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Selection_Region_Should_Be_Focusable_Via_TabIndex(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        IElement selection = cut.Find(".bob-colorpicker__selection");
        selection.GetAttribute("tabindex").Should().Be("0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Hue_Slider_Should_Expose_Min_Max_Step(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        IElement hue = cut.Find(".bob-colorpicker__slider--hue input[type='range']");
        hue.GetAttribute("min").Should().Be("0");
        hue.GetAttribute("max").Should().Be("360");
        hue.GetAttribute("step").Should().Be("1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Alpha_Slider_Should_Expose_Min_Max_Step(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        IElement alpha = cut.Find(".bob-colorpicker__slider--alpha input[type='range']");
        alpha.GetAttribute("min").Should().Be("0");
        alpha.GetAttribute("max").Should().Be("255");
        alpha.GetAttribute("step").Should().Be("1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Rgba_Number_Inputs_Should_Expose_AriaLabels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Rgba));

        IReadOnlyList<IElement> labelledInputs = cut.FindAll("input[aria-label]");
        labelledInputs.Select(i => i.GetAttribute("aria-label"))
            .Should().Contain(new[] { "Red", "Green", "Blue", "Alpha" });
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Hex_Text_Input_Should_Use_Text_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex));

        IElement hex = cut.Find(".bob-picker__input");
        hex.GetAttribute("type").Should().Be("text");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Hue_And_Alpha_Sliders_Should_Be_Keyboard_Reachable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        IElement hue = cut.Find(".bob-colorpicker__slider--hue input[type='range']");
        IElement alpha = cut.Find(".bob-colorpicker__slider--alpha input[type='range']");

        // Native range inputs are keyboard-reachable by default; verify they are not excluded
        hue.GetAttribute("tabindex").Should().NotBe("-1");
        alpha.GetAttribute("tabindex").Should().NotBe("-1");
    }
}
