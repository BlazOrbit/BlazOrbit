namespace BlazOrbit.Components;

/// <summary>
/// Represents a color in HSV (Hue, Saturation, Value) color space. Used internally by the color
/// picker component.
/// </summary>
public readonly struct HsvColor : IEquatable<HsvColor>
{
    public HsvColor(int hue, double saturation, double value)
    {
        Hue = Math.Clamp(hue, 0, 360);
        Saturation = Math.Clamp(saturation, 0.0, 1.0);
        Value = Math.Clamp(value, 0.0, 1.0);
    }

    public int Hue { get; }
    public double Saturation { get; }
    public double Value { get; }

    public static HsvColor FromCssColor(CssColor color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double h = 0;
        if (delta > 0)
        {
            h = Math.Abs(max - r) < 0.0001
                ? 60 * ((g - b) / delta % 6)
                : Math.Abs(max - g) < 0.0001 ? 60 * (((b - r) / delta) + 2) : 60 * (((r - g) / delta) + 4);
        }

        if (h < 0)
        {
            h += 360;
        }

        double s = max > 0 ? delta / max : 0;
        double v = max;

        return new HsvColor((int)Math.Round(h), Math.Round(s, 4), Math.Round(v, 4));
    }

    public static bool operator !=(HsvColor left, HsvColor right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(HsvColor left, HsvColor right)
    {
        return left.Equals(right);
    }

    public bool Equals(HsvColor other)
        => Hue == other.Hue &&
        Math.Abs(Saturation - other.Saturation) < 0.0001 &&
        Math.Abs(Value - other.Value) < 0.0001;

    public override bool Equals(object? obj) => obj is HsvColor other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Hue, Saturation, Value);

    public CssColor ToCssColor(byte alpha = 255)
    {
        double c = Value * Saturation;
        double x = c * (1 - Math.Abs((Hue / 60.0 % 2) - 1));
        double m = Value - c;

        double r, g, b;

        (r, g, b) = Hue switch
        {
            < 60 => ((double, double, double))(c, x, 0),
            < 120 => ((double, double, double))(x, c, 0),
            < 180 => ((double, double, double))(0, c, x),
            < 240 => ((double, double, double))(0, x, c),
            < 300 => ((double, double, double))(x, 0, c),
            _ => ((double, double, double))(c, 0, x),
        };
        return new CssColor(
            (int)Math.Round((r + m) * 255),
            (int)Math.Round((g + m) * 255),
            (int)Math.Round((b + m) * 255),
            alpha
        );
    }

    public HsvColor WithHue(int hue) => new(hue, Saturation, Value);

    public HsvColor WithSaturation(double saturation) => new(Hue, saturation, Value);

    public HsvColor WithValue(double value) => new(Hue, Saturation, value);
}