using BlazOrbit.Components;
using BlazOrbit.Themes;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// THEME-03: WCAG 2.1 AA contrast check for every palette/contrast pair exposed
/// by the built-in <see cref="LightTheme"/> and <see cref="DarkTheme"/>. Running
/// the full matrix so regressions on <c>THEME-01</c>/<c>THEME-02</c> show up as
/// test failures rather than manual inspection.
/// </summary>
[Trait("Library", "ThemeContrast")]
public class ThemeContrastTests
{
    // Normal text AA = 4.5:1. Large text / UI graphics AA = 3:1.
    private const double AaNormal = 4.5;
    private const double AaLargeOrUi = 3.0;

    public static IEnumerable<object[]> TextPairs()
    {
        foreach ((string themeName, BOBThemePaletteBase theme) in Themes())
        {
            foreach ((string pair, CssColor a, CssColor b) in ExtractTextPairs(theme))
            {
                yield return [themeName, pair, a, b];
            }
        }
    }

    public static IEnumerable<object[]> UiPairs()
    {
        foreach ((string themeName, BOBThemePaletteBase theme) in Themes())
        {
            yield return [themeName, "Border/Background", theme.Border, theme.Background];
            yield return [themeName, "Highlight/Background", theme.Highlight, theme.Background];
        }
    }

    [Theory]
    [MemberData(nameof(TextPairs))]
    public void Text_Pair_Should_Meet_WCAG_AA_Normal(
        string theme, string pair, CssColor foreground, CssColor background)
    {
        double ratio = ContrastRatio(foreground, background);
        ratio.Should().BeGreaterThanOrEqualTo(
            AaNormal,
            because: $"{theme} {pair} should pass WCAG 2.1 AA for normal text (ratio 4.5:1); actual {ratio:F2}:1");
    }

    [Theory]
    [MemberData(nameof(UiPairs))]
    public void Ui_Pair_Should_Meet_WCAG_AA_UiGraphics(
        string theme, string pair, CssColor foreground, CssColor background)
    {
        double ratio = ContrastRatio(foreground, background);
        ratio.Should().BeGreaterThanOrEqualTo(
            AaLargeOrUi,
            because: $"{theme} {pair} should pass WCAG 2.1 AA for UI graphics (ratio 3:1); actual {ratio:F2}:1");
    }

    private static double ContrastRatio(CssColor a, CssColor b)
    {
        double la = a.GetRelativeLuminance();
        double lb = b.GetRelativeLuminance();
        double lighter = Math.Max(la, lb);
        double darker = Math.Min(la, lb);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static IEnumerable<(string Name, BOBThemePaletteBase Theme)> Themes()
    {
        yield return ("Light", new LightTheme());
        yield return ("Dark", new DarkTheme());
    }

    private static IEnumerable<(string Pair, CssColor Foreground, CssColor Background)> ExtractTextPairs(BOBThemePaletteBase t)
    {
        yield return ("Background/BackgroundContrast", t.BackgroundContrast, t.Background);
        yield return ("Surface/SurfaceContrast", t.SurfaceContrast, t.Surface);
        yield return ("Primary/PrimaryContrast", t.PrimaryContrast, t.Primary);
        yield return ("Secondary/SecondaryContrast", t.SecondaryContrast, t.Secondary);
        yield return ("Success/SuccessContrast", t.SuccessContrast, t.Success);
        yield return ("Warning/WarningContrast", t.WarningContrast, t.Warning);
        yield return ("Error/ErrorContrast", t.ErrorContrast, t.Error);
        yield return ("Info/InfoContrast", t.InfoContrast, t.Info);
    }
}
