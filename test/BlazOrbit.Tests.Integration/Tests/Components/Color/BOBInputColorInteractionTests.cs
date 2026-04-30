using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component Interaction", "BOBInputColor")]
public class BOBInputColorInteractionTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Valid_Hex_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        CssColor? captured = null;
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("input.bob-input__field").Change("#ff0000");

        captured.Should().NotBeNull();
        captured!.ToString(ColorOutputFormats.Hex).Should().Be("#ff0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Value_On_Empty_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#ff0000") };
        CssColor? captured = new("#ff0000");
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("input.bob-input__field").Change(string.Empty);

        captured.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Picker_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.DisplayMode, ColorPickerDisplayMode.Dropdown));

        cut.FindAll(".bob-input-color__dropdown").Should().BeEmpty();

        // Act — click the palette button (last _BOBBtn inside wrapper)
        cut.Find("[aria-label='Open color picker']").Click();

        cut.Find(".bob-input-color__dropdown").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Picker_On_Overlay_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.DisplayMode, ColorPickerDisplayMode.Dropdown));

        cut.Find("[aria-label='Open color picker']").Click();
        cut.Find(".bob-input-color__dropdown").Should().NotBeNull();

        cut.Find(".bob-input-color__dropdown-overlay").Click();

        cut.FindAll(".bob-input-color__dropdown").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_On_Focus(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");

        cut.Find("input.bob-input__field").Focus();

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Open_Picker_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, true));

        cut.Find("[aria-label='Open color picker']").Click();

        cut.FindAll(".bob-input-color__dropdown").Should().BeEmpty();
    }
}
