using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// LIB-05: BOBShadowPresets.Elevation(n) generates valid CSS shadow strings.
/// </summary>
[Trait("Library", "BOBShadowPresets")]
public class BOBShadowPresetsTests
{
    [Fact]
    public void Elevation_Zero_Should_Produce_Empty_Shadow()
    {
        // Arrange & Act
        ShadowStyle shadow = BOBShadowPresets.Elevation(0);

        // Assert — level 0 → no visible shadow (0px 0px 0px 0px)
        string css = shadow.ToCss();
        css.Should().Contain("0px 0px 0px 0px");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(24)]
    public void Elevation_NonZero_Should_Produce_Multi_Layer_Shadow(int level)
    {
        // Arrange & Act
        ShadowStyle shadow = BOBShadowPresets.Elevation(level);
        string css = shadow.ToCss();

        // Assert — two shadow layers separated by ", "
        css.Should().Contain(", ", because: $"elevation {level} should produce key + ambient layers");
        css.Should().Contain("px", because: "shadow values must have pixel units");
        css.Should().Contain("color-mix", because: "opacity is expressed via color-mix");
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(25)]
    [InlineData(100)]
    public void Elevation_Clamps_Level_To_0_24(int level)
    {
        // Arrange & Act — should not throw
        Action act = () => BOBShadowPresets.Elevation(level);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Elevation_With_Custom_Color_Should_Use_That_Color()
    {
        // Arrange & Act
        ShadowStyle shadow = BOBShadowPresets.Elevation(4, "#FF0000");
        string css = shadow.ToCss();

        // Assert
        css.Should().Contain("#FF0000");
    }

    [Fact]
    public void Elevation_Without_Color_Should_Use_Palette_Shadow_Variable()
    {
        // Arrange & Act
        ShadowStyle shadow = BOBShadowPresets.Elevation(4);
        string css = shadow.ToCss();

        // Assert — default color is PaletteColor.Shadow → var(--palette-shadow)
        css.Should().Contain("--palette-shadow");
    }

    [Fact]
    public void ShadowStyle_ToCss_Should_Produce_Valid_BoxShadow_Syntax()
    {
        // Arrange
        ShadowStyle shadow = ShadowStyle.Create(y: 2, blur: 4, opacity: 0.2f);

        // Act
        string css = shadow.ToCss();

        // Assert — format: [x]px [y]px [blur]px [spread]px color-mix(...)
        css.Should().MatchRegex(@"\d+px \d+px \d+px \d+px color-mix\(");
    }
}
