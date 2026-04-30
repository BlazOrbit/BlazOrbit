namespace BlazOrbit.Components;

public sealed class PaletteColor
{
    private readonly string _variable;

    private PaletteColor(string variable) => _variable = variable;

    public static PaletteColor Background => new("--palette-background");
    public static PaletteColor BackgroundContrast => new("--palette-background-contrast");
    public static PaletteColor Error => new("--palette-error");
    public static PaletteColor ErrorContrast => new("--palette-error-contrast");
    public static PaletteColor Info => new("--palette-info");
    public static PaletteColor InfoContrast => new("--palette-info-contrast");
    public static PaletteColor Primary => new("--palette-primary");
    public static PaletteColor PrimaryContrast => new("--palette-primary-contrast");
    public static PaletteColor Secondary => new("--palette-secondary");
    public static PaletteColor SecondaryContrast => new("--palette-secondary-contrast");
    public static PaletteColor Success => new("--palette-success");
    public static PaletteColor SuccessContrast => new("--palette-success-contrast");
    public static PaletteColor Surface => new("--palette-surface");
    public static PaletteColor SurfaceContrast => new("--palette-surface-contrast");
    public static PaletteColor Warning => new("--palette-warning");
    public static PaletteColor WarningContrast => new("--palette-warning-contrast");
    public static PaletteColor Border => new("--palette-border");
    public static PaletteColor Highlight => new("--palette-highlight");
    public static PaletteColor Shadow => new("--palette-shadow");

    public static implicit operator string(PaletteColor p)
    {
        return $"var({p._variable})";
    }

    public override string ToString() => $"var({_variable})";
}
