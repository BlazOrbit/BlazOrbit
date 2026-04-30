using BlazOrbit.Components;

namespace BlazOrbit.Themes;

public sealed class BOBPalette
{
    public CssColor Background { get; }
    public CssColor BackgroundContrast { get; }
    public CssColor Error { get; }
    public CssColor ErrorContrast { get; }
    public CssColor Info { get; }
    public CssColor InfoContrast { get; }
    public CssColor Primary { get; }
    public CssColor PrimaryContrast { get; }
    public CssColor Secondary { get; }
    public CssColor SecondaryContrast { get; }
    public CssColor Shadow { get; }
    public CssColor Success { get; }
    public CssColor SuccessContrast { get; }
    public CssColor Surface { get; }
    public CssColor SurfaceContrast { get; }
    public CssColor Warning { get; }
    public CssColor WarningContrast { get; }

    public BOBPalette(IReadOnlyDictionary<string, string> palette)
    {
        CssColor C(string key)
        {
            return new(palette[key]);
        }

        Background = C("--palette-background");
        BackgroundContrast = C("--palette-background-contrast");
        Error = C("--palette-error");
        ErrorContrast = C("--palette-error-contrast");
        Info = C("--palette-info");
        InfoContrast = C("--palette-info-contrast");
        Primary = C("--palette-primary");
        PrimaryContrast = C("--palette-primary-contrast");
        Secondary = C("--palette-secondary");
        SecondaryContrast = C("--palette-secondary-contrast");
        Shadow = C("--palette-shadow");
        Success = C("--palette-success");
        SuccessContrast = C("--palette-success-contrast");
        Surface = C("--palette-surface");
        SurfaceContrast = C("--palette-surface-contrast");
        Warning = C("--palette-warning");
        WarningContrast = C("--palette-warning-contrast");
    }
}