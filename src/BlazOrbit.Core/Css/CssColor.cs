using System.Globalization;
using System.Text.RegularExpressions;

namespace BlazOrbit.Components;

/// <summary>
/// The <see cref="CssColor" /> output formats.
/// </summary>
public enum ColorOutputFormats
{
    /// <summary>
    /// Output will be starting with a # and include r,g and b but no alpha values. Example #ab2a3d
    /// </summary>
    Hex,

    /// <summary>
    /// Output will be starting with a # and include r,g and b and alpha values. Example #ab2a3dff
    /// </summary>
    HexA,

    /// <summary>
    /// Will output css like output for value. Example rgb(12,15,40)
    /// </summary>
    Rgb,

    /// <summary>
    /// Will output css like output for value with alpha. Example rgba(12,15,40,0.42)
    /// </summary>
    Rgba,

    /// <summary>
    /// Will output the color elements optimized for CSS usage.
    /// </summary>
    Optimized
}

/// <summary>
/// The Css color representation.
/// </summary>
public sealed class CssColor : IEquatable<CssColor>
{
    #region Fields and Properties

    private const double Epsilon = 0.000000000000001;

    //private readonly ColorVariant? _associatedColorVariant;
    private readonly byte[] _valuesAsByte;

    /// <summary>
    /// The Alpha value.
    /// </summary>
    public byte A => _valuesAsByte[3];

    /// <summary>
    /// The Alpha value percentage.
    /// </summary>
    public double APercentage => Math.Round(A / 255.0, 2);

    /// <summary>
    /// The Blue value.
    /// </summary>
    public byte B => _valuesAsByte[2];

    /// <summary>
    /// The Green Value.
    /// </summary>
    public byte G => _valuesAsByte[1];

    /// <summary>
    /// The Hue Value.
    /// </summary>
    public double H { get; private set; }

    /// <summary>
    /// The Luminosity Value.
    /// </summary>
    public double L { get; private set; }

    /// <summary>
    /// The Red Value.
    /// </summary>
    public byte R => _valuesAsByte[0];

    /// <summary>
    /// The Saturation Value.
    /// </summary>
    public double S { get; private set; }

    /// <summary>
    /// The Color Value.
    /// </summary>
    public string Value => $"#{R:x2}{G:x2}{B:x2}{A:x2}";

    /// <summary>
    /// Gets the alpha percentage as string.
    /// </summary>
    /// <param name="floatCharacter">
    /// </param>
    /// <returns>
    /// </returns>
    public string APercentageString(char? floatCharacter = '.')
        => APercentage
            .ToString(CultureInfo.InvariantCulture)
            .Replace(',', floatCharacter!.Value);

    #endregion Fields and Properties

    #region Constructor

    /// <summary>
    /// Constructs a CssColor from HSL and alpha values (double alpha)
    /// </summary>
    public CssColor(double h, double s, double l, double a)
        : this(h, s, l, (int)(a * 255.0).EnsureRange(255))
    {
    }

