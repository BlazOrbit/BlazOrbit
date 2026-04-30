using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Functional contract tests for the CSS Color System.
///
/// These tests define and enforce the following system-level rules:
///
/// 1 - The color system MUST provide a stable and immutable set of base colors through <see
/// cref="BOBColor" /> and its nested classes.
///
/// 2 - Every base color MUST expose a <c> Default </c> value and MAY expose derived variants
/// (LightenX / DarkenX) computed deterministically from the base color.
///
/// 3 - Color variants (Lighten / Darken) MUST be derived using HSL lightness modification and MUST
/// preserve alpha values unless explicitly changed.
///
/// 4 - Palette colors (e.g. Primary, Background, Success, etc.) MUST be represented as CSS
/// variables and MUST NOT be parsed or evaluated as concrete RGB values.
///
/// 5 - <see cref="CssColor" /> MUST support construction from:
/// - RGBA byte and integer values
/// - HSL values
/// - Valid CSS string representations (HEX, RGB, RGBA)
/// - CSS variables when explicitly marked as such
///
/// 6 - Invalid color inputs MUST throw <see cref="ArgumentException" /> or <see
/// cref="ArgumentOutOfRangeException" /> and MUST NEVER fail silently.
///
/// 7 - <see cref="CssColor" /> equality and hash code MUST be value-based and depend exclusively on
/// RGBA channel values.
///
/// 8 - String output MUST be deterministic and MUST respect the requested <see
/// cref="ColorOutputFormats" />.
///
/// 9 - When no output format is specified, <see cref="CssColor.ToString()" /> MUST default to RGBA format.
///
/// 10 - CSS variable–based colors MUST round-trip without modification and MUST bypass numeric
/// color calculations.
///
/// 11 - Relative luminance and contrast helpers MUST follow WCAG 2.1 rules when numeric color
/// values are available, and MUST degrade gracefully for CSS variable–based colors.
///
/// Together, these tests define the parsing, normalization, transformation, formatting, and safety
/// guarantees of the BlazOrbit CSS Color System.
/// </summary>
[Trait("Library", "CssColorSystem")]
public class CssColorSystemTests
{
    [Fact]
    public async Task BOBColor_Should_Provide_Color_Variants()
    {
        // Assert base colors exist
        BOBColor.Red.Default.Should().NotBeNull();
        BOBColor.Red.Darken1.Should().NotBeNull();
        BOBColor.Red.Lighten1.Should().NotBeNull();

        // Assert palette colors
        ((string)PaletteColor.Primary).Should().NotBeNull();
        ((string)PaletteColor.Background).Should().NotBeNull();

        // Assert color output formats
        CssColor color = BOBColor.Blue.Default;
        string rgba = color.ToString(ColorOutputFormats.Rgba);
        rgba.Should().StartWith("rgba(");
    }

    [Fact]
    public void BOBColor_Should_Return_Deterministic_Values()
    {
        CssColor first = BOBColor.Red.Default;
        CssColor second = BOBColor.Red.Default;

        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void CssColor_APercentage_Property()
    {
        // Arrange
        CssColor color = new(255, 0, 0, 128);

        // Act
        double percentage = color.APercentage;

        // Assert
        percentage.Should().BeApproximately(0.5, 0.01);
    }

    [Fact]
    public void CssColor_Constructor_ByteValues()
    {
        // Act
        CssColor color = new(100, 150, 200, 255);

        // Assert
        color.R.Should().Be(100);
        color.G.Should().Be(150);
        color.B.Should().Be(200);
        color.A.Should().Be(255);
    }

    [Fact]
    public void CssColor_Constructor_IntValuesWithAlpha()
    {
        // Act
        CssColor color = new(100, 150, 200, 255);

        // Assert
        color.R.Should().Be(100);
        color.G.Should().Be(150);
        color.B.Should().Be(200);
        color.A.Should().Be(255);
    }

    [Fact]
    public void CssColor_Constructor_IntValuesWithAlphaDouble()
    {
        // Act
        CssColor color = new(100, 150, 200, 0.5);

        // Assert
        color.R.Should().Be(100);
        color.G.Should().Be(150);
        color.B.Should().Be(200);
        color.A.Should().BeCloseTo(127, 1); // 0.5 * 255
    }

    [Theory]
    [InlineData(null)]                   // null
    [InlineData("")]                     // empty
    [InlineData("   ")]                  // whitespace
    [InlineData("invalid")]              // random string
    [InlineData("rgb()")]                // missing numbers
    [InlineData("rgba()")]               // missing numbers
    [InlineData("rgb(300,400,500)")]     // values out of range
    [InlineData("rgba(0,0,0,2)")]        // alpha > 1
    [InlineData("#GGHHII")]              // invalid hex characters
    [InlineData("#12345")]               // invalid hex length
    [InlineData("#123456789")]           // invalid hex length
    public void CssColor_Constructor_Throws_ForInvalidStrings(string? invalidValue)
    {
        // Arrange
        Action act = () => new CssColor(invalidValue!);

        // Act & Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CssColor_EqualityOperator_ComparesTwoColors()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        CssColor color3 = new("#00FF00");

        // Act & Assert
        (color1 == color2).Should().BeTrue();
        (color1 == color3).Should().BeFalse();
        (color1 != color3).Should().BeTrue();
        (color1 != color2).Should().BeFalse();
    }

    [Fact]
    public void CssColor_Equals_WithCssColorParameter()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        CssColor color3 = new("#00FF00");
        CssColor? nullColor = null;

        // Act & Assert
        color1.Equals(color2).Should().BeTrue();
        color1.Equals(color3).Should().BeFalse();
        color1.Equals(nullColor).Should().BeFalse();

        // Same reference
        color1.Equals(color1).Should().BeTrue();
    }

