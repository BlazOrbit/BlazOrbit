using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component State", "BOBColorPicker")]
public class BOBColorPickerStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Initialize_With_Provided_Color(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex));

        string inputValue = cut.Find(".bob-picker__input").GetAttribute("value") ?? string.Empty;
        inputValue.Should().StartWith("#ff0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Cycle_Format_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Start in Hex with no actions row so only one .bob-picker__row exists
        IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex)
            .Add(c => c.ShowActions, false));

        // Hex format renders a single text input
        cut.FindAll("input[type='text'].bob-picker__input").Should().HaveCount(1);
        cut.FindAll("input[type='number'].bob-picker__input").Should().BeEmpty();

        // Click the sync/cycle button — last button inside the single inputs row
        cut.FindAll(".bob-picker__row button").Last().Click();

        // After cycling from Hex, format switches to Rgb — four numeric inputs (R, G, B, A) appear
        cut.FindAll("input[type='text'].bob-picker__input").Should().BeEmpty();
        cut.FindAll("input[type='number'].bob-picker__input").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Different_Preview_For_Different_Initial_Colors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Two instances initialized with different colors show different preview styles
        IRenderedComponent<BOBColorPicker> cut1 = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#000000")));

        IRenderedComponent<BOBColorPicker> cut2 = ctx.Render<BOBColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#ffffff")));

        string? style1 = cut1.Find(".bob-picker__preview div").GetAttribute("style");
        string? style2 = cut2.Find(".bob-picker__preview div").GetAttribute("style");
        style2.Should().NotBe(style1);
    }
}