    /// <summary>
    /// Constructs a CssColor from HSL and alpha values (int alpha)
    /// </summary>
    public CssColor(double h, double s, double l, int a)
    {
        h = Math.Round(h.EnsureRange(360), 0);
        s = Math.Round(s.EnsureRange(1), 2);
        l = Math.Round(l.EnsureRange(1), 2);
        a = a.EnsureRange(255);

        _valuesAsByte = new byte[4];

        // Achromatic (gray scale)
        if (Math.Abs(s) < Epsilon)
        {
            byte gray = ((int)Math.Ceiling(l * 255D)).EnsureRangeToByte();
            _valuesAsByte[0] = gray;
            _valuesAsByte[1] = gray;
            _valuesAsByte[2] = gray;
            _valuesAsByte[3] = (byte)a;
        }
        else
        {
            double q = l < .5D ? l * (1D + s) : l + s - (l * s);
            double p = (2D * l) - q;

            double hk = h / 360D;
            double[] T = new double[3];
            T[0] = hk + (1D / 3D);
            T[1] = hk;
            T[2] = hk - (1D / 3D);

            for (int i = 0; i < 3; i++)
            {
                if (T[i] < 0D)
                {
                    T[i] += 1D;
                }

                if (T[i] > 1D)
                {
                    T[i] -= 1D;
                }

                T[i] = T[i] * 6D < 1D ? p + ((q - p) * 6D * T[i]) : T[i] * 2D < 1D ? q : T[i] * 3D < 2D ? p + ((q - p) * ((2D / 3D) - T[i]) * 6D) : p;
            }

            _valuesAsByte[0] = ((int)Math.Round(T[0] * 255D)).EnsureRangeToByte();
            _valuesAsByte[1] = ((int)Math.Round(T[1] * 255D)).EnsureRangeToByte();
            _valuesAsByte[2] = ((int)Math.Round(T[2] * 255D)).EnsureRangeToByte();
            _valuesAsByte[3] = (byte)a;
        }

        H = Math.Round(h, 0);
        S = Math.Round(s, 2);
        L = Math.Round(l, 2);
    }

    /// <summary>
    /// Constructs a CssColor from RGBA bytes
    /// </summary>
    public CssColor(byte r, byte g, byte b, byte a)
    {
        _valuesAsByte = new[] { r, g, b, a };
        CalculateHsl();
    }

    /// <summary>
    /// Constructs a CssColor from RGBA integers and double alpha
    /// </summary>
    public CssColor(int r, int g, int b, double alpha)
        : this(r, g, b, (byte)(alpha * 255.0).EnsureRange(255))
    {
    }

    /// <summary>
    /// Constructs a CssColor from RGBA integers and int alpha
    /// </summary>
    public CssColor(int r, int g, int b, int alpha)
        : this((byte)r.EnsureRange(255), (byte)g.EnsureRange(255), (byte)b.EnsureRange(255), (byte)alpha.EnsureRange(255))
    {
    }

    /// <summary>
    /// Constructs a CssColor from RGBA and optional variant
    /// </summary>
    public CssColor(byte r, byte g, byte b, byte a, CssColorVariant? colorVariant = null)
        : this(r, g, b, a)
    {
        if (colorVariant != null)
        {
            CssColor modifiedColor = colorVariant.Mode switch
            {
                CssColorVariant.Modifier.Darken => ColorDarken(colorVariant.Alteration),
                CssColorVariant.Modifier.Lighten => ColorLighten(colorVariant.Alteration),
                _ => throw new ArgumentOutOfRangeException(nameof(colorVariant), colorVariant, null),
            };

            _valuesAsByte = new[] { modifiedColor.R, modifiedColor.G, modifiedColor.B, modifiedColor.A };
            CalculateHsl();
        }
    }

