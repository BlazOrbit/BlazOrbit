using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Interaction", "BOBThemeEditor")]
public class BOBThemeEditorInteractionTests
{
    private static Dictionary<string, CssColor> CreatePalette() => new()
    {
        ["Primary"] = new CssColor("#1A73E8"),
        ["PrimaryContrast"] = new CssColor("#FFFFFF"),
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnPaletteChanged_When_Color_Input_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Dictionary<string, CssColor>? captured = null;
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette())
            .Add(c => c.OnPaletteChanged, v => captured = v));

        // Act — change the first color input text field
        cut.FindAll("bob-component[data-bob-component='input-color'] input.bob-input__field")
           .First()
           .Change("#ff0000");

        // Assert — callback fired with mutated palette
        captured.Should().NotBeNull();
        captured!.Values.Should().Contain(c => c.ToString(ColorOutputFormats.Hex) == "#ff0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Other_Palette_Entries_On_Single_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Dictionary<string, CssColor>? captured = null;
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette())
            .Add(c => c.OnPaletteChanged, v => captured = v));

        // Act
        cut.FindAll("bob-component[data-bob-component='input-color'] input.bob-input__field")
           .First()
           .Change("#123456");

        // Assert — untouched entries remain at their original values
        captured.Should().NotBeNull();
        captured!.Should().ContainKey("PrimaryContrast");
        captured["PrimaryContrast"].ToString(ColorOutputFormats.Hex).Should().Be("#ffffff");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_On_Invalid_Color_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int calls = 0;
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette())
            .Add(c => c.OnPaletteChanged, _ => calls++));

        // Act — BOBInputColor rejects invalid hex by passing null to ValueChanged,
        // which HandleColorChanged ignores (guards against null).
        cut.FindAll("bob-component[data-bob-component='input-color'] input.bob-input__field")
           .First()
           .Change("not-a-color");

        // Assert
        calls.Should().Be(0);
    }
}
