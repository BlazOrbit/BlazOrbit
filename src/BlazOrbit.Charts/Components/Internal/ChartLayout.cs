using System.Globalization;

namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Pixel-space rectangle that the actual chart geometry is drawn into.
/// Margins around the plot area are reserved for axis labels and ticks.
/// All values are doubles to keep arithmetic precise; the SVG output
/// formats with two-decimal precision via <see cref="ToInvariant(double)"/>.
/// </summary>
internal readonly record struct ChartLayout(
    double Width,
    double Height,
    double PlotLeft,
    double PlotTop,
    double PlotRight,
    double PlotBottom)
{
    public double PlotWidth => PlotRight - PlotLeft;
    public double PlotHeight => PlotBottom - PlotTop;

    /// <summary>
    /// Default margins suitable for a Bar / Line / Area chart with axis
    /// labels: 56px left for Y-axis tick labels, 32px bottom for X-axis tick
    /// labels, 12px top + right for breathing room.
    /// </summary>
    public static ChartLayout Default(double width, double height)
        => new(
            width,
            height,
            56,
            12,
            width - 12,
            height - 32);

    /// <summary>
    /// Edge-to-edge layout for sparkline mode (no axis padding). Reserves
    /// a 1px outer inset so antialiased strokes don't clip at the SVG
    /// viewport boundary.
    /// </summary>
    public static ChartLayout Sparkline(double width, double height)
        => new(
            width,
            height,
            1,
            1,
            width - 1,
            height - 1);

    /// <summary>Format a double to two decimals using the invariant culture.</summary>
    public static string ToInvariant(double v)
        => v.ToString("F2", CultureInfo.InvariantCulture);
}