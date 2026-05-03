using BlazOrbit.Components;
using BlazOrbit.Core.Utilities;
using BlazOrbit.Themes;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace BlazOrbit.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class CssInitializeThemesGenerator : IAssetGenerator
{
    public string FileName => "_initialize-themes.css";
    public string Name => "Initialize Themes CSS";

    public Task<string> GetContent()
    {
        StringBuilder sb = new();
        sb.AppendLine("body {");
        sb.AppendLine($"  font-family: var({FeatureDefinitions.Typography.FontFamily});");
        sb.AppendLine($"  font-size: var({FeatureDefinitions.Typography.FontSizeBase});");
        sb.AppendLine($"  line-height: var({FeatureDefinitions.Typography.LineHeight});");
        sb.AppendLine("  background-color: var(--palette-background);");
        sb.AppendLine("  color: var(--palette-background-contrast);");
        sb.AppendLine("}");
        sb.AppendLine();

        // Emit .bob-color-<key> and .bob-bg-<key> for every palette CssColor property.
        // Source of truth: the same reflection LightTheme/DarkTheme use for GetThemeVariables().
        // Must use kebab-case (BackgroundContrast -> background-contrast) to match
        // BOBThemePaletteBase.ToCssVariableName() and the --palette-* declarations.
        string[] keys = typeof(BOBThemePaletteBase)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            // ToKebabCase: Mirrors BOBThemePaletteBase.ToCssVariableName() so the generated class names
            // and var() references stay in sync with the --palette-* declarations.
            .Select(p => p.Name.ToKebabCase())
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToArray();

        foreach (string key in keys)
        {
            sb.AppendLine($".bob-color-{key} {{");
            sb.AppendLine($"  color: var(--palette-{key});");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine($".bob-bg-{key} {{");
            sb.AppendLine($"  background-color: var(--palette-{key});");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return Task.FromResult(sb.ToString().TrimEnd());
    }
}
