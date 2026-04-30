namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a <see cref="Shadow" /> parameter.
/// </summary>
public interface IHasShadow
{
    /// <summary>Shadow style applied to the component.</summary>
    ShadowStyle? Shadow { get; set; }
}

/// <summary>
/// Represents a single line (layer) within a <see cref="ShadowStyle" />.
/// </summary>
public readonly struct ShadowLine
{
    /// <summary>Shadow color.</summary>
    public readonly string Color;
    /// <summary>When <see langword="true" />, the shadow is inset.</summary>
    public readonly bool Inset;
    /// <summary>Shadow opacity (0–1).</summary>
    public readonly float Opacity;
    /// <summary>Horizontal offset in pixels.</summary>
    public readonly int X;
    /// <summary>Vertical offset in pixels.</summary>
    public readonly int Y;
    /// <summary>Blur radius in pixels.</summary>
    public readonly int Blur;
    /// <summary>Spread radius in pixels.</summary>
    public readonly int Spread;

    internal ShadowLine(
        int x, int y, int blur, int spread,
        float opacity, string color, bool inset)
    {
        X = x;
        Y = y;
        Blur = blur;
        Spread = spread;
        Opacity = opacity;
        Color = color;
        Inset = inset;
    }
}

/// <summary>
/// Fluent builder for constructing CSS box-shadow styles.
/// </summary>
public sealed class ShadowStyle
{
    private readonly List<ShadowLine> _lines = [];

    private ShadowStyle()
    { }

    internal IReadOnlyList<ShadowLine> Lines => _lines;

    /// <summary>Creates a new <see cref="ShadowStyle" /> with a single shadow line.</summary>
    public static ShadowStyle Create(
        int y,
        int blur,
        float opacity = 0.2f,
        int x = 0,
        int spread = 0,
        string? color = null,
        bool inset = false)
    {
        ShadowStyle style = new();
        style._lines.Add(new ShadowLine(
            x, y, blur, spread,
            opacity,
            color ?? "#000000",
            inset));

        return style;
    }

    /// <summary>Adds an additional shadow line to the style.</summary>
    public ShadowStyle Add(
        int y,
        int blur,
        float opacity = 0.2f,
        int x = 0,
        int spread = 0,
        string? color = null,
        bool inset = false)
    {
        _lines.Add(new ShadowLine(
            x, y, blur, spread,
            opacity,
            color ?? "#000000",
            inset));

        return this;
    }

    /// <summary>Generates the CSS <c>box-shadow</c> value.</summary>
    public string ToCss()
    {
        return string.Join(", ", _lines.Select(l =>
            $"{(l.Inset ? "inset " : "")}" +
            $"{l.X}px {l.Y}px {l.Blur}px {l.Spread}px " +
            $"color-mix(in srgb, {l.Color} {l.Opacity * 100}%, transparent)"
        ));
    }
}

/// <summary>
/// Provides predefined <see cref="ShadowStyle" /> instances based on Material Design elevation levels.
/// </summary>
public static class BOBShadowPresets
{
    /// <summary>Creates a shadow for the specified elevation level (0–24).</summary>
    public static ShadowStyle Elevation(int level, string? color = null)
    {
        color ??= PaletteColor.Shadow;
        level = Math.Clamp(level, 0, 24);

        if (level == 0)
        {
            return ShadowStyle.Create(0, 0, 0f, color: color);
        }

        (int keyY, int keyBlur, float keyOpacity) = GetKeyShadow(level);
        (int ambientY, int ambientBlur, float ambientOpacity) = GetAmbientShadow(level);

        return ShadowStyle
            .Create(keyY, keyBlur, keyOpacity, color: color)
            .Add(ambientY, ambientBlur, ambientOpacity, color: color);
    }

    private static (int y, int blur, float opacity) GetAmbientShadow(int level)
    {
        // Ambient shadow: soft, diffuse, represents scattered ambient light
        return level switch
        {
            0 => (0, 0, 0.00f),
            1 => (1, 2, 0.12f),
            2 => (2, 3, 0.12f),
            3 => (3, 4, 0.12f),
            4 => (4, 5, 0.12f),
            5 => (4, 8, 0.13f),
            6 => (5, 10, 0.14f),
            7 => (5, 11, 0.14f),
            8 => (6, 12, 0.14f),
            9 => (7, 14, 0.14f),
            10 => (8, 16, 0.15f),
            11 => (9, 16, 0.15f),
            12 => (10, 18, 0.16f),
            13 => (11, 20, 0.16f),
            14 => (12, 22, 0.16f),
            15 => (13, 23, 0.16f),
            16 => (14, 24, 0.16f),
            17 => (15, 26, 0.17f),
            18 => (16, 28, 0.17f),
            19 => (17, 30, 0.17f),
            20 => (18, 32, 0.17f),
            21 => (19, 34, 0.18f),
            22 => (19, 35, 0.18f),
            23 => (20, 36, 0.18f),
            24 => (20, 38, 0.18f),

            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }

    private static (int y, int blur, float opacity) GetKeyShadow(int level)
    {
        // Key shadow: directional, represents direct light occlusion Based on Material Design 3
        // elevation system
        return level switch
        {
            0 => (0, 0, 0.00f),
            1 => (1, 3, 0.20f),
            2 => (2, 4, 0.20f),
            3 => (3, 5, 0.20f),
            4 => (4, 6, 0.20f),
            5 => (5, 7, 0.20f),
            6 => (6, 8, 0.20f),
            7 => (7, 9, 0.21f),
            8 => (8, 10, 0.22f),
            9 => (9, 12, 0.22f),
            10 => (10, 13, 0.23f),
            11 => (11, 13, 0.23f),
            12 => (12, 14, 0.24f),
            13 => (13, 15, 0.24f),
            14 => (14, 16, 0.24f),
            15 => (15, 17, 0.24f),
            16 => (16, 18, 0.24f),
            17 => (17, 19, 0.25f),
            18 => (18, 20, 0.25f),
            19 => (19, 21, 0.25f),
            20 => (20, 22, 0.25f),
            21 => (21, 22, 0.26f),
            22 => (22, 23, 0.26f),
            23 => (23, 23, 0.26f),
            24 => (24, 24, 0.26f),

            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }
}
