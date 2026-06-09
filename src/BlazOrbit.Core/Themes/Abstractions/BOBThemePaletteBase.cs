using BlazOrbit.Components;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace BlazOrbit.Themes;

/// <summary>
/// Base palette for a BlazOrbit theme. Each <see cref="CssColor"/> property maps to a
/// CSS custom property emitted as <c>--{Id}-{kebab-name}</c> (decision D-10).
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public abstract class BOBThemePaletteBase
{
    /// <summary>Page background color.</summary>
    public CssColor Background { get; set; } = new("#0F172A");

    /// <summary>Foreground color for content on <see cref="Background"/>.</summary>
    public CssColor BackgroundContrast { get; set; } = new("#F1F5F9");

    /// <summary>Default border color.</summary>
    public CssColor Border { get; set; } = BOBColor.White.Default;

    /// <summary>Error / destructive accent color.</summary>
    public CssColor Error { get; set; } = new("#EF4444");

    /// <summary>Foreground color for content on <see cref="Error"/>.</summary>
    public CssColor ErrorContrast { get; set; } = new("#0F172A");

    /// <summary>Highlight accent (selections, marks).</summary>
    public CssColor Highlight { get; set; } = BOBColor.Yellow.Default;

    /// <summary>Stable palette identifier used as the CSS variable prefix.</summary>
    public string Id { get; set; } = "default";

    /// <summary>Informational accent color.</summary>
    public CssColor Info { get; set; } = new("#3B82F6");

    /// <summary>Foreground color for content on <see cref="Info"/>.</summary>
    public CssColor InfoContrast { get; set; } = new("#0F172A");

    /// <summary>Human-readable palette name.</summary>
    public string Name { get; set; } = "Default";

    /// <summary>Primary brand color.</summary>
    public CssColor Primary { get; set; } = new("#60A5FA");

    /// <summary>Foreground color for content on <see cref="Primary"/>.</summary>
    public CssColor PrimaryContrast { get; set; } = new("#0F172A");

    /// <summary>Secondary brand color.</summary>
    public CssColor Secondary { get; set; } = new("#A78BFA");

    /// <summary>Foreground color for content on <see cref="Secondary"/>.</summary>
    public CssColor SecondaryContrast { get; set; } = new("#0F172A");

    /// <summary>Default shadow color.</summary>
    public CssColor Shadow { get; set; } = BOBColor.White.Default;

    /// <summary>Success accent color.</summary>
    public CssColor Success { get; set; } = new("#10B981");

    /// <summary>Foreground color for content on <see cref="Success"/>.</summary>
    public CssColor SuccessContrast { get; set; } = new("#0F172A");

    /// <summary>Surface (card / panel) background color.</summary>
    public CssColor Surface { get; set; } = new("#1E293B");

    /// <summary>Foreground color for content on <see cref="Surface"/>.</summary>
    public CssColor SurfaceContrast { get; set; } = new("#F1F5F9");

    /// <summary>Warning accent color.</summary>
    public CssColor Warning { get; set; } = new("#F59E0B");

    /// <summary>Foreground color for content on <see cref="Warning"/>.</summary>
    public CssColor WarningContrast { get; set; } = new("#0F172A");

    /// <summary>Tint overlaid on interactive elements while hovered.</summary>
    public CssColor HoverTint { get; set; } = new("#e9e9e9");

    /// <summary>Tint overlaid on interactive elements while active/pressed.</summary>
    public CssColor ActiveTint { get; set; } = new("#e9e9e9");

    /// <summary>
    /// Gets theme-specific CSS variables (e.g., --dark-background, --light-background)
    /// </summary>
    public Dictionary<string, string> GetThemeVariables()
    {
        Dictionary<string, string> variables = [];
        PropertyInfo[] properties = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

        foreach (PropertyInfo property in properties)
        {
            string cssName = ToCssVariableName(property.Name);
            CssColor color = (CssColor)property.GetValue(this)!;
            variables[$"--{Id}-{cssName}"] = color.ToString(ColorOutputFormats.Optimized);
        }

        return variables;
    }

    // Decision D-10: palette CSS variables use kebab-case so compound names like
    // PrimaryContrast become `--palette-primary-contrast` instead of the older
    // single-token `--palette-primarycontrast`. Insert a dash before each uppercase
    // letter (except the first), then lowercase the whole string. Single-word
    // names such as `Primary` or `Background` are unaffected.
    internal static string ToCssVariableName(string propertyName)
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
