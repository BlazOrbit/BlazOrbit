using BlazOrbit.Components;
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
            .Select(p => ToKebabCase(p.Name))
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

    // Mirrors BOBThemePaletteBase.ToCssVariableName() so the generated class names
    // and var() references stay in sync with the --palette-* declarations.
    private static string ToKebabCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        StringBuilder sb = new(propertyName.Length + 4);
        sb.Append(char.ToLowerInvariant(propertyName[0]));
        for (int i = 1; i < propertyName.Length; i++)
        {
            char c = propertyName[i];
            if (char.IsUpper(c))
            {
                sb.Append('-');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
