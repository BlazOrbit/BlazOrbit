using BlazOrbit.Components;
using System.Reflection;
using System.Text;

namespace BlazOrbit.Themes;

public abstract class BOBThemePaletteBase
{
    public CssColor Background { get; set; } = new("#0F172A");

    public CssColor BackgroundContrast { get; set; } = new("#F1F5F9");

    public CssColor Border { get; set; } = BOBColor.White.Default;

    public CssColor Error { get; set; } = new("#EF4444");

    public CssColor ErrorContrast { get; set; } = new("#0F172A");
    public CssColor Highlight { get; set; } = BOBColor.Yellow.Default;
    public string Id { get; set; } = "default";
    public CssColor Info { get; set; } = new("#3B82F6");
    public CssColor InfoContrast { get; set; } = new("#0F172A");
    public string Name { get; set; } = "Default";

    public CssColor Primary { get; set; } = new("#60A5FA");

    public CssColor PrimaryContrast { get; set; } = new("#0F172A");
    public CssColor Secondary { get; set; } = new("#A78BFA");
    public CssColor SecondaryContrast { get; set; } = new("#0F172A");
    public CssColor Shadow { get; set; } = BOBColor.White.Default;
    public CssColor Success { get; set; } = new("#10B981");
    public CssColor SuccessContrast { get; set; } = new("#0F172A");
    public CssColor Surface { get; set; } = new("#1E293B");
    public CssColor SurfaceContrast { get; set; } = new("#F1F5F9");
    public CssColor Warning { get; set; } = new("#F59E0B");
    public CssColor WarningContrast { get; set; } = new("#0F172A");
    public CssColor HoverTint { get; set; } = new("#e9e9e9");
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