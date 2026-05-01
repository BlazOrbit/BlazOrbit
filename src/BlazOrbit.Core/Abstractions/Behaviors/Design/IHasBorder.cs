namespace BlazOrbit.Components;

#region Public API

/// <summary>
/// Indicates the component supports a <see cref="Border" /> parameter.
/// </summary>
public interface IHasBorder
{
    /// <summary>Border style applied to the component.</summary>
    BorderStyle? Border { get; set; }
}

/// <summary>
/// Fluent builder for constructing CSS border styles.
/// </summary>
public sealed class BorderStyle
{
    private Border? _all;
    private Border? _bottom;
    private Border? _left;
    private BorderRadius? _radius;
    private Border? _right;
    private Border? _top;

    private BorderStyle() { }

    /// <summary>Creates a new empty <see cref="BorderStyle" /> instance.</summary>
    public static BorderStyle Create() => new();

    // ---------- SIDES ----------

    /// <summary>Applies the same border to all four sides.</summary>
    public BorderStyle All(string width, BorderStyleType style, string color)
    {
        _all = new Border(width, style, color);
        _top = _right = _bottom = _left = null;
        return this;
    }

    /// <summary>
    /// CssColor-typed overload — <see cref="BOBBorderPresets"/> and consumer fluent code can pass
    /// <see cref="BOBColor.Gray.Default"/> / <see cref="PaletteColor.Primary"/> directly without an
    /// explicit cast to string.
    /// </summary>
    public BorderStyle All(string width, BorderStyleType style, CssColor color)
        => All(width, style, color.ToString());

    /// <summary>Applies a border to the top side.</summary>
    public BorderStyle Top(string width, BorderStyleType style, string color)
    {
        _top = new Border(width, style, color);
        return this;
    }

    /// <summary>Applies a border to the top side using a <see cref="CssColor"/>.</summary>
    public BorderStyle Top(string width, BorderStyleType style, CssColor color)
        => Top(width, style, color.ToString());

    /// <summary>Applies a border to the right side.</summary>
    public BorderStyle Right(string width, BorderStyleType style, string color)
    {
        _right = new Border(width, style, color);
        return this;
    }

    /// <summary>Applies a border to the right side using a <see cref="CssColor"/>.</summary>
    public BorderStyle Right(string width, BorderStyleType style, CssColor color)
        => Right(width, style, color.ToString());

    /// <summary>Applies a border to the bottom side.</summary>
    public BorderStyle Bottom(string width, BorderStyleType style, string color)
    {
        _bottom = new Border(width, style, color);
        return this;
    }

    /// <summary>Applies a border to the bottom side using a <see cref="CssColor"/>.</summary>
    public BorderStyle Bottom(string width, BorderStyleType style, CssColor color)
        => Bottom(width, style, color.ToString());

    /// <summary>Applies a border to the left side.</summary>
    public BorderStyle Left(string width, BorderStyleType style, string color)
    {
        _left = new Border(width, style, color);
        return this;
    }

    /// <summary>Applies a border to the left side using a <see cref="CssColor"/>.</summary>
    public BorderStyle Left(string width, BorderStyleType style, CssColor color)
        => Left(width, style, color.ToString());

    // ---------- PRESETS ----------

    /// <summary>Removes all borders.</summary>
    public BorderStyle None()
    {
        _all = Border.None;
        _top = _right = _bottom = _left = null;
        _radius = null;
        return this;
    }

    // ---------- RADIUS ----------

    /// <summary>Sets a uniform border radius for all corners.</summary>
    public BorderStyle Radius(int all)
    {
        _radius = BorderRadius.All(all);
        return this;
    }

    /// <summary>Sets individual border radii for each corner.</summary>
    public BorderStyle Radius(
        int? topLeft = null,
        int? topRight = null,
        int? bottomRight = null,
        int? bottomLeft = null)
    {
        _radius = new BorderRadius
        {
            TopLeft = topLeft,
            TopRight = topRight,
            BottomRight = bottomRight,
            BottomLeft = bottomLeft
        };
        return this;
    }

    // ---------- CSS VALUES (raw values without variable names) ----------

