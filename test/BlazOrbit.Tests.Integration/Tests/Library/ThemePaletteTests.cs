using BlazOrbit.Components;
using BlazOrbit.Themes;
using FluentAssertions;
using System.Reflection;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Palette invariants for shipped themes.
/// Pins that every CssColor property is populated and parseable, and that the primary
/// contrast pairs meet the minimum WCAG AA contrast ratio for large text (≥ 3:1).
/// </summary>
[Trait("Core", "ThemePalettes")]
public class ThemePaletteTests
{
    public static IEnumerable<object[]> AllThemes => new[]
    {
        new object[] { new LightTheme() },
        new object[] { new DarkTheme() }
    };

    // CSS-BUNDLE-04: every shipped theme must declare exactly the same
    // set of palette variables (after stripping the per-theme prefix).
    // A divergence would silently inherit from `:root` (default theme) on
    // theme switch, producing a visually inconsistent mix.
    [Fact]
    public void Light_And_Dark_Themes_Should_Declare_Identical_Variable_Sets()
    {
        BOBThemePaletteBase light = new LightTheme();
        BOBThemePaletteBase dark = new DarkTheme();

        HashSet<string> lightKeys = StripPrefix(light);
        HashSet<string> darkKeys = StripPrefix(dark);

        HashSet<string> missingFromLight = new(darkKeys, StringComparer.Ordinal);
        missingFromLight.ExceptWith(lightKeys);

        HashSet<string> missingFromDark = new(lightKeys, StringComparer.Ordinal);
        missingFromDark.ExceptWith(darkKeys);

        missingFromLight.Should().BeEmpty(
            $"theme 'light' must declare every variable that 'dark' has; missing {{{string.Join(", ", missingFromLight)}}}");
        missingFromDark.Should().BeEmpty(
            $"theme 'dark' must declare every variable that 'light' has; missing {{{string.Join(", ", missingFromDark)}}}");

        static HashSet<string> StripPrefix(BOBThemePaletteBase palette)
        {
            string prefix = $"--{palette.Id}-";
            HashSet<string> result = new(StringComparer.Ordinal);
            foreach (string key in palette.GetThemeVariables().Keys)
            {
                result.Add(key.StartsWith(prefix, StringComparison.Ordinal)
                    ? key[prefix.Length..]
                    : key);
            }

            return result;
        }
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void All_CssColor_Properties_Should_Be_Non_Null(BOBThemePaletteBase theme)
    {
        PropertyInfo[] colorProps = theme.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

        colorProps.Should().NotBeEmpty();

        foreach (PropertyInfo prop in colorProps)
        {
            CssColor? color = (CssColor?)prop.GetValue(theme);
            color.Should().NotBeNull($"{prop.Name} must be populated");
        }
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void All_CssColor_Properties_Should_Emit_Valid_Rgba(BOBThemePaletteBase theme)
    {
        PropertyInfo[] colorProps = theme.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

        foreach (PropertyInfo prop in colorProps)
        {
            CssColor color = (CssColor)prop.GetValue(theme)!;
            string rendered = color.ToString(ColorOutputFormats.Rgba);
            rendered.Should().StartWith("rgba(");
            rendered.Should().EndWith(")");
        }
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Id_And_Name_Should_Be_Populated(BOBThemePaletteBase theme)
    {
        theme.Id.Should().NotBeNullOrWhiteSpace();
        theme.Name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void LightTheme_Should_Declare_Light_Id()
    {
        LightTheme theme = new();
        theme.Id.Should().Be("light");
        theme.Name.Should().Be("Light");
    }

    [Fact]
    public void DarkTheme_Should_Declare_Dark_Id()
    {
        DarkTheme theme = new();
        theme.Id.Should().Be("dark");
        theme.Name.Should().Be("Dark");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void GetThemeVariables_Should_Emit_Resolved_Color_Values(BOBThemePaletteBase theme)
    {
        Dictionary<string, string> vars = theme.GetThemeVariables();

        vars.Should().NotBeEmpty();
        vars.Keys.Should().OnlyContain(k => k.StartsWith($"--{theme.Id}-"));
        vars.Values.Should().OnlyContain(v => !string.IsNullOrWhiteSpace(v));
    }

    // THEME-06 / D-10: PascalCase property names map to kebab-case CSS variables.
    [Theory]
    [MemberData(nameof(AllThemes))]
    public void GetThemeVariables_Should_Emit_KebabCase_For_Compound_Names(BOBThemePaletteBase theme)
    {
        Dictionary<string, string> vars = theme.GetThemeVariables();
        string id = theme.Id;

        vars.Should().ContainKey($"--{id}-primary-contrast");
        vars.Should().ContainKey($"--{id}-background-contrast");
        vars.Should().ContainKey($"--{id}-surface-contrast");
        vars.Should().ContainKey($"--{id}-hover-tint");
        vars.Should().ContainKey($"--{id}-active-tint");

        // Old single-token names must not appear anywhere.
        vars.Keys.Should().NotContain(k => k.EndsWith("contrast") && !k.EndsWith("-contrast"));
        vars.Keys.Should().NotContain(k => k.EndsWith("tint") && !k.EndsWith("-tint"));

        // Single-word names stay as a single token.
        vars.Should().ContainKey($"--{id}-primary");
        vars.Should().ContainKey($"--{id}-background");
    }

    // ─────────── Contrast pairs (WCAG) ───────────

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Background_Pair_Should_Meet_WCAG_Large_Text_Contrast(BOBThemePaletteBase theme)
    {
        double ratio = Contrast(theme.Background, theme.BackgroundContrast);

        ratio.Should().BeGreaterThanOrEqualTo(3.0, "Background ↔ BackgroundContrast must be legible");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Surface_Pair_Should_Meet_WCAG_Large_Text_Contrast(BOBThemePaletteBase theme)
    {
        double ratio = Contrast(theme.Surface, theme.SurfaceContrast);

        ratio.Should().BeGreaterThanOrEqualTo(3.0, "Surface ↔ SurfaceContrast must be legible");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Primary_Pair_Should_Meet_WCAG_Large_Text_Contrast(BOBThemePaletteBase theme)
    {
        double ratio = Contrast(theme.Primary, theme.PrimaryContrast);

        ratio.Should().BeGreaterThanOrEqualTo(3.0, "Primary ↔ PrimaryContrast must be legible");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Semantic_Pairs_Should_Meet_WCAG_Large_Text_Contrast(BOBThemePaletteBase theme)
    {
        Contrast(theme.Error, theme.ErrorContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Success, theme.SuccessContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Warning, theme.WarningContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Info, theme.InfoContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Secondary, theme.SecondaryContrast).Should().BeGreaterThanOrEqualTo(3.0);
    }

    private static double Contrast(CssColor a, CssColor b)
    {
        double la = a.GetRelativeLuminance();
        double lb = b.GetRelativeLuminance();
        double light = Math.Max(la, lb);
        double dark = Math.Min(la, lb);
        return (light + 0.05) / (dark + 0.05);
    }
}
