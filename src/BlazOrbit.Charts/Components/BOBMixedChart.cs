using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;
using System.Text;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Combined-series chart: bar + line + area in the same plot, sharing the X axis and
/// optionally a secondary Y axis (right side, distinct range). Useful for
/// "actual vs target" / "value vs ratio" / "stock vs index" comparisons where one
/// series belongs to a different domain than the others.
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
public sealed class BOBMixedChart<TX> : BOBChartBase<TX, double>
    where TX : notnull
{
    /// <summary>Series rendered in this chart. Each series picks its own visual type.</summary>
    [Parameter]
    public IEnumerable<BOBChartMixedSeries<TX>>? Series { get; set; }

    /// <summary>
    /// When at least one series sets <c>UseSecondaryAxis</c>, the secondary scale is
    /// rendered on the right edge. Title shown above the axis ticks.
    /// </summary>
    [Parameter]
    public string? SecondaryAxisLabel { get; set; }

    /// <summary>Width fraction of the band each bar group occupies. Default 0.7.</summary>
    [Parameter]
    public double BarGroupRatio { get; set; } = 0.7;

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int n = Series?.Count() ?? 0;
        return n == 0 ? "Mixed chart with no series" : $"Mixed chart with {n} series";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Series is null)
        {
            return;
        }

        BOBChartMixedSeries<TX>[] series = Series
            .Where(s => !IsSeriesHidden(s.Label))
            .ToArray();
        if (series.Length == 0)
        {
            return;
        }

        ChartLayout layout = ChartLayout.Default(EffectiveWidth, EffectiveHeight);

        // Build the categorical X axis from the union of points across all series so
        // missing points in a particular series don't shift the band positions.
        IEnumerable<TX> categories = series.SelectMany(s => s.Points.Select(p => p.X)).Distinct();
        CategoricalScale<TX> xScale = new(categories, layout.PlotLeft, layout.PlotRight);
        double bandWidth = xScale.BandWidth;

        // Split Y range: primary axis = series without UseSecondaryAxis; secondary axis
        // = series with UseSecondaryAxis. Each axis spans the observed min/max of its
        // own series.
        (double pMin, double pMax) = ExtractRange(series.Where(s => !s.UseSecondaryAxis));
        (double sMin, double sMax) = ExtractRange(series.Where(s => s.UseSecondaryAxis));
        LinearScale yPrimary = new([pMin, pMax], layout.PlotBottom, layout.PlotTop, includeZero: true);
        bool hasSecondary = series.Any(s => s.UseSecondaryAxis);
        LinearScale ySecondary = hasSecondary
            ? new LinearScale([sMin, sMax], layout.PlotBottom, layout.PlotTop, includeZero: true)
            : yPrimary;

        int seq = 100;

        RenderAxes(builder, ref seq, layout, yPrimary, ySecondary, hasSecondary);

        // Bar series share a band - they sit side-by-side within each category. Compute
        // per-bar width and offset based on how many bar series there are.
        int barSeriesCount = series.Count(s => s.Type == BOBChartMixedSeriesType.Bar);
        double groupWidth = bandWidth * BarGroupRatio;
        double barWidth = barSeriesCount > 0 ? groupWidth / barSeriesCount : groupWidth;
        int barIdx = 0;

        // Render bars first (background), then areas, then lines on top so the visual
        // stack reads in the natural priority order.
        for (int i = 0; i < series.Length; i++)
        {
            BOBChartMixedSeries<TX> s = series[i];
            if (s.Type != BOBChartMixedSeriesType.Bar) { continue; }
            LinearScale yScale = s.UseSecondaryAxis ? ySecondary : yPrimary;
            RenderBars(builder, ref seq, s, xScale, yScale, i, barIdx, barSeriesCount, barWidth, groupWidth, layout);
            barIdx++;
        }

        for (int i = 0; i < series.Length; i++)
        {
            BOBChartMixedSeries<TX> s = series[i];
            if (s.Type != BOBChartMixedSeriesType.Area) { continue; }
            LinearScale yScale = s.UseSecondaryAxis ? ySecondary : yPrimary;
            RenderAreaOrLine(builder, ref seq, s, xScale, yScale, i, layout, fill: true);
        }

        for (int i = 0; i < series.Length; i++)
        {
            BOBChartMixedSeries<TX> s = series[i];
            if (s.Type != BOBChartMixedSeriesType.Line) { continue; }
            LinearScale yScale = s.UseSecondaryAxis ? ySecondary : yPrimary;
            RenderAreaOrLine(builder, ref seq, s, xScale, yScale, i, layout, fill: false);
        }
    }

    private static (double Min, double Max) ExtractRange(IEnumerable<BOBChartMixedSeries<TX>> series)
    {
        double min = double.PositiveInfinity;
        double max = double.NegativeInfinity;
        foreach (BOBChartMixedSeries<TX> s in series)
        {
            foreach (BOBChartPoint<TX, double> p in s.Points)
            {
                if (p.Y < min) { min = p.Y; }
                if (p.Y > max) { max = p.Y; }
            }
        }

        if (double.IsPositiveInfinity(min) || double.IsNegativeInfinity(max))
        {
            return (0, 1);
        }

        return (min, max);
    }

    private void RenderAxes(
        RenderTreeBuilder builder, ref int seq,
        ChartLayout layout, LinearScale yPrimary, LinearScale ySecondary, bool hasSecondary)
    {
        // Y axis grid lines + tick labels (primary, left).
        foreach (double tick in yPrimary.Ticks())
        {
            double y = yPrimary.Project(tick);
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "class", "bob-mixed-chart__grid");
            builder.AddAttribute(seq++, "x1", Squarified.F(layout.PlotLeft));
            builder.AddAttribute(seq++, "x2", Squarified.F(layout.PlotRight));
            builder.AddAttribute(seq++, "y1", Squarified.F(y));
            builder.AddAttribute(seq++, "y2", Squarified.F(y));
            builder.AddAttribute(seq++, "stroke", "var(--palette-border, #ddd)");
            builder.AddAttribute(seq++, "stroke-opacity", "0.3");
            builder.CloseElement();

            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-mixed-chart__axis-label");
            builder.AddAttribute(seq++, "x", Squarified.F(layout.PlotLeft - 8));
            builder.AddAttribute(seq++, "y", Squarified.F(y + 4));
            builder.AddAttribute(seq++, "text-anchor", "end");
            builder.AddAttribute(seq++, "font-size", "11");
            builder.AddContent(seq++, tick.ToString("N1", CultureInfo.InvariantCulture));
            builder.CloseElement();
        }

        // Secondary axis ticks on the right edge.
        if (hasSecondary)
        {
            foreach (double tick in ySecondary.Ticks())
            {
                double y = ySecondary.Project(tick);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "class", "bob-mixed-chart__axis-label bob-mixed-chart__axis-label--secondary");
                builder.AddAttribute(seq++, "x", Squarified.F(layout.PlotRight + 8));
                builder.AddAttribute(seq++, "y", Squarified.F(y + 4));
                builder.AddAttribute(seq++, "text-anchor", "start");
                builder.AddAttribute(seq++, "font-size", "11");
                builder.AddContent(seq++, tick.ToString("N1", CultureInfo.InvariantCulture));
                builder.CloseElement();
            }

            if (!string.IsNullOrEmpty(SecondaryAxisLabel))
            {
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "class", "bob-mixed-chart__axis-title");
                builder.AddAttribute(seq++, "x", Squarified.F(layout.PlotRight + 8));
                builder.AddAttribute(seq++, "y", Squarified.F(layout.PlotTop - 8));
                builder.AddAttribute(seq++, "font-size", "11");
                builder.AddAttribute(seq++, "fill", "var(--palette-surface-contrast, currentColor)");
                builder.AddContent(seq++, SecondaryAxisLabel);
                builder.CloseElement();
            }
        }
    }

    private static void RenderBars(
        RenderTreeBuilder builder, ref int seq,
        BOBChartMixedSeries<TX> s, CategoricalScale<TX> xScale, LinearScale yScale,
        int seriesIndex, int barIdx, int barSeriesCount, double barWidth, double groupWidth,
        ChartLayout layout)
    {
        string color = s.Color ?? Palette.ColorAt(seriesIndex);
        double zeroY = yScale.Project(0);

        foreach (BOBChartPoint<TX, double> p in s.Points)
        {
            double cx = xScale.Center(p.X);
            double x0 = cx - (groupWidth / 2) + (barIdx * barWidth);
            double y = yScale.Project(p.Y);
            double yTop = Math.Min(y, zeroY);
            double height = Math.Abs(y - zeroY);

            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-mixed-chart__bar");
            builder.AddAttribute(seq++, "x", Squarified.F(x0));
            builder.AddAttribute(seq++, "y", Squarified.F(yTop));
            builder.AddAttribute(seq++, "width", Squarified.F(barWidth));
            builder.AddAttribute(seq++, "height", Squarified.F(height));
            builder.AddAttribute(seq++, "fill", color);
            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, string.Format(CultureInfo.InvariantCulture, "{0}: {1} = {2:N2}", s.Label, p.X, p.Y));
            builder.CloseElement();
            builder.CloseElement();
        }
    }

    private static void RenderAreaOrLine(
        RenderTreeBuilder builder, ref int seq,
        BOBChartMixedSeries<TX> s, CategoricalScale<TX> xScale, LinearScale yScale,
        int seriesIndex, ChartLayout layout, bool fill)
    {
        string color = s.Color ?? Palette.ColorAt(seriesIndex);
        BOBChartPoint<TX, double>[] points = s.Points.ToArray();
        if (points.Length == 0)
        {
            return;
        }

        StringBuilder line = new();
        for (int i = 0; i < points.Length; i++)
        {
            double x = xScale.Center(points[i].X);
            double y = yScale.Project(points[i].Y);
            line.Append(i == 0 ? "M " : "L ");
            line.Append(x.ToString("F2", CultureInfo.InvariantCulture));
            line.Append(' ');
            line.Append(y.ToString("F2", CultureInfo.InvariantCulture));
            line.Append(' ');
        }

        if (fill)
        {
            // Close the path back to the baseline (y=0 in scale space) so SVG fills the
            // region under the curve.
            double zeroY = yScale.Project(0);
            double lastX = xScale.Center(points[^1].X);
            double firstX = xScale.Center(points[0].X);
            line.Append("L ");
            line.Append(lastX.ToString("F2", CultureInfo.InvariantCulture));
            line.Append(' ');
            line.Append(zeroY.ToString("F2", CultureInfo.InvariantCulture));
            line.Append(" L ");
            line.Append(firstX.ToString("F2", CultureInfo.InvariantCulture));
            line.Append(' ');
            line.Append(zeroY.ToString("F2", CultureInfo.InvariantCulture));
            line.Append(" Z");

            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "class", "bob-mixed-chart__area");
            builder.AddAttribute(seq++, "d", line.ToString());
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "fill-opacity", "0.3");
            builder.AddAttribute(seq++, "stroke", color);
            builder.AddAttribute(seq++, "stroke-width", s.BorderWidth.ToString(CultureInfo.InvariantCulture));
            builder.CloseElement();
        }
        else
        {
            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "class", "bob-mixed-chart__line");
            builder.AddAttribute(seq++, "d", line.ToString());
            builder.AddAttribute(seq++, "fill", "none");
            builder.AddAttribute(seq++, "stroke", color);
            builder.AddAttribute(seq++, "stroke-width", s.BorderWidth.ToString(CultureInfo.InvariantCulture));
            builder.CloseElement();
        }
    }
}
