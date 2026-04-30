using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component Rendering", "BOBColorPicker")]
public class BOBColorPickerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Picker_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("color-picker");
        root.GetAttribute("data-bob-picker-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Hue_Slider(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        cut.Find(".bob-colorpicker__slider--hue input[type='range']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Alpha_Slider(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        cut.Find(".bob-colorpicker__slider--alpha input[type='range']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Preview(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>();

        cut.Find(".bob-picker__preview").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Rgb_Inputs_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Rgba));

        // RGB mode renders 4 inline number inputs (R, G, B, A)
        cut.FindAll("input[aria-label]").Count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Hex_Input_When_Hex_Format(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex));

        cut.Find(".bob-picker__input").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Actions_When_ShowActions_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.ShowActions, true)
            .Add(c => c.RevertText, "Undo"));

        cut.Markup.Should().Contain("Undo");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Actions_When_ShowActions_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.ShowActions, false));

        cut.FindAll(".bob-picker__row").Should().HaveCount(1);
    }
}