    /// <summary>
    /// Constructs a CssColor from string representation (RGB/RGBA/HEX)
    /// </summary>
    public CssColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Color value cannot be null or empty.", nameof(value));
        }

        value = value.Trim().ToLowerInvariant();
        byte[] bytes = value switch
        {
            _ when IsRgb(value) => ParseRgb(value),
            _ when IsHex(value) => ParseHex(value),
            _ => throw new ArgumentException("Invalid CSS color format.", nameof(value))
        };
        _valuesAsByte = bytes;
        CalculateHsl();
    }

    private static bool IsHex(string value)
        => Regex.IsMatch(
            value.StartsWith('#') ? value[1..] : value,
            @"^[0-9a-f]{3}$|^[0-9a-f]{4}$|^[0-9a-f]{6}$|^[0-9a-f]{8}$",
            RegexOptions.CultureInvariant
        );

    private static bool IsRgb(string value)
            => Regex.IsMatch(
            value,
            @"^rgba?\(\s*\d{1,3}\s*,\s*\d{1,3}\s*,\s*\d{1,3}(\s*,\s*(0(\.\d+)?|1(\.0+)?))?\s*\)$",
            RegexOptions.CultureInvariant
        );

    private static byte ParseAlpha(string value)
    {
        return !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double alpha)
            || alpha < 0 || alpha > 1
            ? throw new ArgumentOutOfRangeException(nameof(value), "Alpha must be between 0 and 1.")
            : (byte)Math.Round(alpha * 255);
    }

    private static byte ParseByte(string value, string channel)
    {
        return !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
            || result < 0 || result > 255
            ? throw new ArgumentOutOfRangeException(channel, "RGB values must be between 0 and 255.")
            : (byte)result;
    }

    private static byte[] ParseHex(string value)
    {
        if (value.StartsWith('#'))
        {
            value = value[1..];
        }

        value = value.Length switch
        {
            3 => $"{value[0]}{value[0]}{value[1]}{value[1]}{value[2]}{value[2]}ff",
            4 => $"{value[0]}{value[0]}{value[1]}{value[1]}{value[2]}{value[2]}{value[3]}{value[3]}",
            6 => value + "ff",
            8 => value,
            _ => throw new ArgumentException("Invalid hex color format.", nameof(value))
        };

        return new[]
        {
        GetByteFromValuePart(value, 0),
        GetByteFromValuePart(value, 2),
        GetByteFromValuePart(value, 4),
        GetByteFromValuePart(value, 6)
    };
    }

    private static byte[] ParseRgb(string value)
    {
        string[] parts = SplitInputIntoParts(value);

        if ((value.StartsWith("rgba") && parts.Length != 4) ||
            (value.StartsWith("rgb") && !value.StartsWith("rgba") && parts.Length != 3))
        {
            throw new ArgumentException("Invalid rgb/rgba format.", nameof(value));
        }

        byte r = ParseByte(parts[0], "R");
        byte g = ParseByte(parts[1], "G");
        byte b = ParseByte(parts[2], "B");
        byte a = parts.Length == 4 ? ParseAlpha(parts[3]) : (byte)255;

        return new[] { r, g, b, a };
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Modify Lightness value.
    /// </summary>
    /// <param name="amount">
    /// </param>
    /// <returns>
    /// </returns>
    public CssColor ChangeLightness(double amount) => new(H, S, (L + amount).EnsureRange(0, 1), A);

    /// <summary>
    /// Get darken color from current instance.
    /// </summary>
    /// <param name="amount">
    /// </param>
    /// <returns>
    /// </returns>
    public CssColor ColorDarken(double amount) => ChangeLightness(-amount);

    /// <summary>
    /// Get lighten color from current instance.
    /// </summary>
    /// <param name="amount">
    /// </param>
    /// <returns>
    /// </returns>
    public CssColor ColorLighten(double amount) => ChangeLightness(+amount);

    /// <summary>
    /// Sets the alpha value for current instance.
    /// </summary>
    /// <param name="a">
    /// </param>
    /// <returns>
    /// </returns>
    public CssColor SetAlpha(int a) => new(R, G, B, a);

    /// <summary>
    /// Sets the alpha value for current instance.
    /// </summary>
    /// <param name="a">
    /// </param>
    /// <returns>
    /// </returns>
    public CssColor SetAlpha(double a) => new(R, G, B, a);

    private void CalculateHsl()
    {
        double h = 0D;
        double s = 0D;
        double l;

        // normalize red, green, blue values
        double r = R / 255D;
        double g = G / 255D;
        double b = B / 255D;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));

        // hue
        if (Math.Abs(max - min) < Epsilon)
        {
            h = 0D; // undefined
        }
        else if (Math.Abs(max - r) < Epsilon
                 && g >= b)
        {
            h = 60D * (g - b) / (max - min);
        }
        else if (Math.Abs(max - r) < Epsilon
                 && g < b)
        {
            h = (60D * (g - b) / (max - min)) + 360D;
        }
        else if (Math.Abs(max - g) < Epsilon)
        {
            h = (60D * (b - r) / (max - min)) + 120D;
        }
        else if (Math.Abs(max - b) < Epsilon)
        {
            h = (60D * (r - g) / (max - min)) + 240D;
        }

        // luminance
        l = (max + min) / 2D;

        // saturation
        if (Math.Abs(l) < Epsilon
            || Math.Abs(max - min) < Epsilon)
        {
            s = 0D;
        }
        else if (l is > 0D and <= .5D)
        {
            s = (max - min) / (max + min);
        }
        else if (l > .5D)
        {
            s = (max - min) / (2D - (max + min)); //(max-min > 0)?
        }

        H = Math.Round(h.EnsureRange(360), 0);
        S = Math.Round(s.EnsureRange(1), 2);
        L = Math.Round(l.EnsureRange(1), 2);
    }

    #endregion Methods

    #region Contrast Helpers

    /// <summary>
    /// Returns the best contrast color (black or white) for this color. Uses CSS variables with
    /// fallback if no custom values are set.
    /// </summary>
    public CssColor GetBestContrast(CssColor contrastBlack, CssColor contrastWhite)
    {
        double L = GetRelativeLuminance();

        // WCAG contrast formula
        double contrastWithBlack = (L + 0.05) / 0.05;
        double contrastWithWhite = 1.05 / (L + 0.05);

        return contrastWithWhite > contrastWithBlack ? contrastWhite : contrastBlack;
    }

    /// <summary>
    /// Returns the relative luminance of the current color (0 = dark, 1 = light) using the WCAG 2.1 formula.
    /// </summary>
    public double GetRelativeLuminance()
    {
        if (_valuesAsByte == null)
        {
            return 0.5;
        }

        double RsRGB = R / 255.0;
        double GsRGB = G / 255.0;
        double BsRGB = B / 255.0;

        double Rlin = RsRGB <= 0.03928 ? RsRGB / 12.92 : Math.Pow((RsRGB + 0.055) / 1.055, 2.4);
        double Glin = GsRGB <= 0.03928 ? GsRGB / 12.92 : Math.Pow((GsRGB + 0.055) / 1.055, 2.4);
        double Blin = BsRGB <= 0.03928 ? BsRGB / 12.92 : Math.Pow((BsRGB + 0.055) / 1.055, 2.4);

        return (0.2126 * Rlin) + (0.7152 * Glin) + (0.0722 * Blin);
    }

    #endregion Contrast Helpers

    #region Helper

    private static byte GetByteFromValuePart(string input, int index) => byte.Parse(new string(new[] { input[index], input[index + 1] }), NumberStyles.HexNumber);

    private static string[] SplitInputIntoParts(string value)
    {
        int startIndex = value.IndexOf('(');
        int lastIndex = value.LastIndexOf(')');
        string subString = value[(startIndex + 1)..lastIndex];
        string[] parts = subString.Split(',', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();
        }

        return parts;
    }

    #endregion Helper

    #region operators and object members

    /// <summary>
    /// Renders the color as a CSS string. Explicit because the conversion
    /// is lossy (the format defaults to <c>#rrggbb</c>) and the implicit
    /// version would silently absorb <c>null</c> into <see cref="string.Empty"/>
    /// in unexpected contexts. Prefer <see cref="ToString()"/> directly when
    /// the call site is obvious.
    /// </summary>
    public static explicit operator string(CssColor? color)
    {
        return color?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Parses a CSS color string (hex / rgb / rgba). Explicit because the
    /// constructor throws on malformed input and the implicit version made
    /// it easy to feed unvalidated user data straight into a parse. Use
    /// <see cref="TryParse"/> when the input might fail; use <see cref="Parse"/>
    /// or this cast when you've already validated it.
    /// </summary>
    public static explicit operator CssColor(string input)
    {
        return new(input);
    }

    /// <summary>Parses a CSS color string. Throws <see cref="ArgumentException"/> on malformed input.</summary>
    public static CssColor Parse(string value) => new(value);

    /// <summary>
    /// Tries to parse a CSS color string. Returns <c>true</c> on success and
    /// sets <paramref name="result"/>; returns <c>false</c> for null / empty
    /// / unrecognized formats without throwing.
    /// </summary>
    public static bool TryParse(string? value, out CssColor? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return false;
        }

        try
        {
            result = new CssColor(value);
            return true;
        }
        catch (ArgumentException)
        {
            // Covers ArgumentOutOfRangeException (e.g. alpha out of range).
            result = null;
            return false;
        }
    }

    public static bool operator !=(CssColor? lhs, CssColor? rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator ==(CssColor? lhs, CssColor? rhs)
    {
        bool lhsIsNull = lhs is null;
        bool rhsIsNull = rhs is null;
        return (lhsIsNull && rhsIsNull) || (!lhsIsNull && !rhsIsNull && lhs!.Equals(rhs!));
    }

    public override bool Equals(object? obj) => obj is CssColor color && Equals(color);

    public bool Equals(CssColor? other)
    {
        return other is not null && _valuesAsByte is not null && other._valuesAsByte is not null && _valuesAsByte[0] == other._valuesAsByte[0] &&
            _valuesAsByte[1] == other._valuesAsByte[1] &&
            _valuesAsByte[2] == other._valuesAsByte[2] &&
            _valuesAsByte[3] == other._valuesAsByte[3];
    }

    public override int GetHashCode()
    {
        return _valuesAsByte is null
            ? 0
            : HashCode.Combine(
            _valuesAsByte[0],
            _valuesAsByte[1],
            _valuesAsByte[2],
            _valuesAsByte[3]
        );
    }

    public override string ToString() => ToString(ColorOutputFormats.Rgba);

    public string ToString(ColorOutputFormats format)
    {
        return format switch
        {
            ColorOutputFormats.Hex => Value[..7],
            ColorOutputFormats.HexA => Value,
            ColorOutputFormats.Rgb => $"rgb({R},{G},{B})",
            ColorOutputFormats.Rgba => $"rgba({R},{G},{B},{APercentage.ToString(CultureInfo.InvariantCulture)})",
            ColorOutputFormats.Optimized => ToOptimizedFormat(),
            _ => Value
        };
    }

    private string ToOptimizedFormat()
    {
        return A < 255
            ? $"rgba({R},{G},{B},{APercentage.ToString(CultureInfo.InvariantCulture)})"
            : CanUseShortHex() ? $"#{R & 0x0F:x}{G & 0x0F:x}{B & 0x0F:x}" : $"#{R:x2}{G:x2}{B:x2}";
    }

    private bool CanUseShortHex()
    {
        return (R >> 4) == (R & 0x0F) &&
               (G >> 4) == (G & 0x0F) &&
               (B >> 4) == (B & 0x0F);
    }

    #endregion operators and object members
}

