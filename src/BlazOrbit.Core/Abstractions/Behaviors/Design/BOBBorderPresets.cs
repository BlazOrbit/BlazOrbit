namespace BlazOrbit.Components;

/// <summary>
/// Provides predefined <see cref="BorderStyle" /> instances for common border appearances.
/// </summary>
public static class BOBBorderPresets
{
    // ===================================== Básicos (sin radius) =====================================

    /// <summary>Dashed gray border with 1px width.</summary>
    public static BorderStyle Dashed
        => BorderStyle.Create()
            .All("1px", BorderStyleType.Dashed, BOBColor.Gray.Default);

    /// <summary>Default subtle solid border with 1px width.</summary>
    public static BorderStyle Default
            => BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BOBColor.Gray.Lighten2);

    // ===================================== Estilos especiales =====================================

    /// <summary>Dotted gray border with 1px width.</summary>
    public static BorderStyle Dotted
        => BorderStyle.Create()
            .All("1px", BorderStyleType.Dotted, BOBColor.Gray.Default);

    /// <summary>Double gray border with 3px width.</summary>
    public static BorderStyle Double
        => BorderStyle.Create()
            .All("3px", BorderStyleType.Double, BOBColor.Gray.Default);

    /// <summary>Solid error-colored border with 2px width.</summary>
    public static BorderStyle Error
        => BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Error);

    /// <summary>Solid info-colored border with 2px width.</summary>
    public static BorderStyle Info
        => BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Info);

    /// <summary>No border.</summary>
    public static BorderStyle None
        => BorderStyle.Create().None();

    /// <summary>Pill-shaped border with full radius and subtle gray color.</summary>
    public static BorderStyle Pill
        => BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BOBColor.Gray.Lighten2)
            .Radius(9999);

    /// <summary>Solid primary-colored border with 2px width.</summary>
    public static BorderStyle Primary
        => BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Primary);

    /// <summary>Rounded border with 4px radius and subtle gray color.</summary>
    public static BorderStyle Rounded
        => BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BOBColor.Gray.Lighten2)
            .Radius(4);

    // ===================================== Radius predefinido =====================================

    /// <summary>Rounded border with 8px radius and subtle gray color.</summary>
    public static BorderStyle RoundedLarge
        => BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BOBColor.Gray.Lighten2)
            .Radius(8);

    // ===================================== Temáticos =====================================

    /// <summary>Solid secondary-colored border with 2px width.</summary>
    public static BorderStyle Secondary
        => BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Secondary);

    /// <summary>Strong gray solid border with 2px width.</summary>
    public static BorderStyle Strong
        => BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BOBColor.Gray.Default);

    /// <summary>Subtle light gray solid border with 1px width.</summary>
    public static BorderStyle Subtle
        => BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BOBColor.Gray.Lighten3);

    /// <summary>Solid success-colored border with 2px width.</summary>
    public static BorderStyle Success
        => BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Success);

    /// <summary>Solid warning-colored border with 2px width.</summary>
    public static BorderStyle Warning
            => BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Warning);

    // ===================================== Utilitarios =====================================
}