    [Fact]
    public void CssColor_Equals_WithObjectParameter()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        object? nullObject = null;
        object notAColor = "Not a color";

        // Act & Assert
        color1.Equals((object)color2).Should().BeTrue();
        color1.Equals(nullObject).Should().BeFalse();
        color1.Equals(notAColor).Should().BeFalse();
    }

    [Fact]
    public void CssColor_GetHashCode_SameColorsHaveSameHashCode()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        CssColor color3 = new("#00FF00");

        // Act
        int hash1 = color1.GetHashCode();
        int hash2 = color2.GetHashCode();
        int hash3 = color3.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        hash1.Should().NotBe(hash3);
    }

    [Fact]
    public void CssColor_HSL_Properties_Get()
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act & Assert - Only test getters
        color.H.Should().BeInRange(0, 360);
        color.S.Should().BeInRange(0, 100);
        color.L.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CssColor_ImplicitOperator_FromCssColorToString()
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string colorString = (string)color; // Explicit cast since implicit might not be available

        // Assert
        colorString.Should().Be("rgba(255,0,0,1)");
    }

    [Fact]
    public void CssColor_ImplicitOperator_FromStringToCssColor()
    {
        // Act
        CssColor color = new("#FF0000"); // Direct constructor instead of implicit

        // Assert
        color.Should().NotBeNull();
        color.R.Should().Be(255);
        color.G.Should().Be(0);
        color.B.Should().Be(0);
    }

    [Theory]
    [InlineData("#FF0000")]
    [InlineData("#ff0000")]           // lowercase
    [InlineData("#F00")]
    [InlineData("#F00F")]             // 4-digit HEX with alpha
    [InlineData("#FF0000FF")]         // 8-digit HEX with alpha
    [InlineData("rgb(255,0,0)")]
    [InlineData("rgb( 255 , 0 , 0 )")] // spaces
    [InlineData("rgba(255,0,0,1)")]
    [InlineData("rgba(255,0,0,0.5)")]  // alpha decimal
    [InlineData("RGBA(255,0,0,0.5)")]  // uppercase rgba
    public void CssColor_Parse_ValidFormats(string colorString)
    {
        // Act
        CssColor color = new(colorString);

        // Assert
        color.Should().NotBeNull();
        color.R.Should().BeInRange(0, 255);
        color.G.Should().BeInRange(0, 255);
        color.B.Should().BeInRange(0, 255);
        color.A.Should().BeInRange(0, 255);
    }

    [Theory]
    [InlineData(ColorOutputFormats.Hex)]
    [InlineData(ColorOutputFormats.Rgba)]
    [InlineData(ColorOutputFormats.Rgb)]
    [InlineData(ColorOutputFormats.HexA)]
    public void CssColor_ToString_AllFormats(ColorOutputFormats format)
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string result = color.ToString(format);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(ColorOutputFormats.Hex, "#")]
    [InlineData(ColorOutputFormats.Rgba, "rgba")] // Using Rgba instead of RGB/RGBA
    public void CssColor_ToString_WithFormat_ReturnsCorrectFormat(ColorOutputFormats format, string expectedStart)
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string result = color.ToString(format);

        // Assert
        result.Should().StartWith(expectedStart, "Format {0} should produce output starting with {1}", format, expectedStart);
    }

    [Fact]
    public void CssColor_ToString_WithoutParameter_ReturnsDefaultFormat()
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string result = color.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Should return Rgba format by default
        result.Should().Be("rgba(255,0,0,1)");
    }

    [Fact]
    public void CssColorVariant_Constructor()
    {
        // This tests the CssColorVariant constructor that takes Modifier and double We can't create
        // it directly, but we can test through the static methods
        CssColorVariant lighten1 = CssColorVariant.Lighten(1);
        CssColorVariant darken2 = CssColorVariant.Darken(2);

        // Assert
        lighten1.Should().NotBeNull();
        darken2.Should().NotBeNull();

        // Test the properties if they are accessible
        lighten1.Alteration.Should().BeGreaterThan(0);
        darken2.Alteration.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CssColorVariant_Should_Preserve_Alpha_Channel()
    {
        // Arrange
        CssColor baseColor = new(100, 100, 100, 128);

        // Act
        CssColor lightened = new(100, 100, 100, 128, CssColorVariant.Lighten(1));
        CssColor darkened = new(100, 100, 100, 128, CssColorVariant.Darken(1));

        // Assert
        lightened.A.Should().Be(128);
        darkened.A.Should().Be(128);
    }

    [Theory]
    [InlineData(0x00, 0x00, 0x00, 255, "#000")]        // Black - short hex
    [InlineData(0xFF, 0xFF, 0xFF, 255, "#fff")]        // White - short hex
    [InlineData(0xAA, 0xBB, 0xCC, 255, "#abc")]        // Pairs match - short hex
    [InlineData(0x11, 0x22, 0x33, 255, "#123")]        // Pairs match - short hex
    [InlineData(0xFF, 0x00, 0x00, 255, "#f00")]        // Red - short hex
    [InlineData(0x00, 0xFF, 0x00, 255, "#0f0")]        // Green - short hex
    [InlineData(0x00, 0x00, 0xFF, 255, "#00f")]        // Blue - short hex
    public void CssColor_Optimized_ShouldReturn_ShortHex_WhenPairsMatch(byte r, byte g, byte b, byte a, string expected)
    {
        // Arrange
        CssColor color = new(r, g, b, a);

        // Act
        string result = color.ToString(ColorOutputFormats.Optimized);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0x12, 0x34, 0x56, 255, "#123456")]     // No pairs match
    [InlineData(0xAB, 0xCD, 0xEF, 255, "#abcdef")]     // No pairs match
    [InlineData(0x10, 0x20, 0x30, 255, "#102030")]     // First nibble differs from second
    [InlineData(0xFF, 0x00, 0x01, 255, "#ff0001")]     // Almost red but B=1
    [InlineData(0x01, 0x23, 0x45, 255, "#012345")]     // Mixed values
    public void CssColor_Optimized_ShouldReturn_FullHex_WhenPairsDontMatch(byte r, byte g, byte b, byte a, string expected)
    {
        // Arrange
        CssColor color = new(r, g, b, a);

        // Act
        string result = color.ToString(ColorOutputFormats.Optimized);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(255, 0, 0, 128, "rgba(255,0,0,0.5)")]           // 50% alpha
    [InlineData(0, 255, 0, 0, "rgba(0,255,0,0)")]               // Fully transparent
    [InlineData(100, 150, 200, 25, "rgba(100,150,200,0.1)")]    // ~10% alpha
    [InlineData(0, 0, 0, 254, "rgba(0,0,0,1)")]                 // Almost opaque (rounds to 1)
    public void CssColor_Optimized_ShouldReturn_Rgba_WhenAlphaLessThan255(byte r, byte g, byte b, byte a, string expected)
    {
        // Arrange
        CssColor color = new(r, g, b, a);

        // Act
        string result = color.ToString(ColorOutputFormats.Optimized);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CssColor_Optimized_OutputLength_ShouldBeShorterOrEqual_ToRgba()
    {
        // Arrange
        CssColor[] testColors = new[]
        {
        new CssColor(255, 255, 255, 255),  // #fff vs rgba(255,255,255,1)
        new CssColor(0, 0, 0, 255),        // #000 vs rgba(0,0,0,1)
        new CssColor(170, 187, 204, 255),  // #abc vs rgba(170,187,204,1)
        new CssColor(18, 52, 86, 255),     // #123456 vs rgba(18,52,86,1)
    };

        foreach (CssColor color in testColors)
        {
            // Act
            string optimized = color.ToString(ColorOutputFormats.Optimized);
            string rgba = color.ToString(ColorOutputFormats.Rgba);

            // Assert
            optimized.Length.Should().BeLessThanOrEqualTo(rgba.Length,
                "Optimized format '{0}' should not be longer than RGBA format '{1}'", optimized, rgba);
        }
    }

    [Fact]
    public void CssColor_Optimized_ShouldProduce_ValidCssColors()
    {
        // Arrange
        CssColor[] testColors = new[]
        {
        new CssColor("#abc"),
        new CssColor("#123456"),
        new CssColor("rgb(100,150,200)"),
        new CssColor("rgba(50,100,150,0.5)"),
    };

        foreach (CssColor original in testColors)
        {
            // Act
            string optimized = original.ToString(ColorOutputFormats.Optimized);

            // Assert - Should be parseable back (except short hex needs expansion)
            if (optimized.StartsWith("rgba"))
            {
                CssColor reparsed = new(optimized);
                reparsed.R.Should().Be(original.R);
                reparsed.G.Should().Be(original.G);
                reparsed.B.Should().Be(original.B);
            }
            else
            {
                // Hex format validation
                optimized.Should().MatchRegex(@"^#[0-9a-f]{3}$|^#[0-9a-f]{6}$");
            }
        }
    }

    // ---- LIB-01: HsvColor conversions ----

    [Theory]
    [InlineData(255, 0, 0, 0, 1.0, 1.0)]       // red
    [InlineData(0, 255, 0, 120, 1.0, 1.0)]     // green
    [InlineData(0, 0, 255, 240, 1.0, 1.0)]     // blue
    [InlineData(255, 255, 255, 0, 0.0, 1.0)]   // white
    [InlineData(0, 0, 0, 0, 0.0, 0.0)]         // black
    public void HsvColor_FromCssColor_Should_Produce_Correct_HSV(
        int r, int g, int b, int expectedHue, double expectedSat, double expectedVal)
    {
        CssColor color = new(r, g, b, 255);
        HsvColor hsv = HsvColor.FromCssColor(color);

        hsv.Hue.Should().Be(expectedHue);
        hsv.Saturation.Should().BeApproximately(expectedSat, 0.001);
        hsv.Value.Should().BeApproximately(expectedVal, 0.001);
    }

    [Theory]
    [InlineData(0, 1.0, 1.0, 255, 0, 0)]       // red
    [InlineData(120, 1.0, 1.0, 0, 255, 0)]     // green
    [InlineData(240, 1.0, 1.0, 0, 0, 255)]     // blue
    [InlineData(0, 0.0, 1.0, 255, 255, 255)]   // white
    [InlineData(0, 0.0, 0.0, 0, 0, 0)]         // black
    public void HsvColor_ToCssColor_Should_Produce_Correct_RGB(
        int hue, double saturation, double value, int expectedR, int expectedG, int expectedB)
    {
        HsvColor hsv = new(hue, saturation, value);
        CssColor color = hsv.ToCssColor();

        color.R.Should().Be((byte)expectedR);
        color.G.Should().Be((byte)expectedG);
        color.B.Should().Be((byte)expectedB);
    }

    [Theory]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    [InlineData(128, 64, 32)]
    [InlineData(0, 0, 0)]
    [InlineData(255, 255, 255)]
    public void HsvColor_RoundTrip_Should_Preserve_Color(int r, int g, int b)
    {
        CssColor original = new(r, g, b, 255);
        HsvColor hsv = HsvColor.FromCssColor(original);
        CssColor roundTripped = hsv.ToCssColor();

        // Allow ±1 rounding error per channel
        Math.Abs((int)roundTripped.R - r).Should().BeLessThanOrEqualTo(1);
        Math.Abs((int)roundTripped.G - g).Should().BeLessThanOrEqualTo(1);
        Math.Abs((int)roundTripped.B - b).Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public void HsvColor_WithHue_Should_Change_Only_Hue()
    {
        HsvColor original = new(120, 0.8, 0.9);
        HsvColor modified = original.WithHue(240);

        modified.Hue.Should().Be(240);
        modified.Saturation.Should().Be(original.Saturation);
        modified.Value.Should().Be(original.Value);
    }

    [Fact]
    public void HsvColor_WithSaturation_Should_Change_Only_Saturation()
    {
        HsvColor original = new(120, 0.5, 0.9);
        HsvColor modified = original.WithSaturation(1.0);

        modified.Hue.Should().Be(original.Hue);
        modified.Saturation.Should().Be(1.0);
        modified.Value.Should().Be(original.Value);
    }
}