public sealed class CssColorVariant
{
    private const double VariantModifier = 0.030; // 5% per alteration step

    public CssColorVariant(Modifier modifier, double alteration)
    {
        Mode = modifier;
        Alteration = alteration;
    }

    public enum Modifier
    {
        Darken,
        Lighten
    }

    public double Alteration { get; set; }
    public Modifier Mode { get; set; }

    public static CssColorVariant Darken(int alteration) => new(Modifier.Darken, VariantModifier * alteration);

    public static CssColorVariant Lighten(int alteration) => new(Modifier.Lighten, VariantModifier * alteration);
}

internal static class NumberExtensions
{
    public static double EnsureRange(this double input, double max) => input.EnsureRange(0.0, max);

    public static double EnsureRange(this double input, double min, double max) => Math.Max(min, Math.Min(max, input));

    public static byte EnsureRange(this byte input, byte max) => input.EnsureRange((byte)0, max);

    public static byte EnsureRange(this byte input, byte min, byte max) => Math.Max(min, Math.Min(max, input));

    public static int EnsureRange(this int input, int max) => input.EnsureRange(0, max);

    public static int EnsureRange(this int input, int min, int max) => Math.Max(min, Math.Min(max, input));

    public static byte EnsureRangeToByte(this int input) => (byte)input.EnsureRange(0, 255);
}