namespace BlazOrbit.Components;

/// <summary>
/// Strongly-typed accessor for a palette CSS custom property. Implicitly converts to a
/// <c>var(--palette-*)</c> reference for direct use in CSS values.
/// </summary>
public sealed class PaletteColor
{
    private readonly string _variable;

    private PaletteColor(string variable) => _variable = variable;

    /// <summary><c>var(--palette-background)</c>.</summary>
    public static PaletteColor Background => new("--palette-background");

    /// <summary><c>var(--palette-background-contrast)</c>.</summary>
    public static PaletteColor BackgroundContrast => new("--palette-background-contrast");

    /// <summary><c>var(--palette-error)</c>.</summary>
    public static PaletteColor Error => new("--palette-error");

    /// <summary><c>var(--palette-error-contrast)</c>.</summary>
    public static PaletteColor ErrorContrast => new("--palette-error-contrast");

    /// <summary><c>var(--palette-info)</c>.</summary>
    public static PaletteColor Info => new("--palette-info");

    /// <summary><c>var(--palette-info-contrast)</c>.</summary>
    public static PaletteColor InfoContrast => new("--palette-info-contrast");

    /// <summary><c>var(--palette-primary)</c>.</summary>
    public static PaletteColor Primary => new("--palette-primary");

    /// <summary><c>var(--palette-primary-contrast)</c>.</summary>
    public static PaletteColor PrimaryContrast => new("--palette-primary-contrast");

    /// <summary><c>var(--palette-secondary)</c>.</summary>
    public static PaletteColor Secondary => new("--palette-secondary");

    /// <summary><c>var(--palette-secondary-contrast)</c>.</summary>
    public static PaletteColor SecondaryContrast => new("--palette-secondary-contrast");

    /// <summary><c>var(--palette-success)</c>.</summary>
    public static PaletteColor Success => new("--palette-success");

    /// <summary><c>var(--palette-success-contrast)</c>.</summary>
    public static PaletteColor SuccessContrast => new("--palette-success-contrast");

    /// <summary><c>var(--palette-surface)</c>.</summary>
    public static PaletteColor Surface => new("--palette-surface");

    /// <summary><c>var(--palette-surface-contrast)</c>.</summary>
    public static PaletteColor SurfaceContrast => new("--palette-surface-contrast");

    /// <summary><c>var(--palette-warning)</c>.</summary>
    public static PaletteColor Warning => new("--palette-warning");

    /// <summary><c>var(--palette-warning-contrast)</c>.</summary>
    public static PaletteColor WarningContrast => new("--palette-warning-contrast");

    /// <summary><c>var(--palette-border)</c>.</summary>
    public static PaletteColor Border => new("--palette-border");

    /// <summary><c>var(--palette-highlight)</c>.</summary>
    public static PaletteColor Highlight => new("--palette-highlight");

    /// <summary><c>var(--palette-shadow)</c>.</summary>
    public static PaletteColor Shadow => new("--palette-shadow");

    /// <summary>Implicit conversion to a CSS <c>var(...)</c> reference.</summary>
    public static implicit operator string(PaletteColor p)
    {
        return $"var({p._variable})";
    }

    /// <inheritdoc />
    public override string ToString() => $"var({_variable})";
}
