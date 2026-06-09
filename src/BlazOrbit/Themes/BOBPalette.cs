using BlazOrbit.Components;

namespace BlazOrbit.Themes;

/// <summary>Resolved snapshot of the active palette CSS custom properties as <see cref="CssColor"/> instances.</summary>
public sealed class BOBPalette
{
    /// <summary>Resolved <c>--palette-background</c>.</summary>
    public CssColor Background { get; }

    /// <summary>Resolved <c>--palette-background-contrast</c>.</summary>
    public CssColor BackgroundContrast { get; }

    /// <summary>Resolved <c>--palette-error</c>.</summary>
    public CssColor Error { get; }

    /// <summary>Resolved <c>--palette-error-contrast</c>.</summary>
    public CssColor ErrorContrast { get; }

    /// <summary>Resolved <c>--palette-info</c>.</summary>
    public CssColor Info { get; }

    /// <summary>Resolved <c>--palette-info-contrast</c>.</summary>
    public CssColor InfoContrast { get; }

    /// <summary>Resolved <c>--palette-primary</c>.</summary>
    public CssColor Primary { get; }

    /// <summary>Resolved <c>--palette-primary-contrast</c>.</summary>
    public CssColor PrimaryContrast { get; }

    /// <summary>Resolved <c>--palette-secondary</c>.</summary>
    public CssColor Secondary { get; }

    /// <summary>Resolved <c>--palette-secondary-contrast</c>.</summary>
    public CssColor SecondaryContrast { get; }

    /// <summary>Resolved <c>--palette-shadow</c>.</summary>
    public CssColor Shadow { get; }

    /// <summary>Resolved <c>--palette-success</c>.</summary>
    public CssColor Success { get; }

    /// <summary>Resolved <c>--palette-success-contrast</c>.</summary>
    public CssColor SuccessContrast { get; }

    /// <summary>Resolved <c>--palette-surface</c>.</summary>
    public CssColor Surface { get; }

    /// <summary>Resolved <c>--palette-surface-contrast</c>.</summary>
    public CssColor SurfaceContrast { get; }

    /// <summary>Resolved <c>--palette-warning</c>.</summary>
    public CssColor Warning { get; }

    /// <summary>Resolved <c>--palette-warning-contrast</c>.</summary>
    public CssColor WarningContrast { get; }

    /// <summary>Constructs the palette by parsing each <c>--palette-*</c> entry from <paramref name="palette"/>.</summary>
    public BOBPalette(IReadOnlyDictionary<string, string> palette)
    {
        CssColor C(string key)
        {
            return new CssColor(palette[key]);
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