    /// <summary>Extracts the computed CSS values for all border properties.</summary>
    public BorderCssValues GetCssValues()
    {
        return new BorderCssValues
        {
            All = ToVarValue(_all),
            Top = ToVarValue(_top),
            Right = ToVarValue(_right),
            Bottom = ToVarValue(_bottom),
            Left = ToVarValue(_left),
            Radius = _radius?.ToCss()
        };

        static string? ToVarValue(Border? border)
        {
            return border is null ? null : border.IsNone ? "none" : border.ToCss();
        }
    }

    /// <summary>Gets the computed border color CSS string.</summary>
    public string? GetColorCss()
    {
        if (_all is { IsNone: false })
        {
            return _all.Color;
        }

        if (_top == null && _right == null && _bottom == null && _left == null)
        {
            return null;
        }

        string fallback = "currentColor";
        string top = _top is { IsNone: false } ? _top.Color : fallback;
        string right = _right is { IsNone: false } ? _right.Color : fallback;
        string bottom = _bottom is { IsNone: false } ? _bottom.Color : fallback;
        string left = _left is { IsNone: false } ? _left.Color : fallback;

        return top == right && right == bottom && bottom == left ? top : $"{top} {right} {bottom} {left}";
    }

    /// <summary>Gets the computed border radius CSS string.</summary>
    public string? GetRadiusCss() => _radius?.ToCss();
}

/// <summary>
/// Holds the computed CSS border values extracted from a <see cref="BorderStyle" />.
/// </summary>
public sealed class BorderCssValues
{
    /// <summary>CSS value for all sides, or <see langword="null"/> if not set.</summary>
    public string? All { get; init; }
    /// <summary>CSS value for the top side, or <see langword="null"/> if not set.</summary>
    public string? Top { get; init; }
    /// <summary>CSS value for the right side, or <see langword="null"/> if not set.</summary>
    public string? Right { get; init; }
    /// <summary>CSS value for the bottom side, or <see langword="null"/> if not set.</summary>
    public string? Bottom { get; init; }
    /// <summary>CSS value for the left side, or <see langword="null"/> if not set.</summary>
    public string? Left { get; init; }
    /// <summary>CSS border-radius value, or <see langword="null"/> if not set.</summary>
    public string? Radius { get; init; }
}

#endregion

#region Internal Models

internal sealed class Border
{
    public Border(string width, BorderStyleType style, string color)
    {
        if (style == BorderStyleType.None || width == "0" || width == "0px")
        {
            Width = "0";
            Style = BorderStyleType.None;
            Color = "transparent";
            return;
        }

        Width = width;
        Style = style;
        Color = color;
    }

    public static Border None => new("0", BorderStyleType.None, "transparent");

    public string Color { get; }
    public bool IsNone => Style == BorderStyleType.None;
    public BorderStyleType Style { get; }
    public string StyleCss => Style.ToString().ToLowerInvariant();
    public string Width { get; }

    public string ToCss() => IsNone ? "0" : $"{Width} {StyleCss} {Color}";
}

internal sealed class BorderRadius
{
    public int? BottomLeft { get; set; }
    public int? BottomRight { get; set; }
    public int? TopLeft { get; set; }
    public int? TopRight { get; set; }

    public static BorderRadius All(int value)
    {
        value = Math.Max(0, value);

        return new()
        {
            TopLeft = value,
            TopRight = value,
            BottomRight = value,
            BottomLeft = value
        };
    }

    public string ToCss()
    {
        int tl = Math.Max(0, TopLeft ?? 0);
        int tr = Math.Max(0, TopRight ?? tl);
        int br = Math.Max(0, BottomRight ?? tl);
        int bl = Math.Max(0, BottomLeft ?? tl);

        return tl == tr && tr == br && br == bl ? $"{tl}px" : $"{tl}px {tr}px {br}px {bl}px";
    }
}

#endregion

#region Enums

/// <summary>
/// Defines the CSS border-style values.
/// </summary>
public enum BorderStyleType
{
    /// <summary>No border.</summary>
    None,
    /// <summary>Solid border.</summary>
    Solid,
    /// <summary>Dashed border.</summary>
    Dashed,
    /// <summary>Dotted border.</summary>
    Dotted,
    /// <summary>Double border.</summary>
    Double,
    /// <summary>Groove border.</summary>
    Groove,
    /// <summary>Ridge border.</summary>
    Ridge,
    /// <summary>Inset border.</summary>
    Inset,
    /// <summary>Outset border.</summary>
    Outset
}

#endregion
