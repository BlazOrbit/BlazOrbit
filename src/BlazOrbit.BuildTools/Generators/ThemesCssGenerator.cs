using BlazOrbit.Components;
using BlazOrbit.Themes;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlazOrbit.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ThemesCssGenerator : IAssetGenerator
{
    public string FileName => "_themes.css";
    public string Name => "Themes CSS";

    public Task<string> GetContent() => Task.FromResult(CssThemeGenerator.Generate("dark",
            [new Themes.DarkTheme(), new Themes.LightTheme()]));
}

public static class CssThemeGenerator
{
    // Canonical CSS custom-property prefix exposed in `_themes.css` and
    // consumed by the rest of the bundle (`_base.css`, family files, scoped
    // .razor.css, ThemeInterop's PALETTE_VARS list). Per-theme palettes are
    // emitted by `BOBThemePaletteBase.GetThemeVariables()` keyed by `--{Id}-…`
    // and rewritten here to the theme-agnostic surface.
    private const string PalettePrefix = "--palette-";

    public static string Generate(
        string defaultTheme,
        IReadOnlyCollection<BOBThemePaletteBase> palettes)
    {
        // Build-time parity check: every theme must declare the same set of
        // palette variables (after stripping the per-theme prefix). A missing
        // variable in one theme would inherit from `:root` (the default theme)
        // at runtime — so the page renders mostly Light but with a few Dark
        // pixels (or vice versa), and nothing flags the inconsistency until QA.
        // Failing the build here surfaces it the moment a theme is touched.
        AssertPaletteParity(palettes);

        StringBuilder sb = new();

        // 1. Definir el tema por defecto en :root con valores finales.
        sb.AppendLine(":root {");
        sb.AppendLine("  /* === Base Palette (Default Theme) === */");

        BOBThemePaletteBase defaultPalette = palettes.First(p => p.Id == defaultTheme);
        AppendPaletteVariables(sb, defaultPalette);
        sb.AppendLine("}");

        // 2. Sobreescrituras para los demás temas, scoped al data-attribute.
        foreach (BOBThemePaletteBase palette in palettes)
        {
            if (palette.Id == defaultTheme)
            {
                continue;
            }

            sb.AppendLine($"\nhtml[{FeatureDefinitions.DataAttributes.Theme}=\"{palette.Id}\"] {{");
            AppendPaletteVariables(sb, palette);
            sb.AppendLine("}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Asegura que todas las paletas declaran exactamente el mismo set de
    /// variables (tras stripear el prefijo theme-específico). Si difieren,
    /// lanza con un mensaje accionable enumerando qué le falta a cada tema.
    /// </summary>
    private static void AssertPaletteParity(IReadOnlyCollection<BOBThemePaletteBase> palettes)
    {
        if (palettes.Count <= 1)
        {
            return;
        }

        Dictionary<string, HashSet<string>> normalizedByTheme = new(StringComparer.Ordinal);
        foreach (BOBThemePaletteBase p in palettes)
        {
            string themePrefix = $"--{p.Id}-";
            HashSet<string> normalized = new(StringComparer.Ordinal);
            foreach (string key in p.GetThemeVariables().Keys)
            {
                normalized.Add(key.StartsWith(themePrefix, StringComparison.Ordinal)
                    ? key[themePrefix.Length..]
                    : key);
            }

            normalizedByTheme[p.Id] = normalized;
        }

        // Reference set = union of every theme's keys; any theme missing a
        // member of the union is the offender.
        HashSet<string> reference = new(StringComparer.Ordinal);
        foreach (HashSet<string> set in normalizedByTheme.Values)
        {
            reference.UnionWith(set);
        }

        List<string> mismatches = [];
        foreach (KeyValuePair<string, HashSet<string>> kv in normalizedByTheme)
        {
            HashSet<string> missing = new(reference, StringComparer.Ordinal);
            missing.ExceptWith(kv.Value);
            if (missing.Count > 0)
            {
                mismatches.Add($"theme '{kv.Key}' missing {{{string.Join(", ", missing.OrderBy(x => x, StringComparer.Ordinal))}}}");
            }
        }

        if (mismatches.Count > 0)
        {
            throw new InvalidOperationException(
                "Theme palettes have divergent variable sets: " +
                string.Join("; ", mismatches) +
                ". Every theme must declare the same set of CssColor properties so a runtime theme switch never inherits from :root by accident.");
        }
    }

    /// <summary>
    /// Emite las variables de la paleta como <c>--palette-&lt;name&gt;: &lt;value&gt;;</c>.
    /// Despoja el prefijo theme-específico (<c>--{palette.Id}-</c>) por <see cref="string.StartsWith(string, StringComparison)"/>
    /// — en lugar del antiguo <c>.Replace("--dark-", ...).Replace("--light-", ...)</c> que era
    /// frágil ante temas nuevos (`HighContrast`, `Sepia`, etc.) y propenso a colisiones si
    /// el prefijo aparecía dentro del nombre de la variable.
    /// </summary>
    private static void AppendPaletteVariables(StringBuilder sb, BOBThemePaletteBase palette)
    {
        string themePrefix = $"--{palette.Id}-";
        foreach (KeyValuePair<string, string> variable in palette.GetThemeVariables())
        {
            string key = variable.Key.StartsWith(themePrefix, StringComparison.Ordinal)
                ? PalettePrefix + variable.Key[themePrefix.Length..]
                : variable.Key;
            sb.AppendLine($"  {key}: {variable.Value};");
        }
    }
}