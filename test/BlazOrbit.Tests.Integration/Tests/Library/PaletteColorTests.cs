using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// LIB-04: Each PaletteColor static property resolves to var(--palette-*).
/// Verifies the implicit string conversion and ToString() emit valid CSS variable references.
/// </summary>
[Trait("Library", "PaletteColor")]
public class PaletteColorTests
{
    [Theory]
    [InlineData("Background", "--palette-background")]
    [InlineData("BackgroundContrast", "--palette-background-contrast")]
    [InlineData("Error", "--palette-error")]
    [InlineData("ErrorContrast", "--palette-error-contrast")]
    [InlineData("Info", "--palette-info")]
    [InlineData("InfoContrast", "--palette-info-contrast")]
    [InlineData("Primary", "--palette-primary")]
    [InlineData("PrimaryContrast", "--palette-primary-contrast")]
    [InlineData("Secondary", "--palette-secondary")]
    [InlineData("SecondaryContrast", "--palette-secondary-contrast")]
    [InlineData("Success", "--palette-success")]
    [InlineData("SuccessContrast", "--palette-success-contrast")]
    [InlineData("Surface", "--palette-surface")]
    [InlineData("SurfaceContrast", "--palette-surface-contrast")]
    [InlineData("Warning", "--palette-warning")]
    [InlineData("WarningContrast", "--palette-warning-contrast")]
    [InlineData("Border", "--palette-border")]
    [InlineData("Highlight", "--palette-highlight")]
    [InlineData("Shadow", "--palette-shadow")]
    public void PaletteColor_ImplicitString_Should_Produce_Var_Reference(
        string propertyName, string expectedVariable)
    {
        // Arrange
        PaletteColor color = (PaletteColor)typeof(PaletteColor)
            .GetProperty(propertyName)!
            .GetValue(null)!;

        // Act
        string css = color;

        // Assert
        css.Should().Be($"var({expectedVariable})");
    }

    [Fact]
    public void PaletteColor_ToString_Should_Match_ImplicitConversion()
    {
        // Arrange
        PaletteColor color = PaletteColor.Primary;

        // Assert
        color.ToString().Should().Be((string)color);
    }

    [Fact]
    public void PaletteColor_All_Properties_Should_Start_With_VarPalette()
    {
        // Arrange — collect all static PaletteColor properties
        IEnumerable<string> values = typeof(PaletteColor)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(PaletteColor))
            .Select(p => (string)(PaletteColor)p.GetValue(null)!);

        // Assert
        foreach (string css in values)
        {
            css.Should().StartWith("var(--palette-",
                because: $"PaletteColor '{css}' must reference a --palette-* CSS variable");
        }
    }
}
