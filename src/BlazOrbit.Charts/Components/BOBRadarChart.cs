using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Radar / spider chart — multi-dimensional comparison across N axes.
/// Each <see cref="BOBChartSeries{TX, TY}"/> contributes one closed
/// polygon, with one vertex per <typeparamref name="TX"/> category.
/// Best for 3-12 axes; more becomes hard to read.
/// </summary>
/// <typeparam name="TX">Categorical axis label type (typically <c>string</c>).</typeparam>
/// <typeparam name="TY">Numeric value type.</typeparam>
public class BOBRadarChart<TX, TY> : BOBChartBase<TX, TY>
    where TX : notnull
    where TY : struct
{
    /// <summary>Series; each series renders one polygon spanning all categories.</summary>
    [Parameter] public IEnumerable<BOBChartSeries<TX, TY>>? Series { get; set; }

    /// <summary>
    /// Explicit upper bound of the radial axis. When <c>null</c> (default),
    /// inferred from the maximum of all values across all series.
    /// </summary>
    [Parameter] public double? Max { get; set; }

    /// <summary>Number of concentric grid rings. Default 4.</summary>
    [Parameter] public int RingCount { get; set; } = 4;

    /// <summary>Polygon fill opacity. Default 0.25.</summary>
    [Parameter] public double FillOpacity { get; set; } = 0.25;

    /// <summary>Show small circular markers at each polygon vertex. Default <c>true</c>.</summary>
    [Parameter] public bool ShowMarkers { get; set; } = true;

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int n = Series?.Count() ?? 0;
        return n == 0 ? "Radar chart with no data" : $"Radar chart with {n} series";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Series is null) return;
        List<BOBChartSeries<TX, TY>> all = Series.ToList();
        List<BOBChartSeries<TX, TY>> seriesList = new();
        List<int> originalIndices = new();
        for (int i = 0; i < all.Count; i++)
        {
            if (!IsSeriesHidden(all[i].Label))
            {
                seriesList.Add(all[i]);
                originalIndices.Add(i);
            }
        }
        if (seriesList.Count == 0) return;

        // Categories — union across all series, declaration order from first series.
        List<TX> categories = seriesList[0].Points.Select(p => p.X).ToList();
        if (categories.Count < 3) return; // radar needs ≥3 axes to be meaningful.

        double w = EffectiveWidth;
        double h = EffectiveHeight;
        double cx = w / 2;
        double cy = h / 2;
        double radius = Math.Min(w, h) / 2 - 32;

        double maxV = Max ?? seriesList
            .SelectMany(s => s.Points.Select(p => Convert.ToDouble(p.Y, CultureInfo.InvariantCulture)))
            .DefaultIfEmpty(1).Max();
        if (maxV <= 0) maxV = 1;

        int seq = 100;
        // Concentric grid rings + axis spokes.
        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-radar-chart__grid");
        for (int r = 1; r <= RingCount; r++)
        {
            double rPx = radius * r / RingCount;
            string ring = BuildPolygon(cx, cy, rPx, categories.Count);
            builder.OpenElement(seq++, "polygon");
            builder.AddAttribute(seq++, "points", ring);
            builder.AddAttribute(seq++, "fill", "none");
            builder.AddAttribute(seq++, "stroke", "var(--palette-border, #d1d5db)");
            builder.AddAttribute(seq++, "stroke-dasharray", "2 3");
            builder.CloseElement();
        }
        for (int i = 0; i < categories.Count; i++)
        {
            double angle = AngleAt(i, categories.Count);
            double xEnd = cx + radius * Math.Cos(angle);
            double yEnd = cy + radius * Math.Sin(angle);
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(cy));
            builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(xEnd));
            builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(yEnd));
            builder.AddAttribute(seq++, "stroke", "var(--palette-border, #d1d5db)");
            builder.CloseElement();

            // Axis label outside the outermost ring.
            double labelR = radius + 14;
            double xLabel = cx + labelR * Math.Cos(angle);
            double yLabel = cy + labelR * Math.Sin(angle);
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-radar-chart__axis-label");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(xLabel));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(yLabel + 4));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddContent(seq++, categories[i]?.ToString() ?? string.Empty);
            builder.CloseElement();
        }
        builder.CloseElement();

        // Series polygons.
        for (int s = 0; s < seriesList.Count; s++)
        {
            BOBChartSeries<TX, TY> series = seriesList[s];
            int paletteIdx = s < originalIndices.Count ? originalIndices[s] : s;
            string color = series.Color ?? Palette.ColorAt(paletteIdx);

            // Map series points → vertices keyed by category order.
            Dictionary<TX, double> byX = new();
            foreach (BOBChartPoint<TX, TY> p in series.Points)
            {
                byX[p.X] = Convert.ToDouble(p.Y, CultureInfo.InvariantCulture);
            }
            System.Text.StringBuilder pts = new();
            for (int i = 0; i < categories.Count; i++)
            {
                double v = byX.TryGetValue(categories[i], out double y) ? y : 0;
                double t = Math.Clamp(v / maxV, 0, 1);
                double rPx = radius * t;
                double angle = AngleAt(i, categories.Count);
                double px = cx + rPx * Math.Cos(angle);
                double py = cy + rPx * Math.Sin(angle);
                if (i > 0) pts.Append(' ');
                pts.Append(px.ToString("F2", CultureInfo.InvariantCulture)).Append(',')
                   .Append(py.ToString("F2", CultureInfo.InvariantCulture));
            }

            builder.OpenElement(seq++, "polygon");
            builder.AddAttribute(seq++, "class", "bob-radar-chart__polygon");
            builder.AddAttribute(seq++, "data-bob-series", series.Label);
            builder.AddAttribute(seq++, "points", pts.ToString());
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "fill-opacity", ChartLayout.ToInvariant(FillOpacity));
            builder.AddAttribute(seq++, "stroke", color);
            builder.AddAttribute(seq++, "stroke-width", "2");
            builder.CloseElement();

            if (ShowMarkers)
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    double v = byX.TryGetValue(categories[i], out double y) ? y : 0;
                    double t = Math.Clamp(v / maxV, 0, 1);
                    double rPx = radius * t;
                    double angle = AngleAt(i, categories.Count);
                    double px = cx + rPx * Math.Cos(angle);
                    double py = cy + rPx * Math.Sin(angle);
                    builder.OpenElement(seq++, "circle");
                    builder.AddAttribute(seq++, "class", "bob-radar-chart__marker");
                    builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(px));
                    builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(py));
                    builder.AddAttribute(seq++, "r", "3.5");
                    builder.AddAttribute(seq++, "fill", color);
                    builder.OpenElement(seq++, "title");
                    builder.AddContent(seq++, $"{series.Label} — {categories[i]}: {v}");
                    builder.CloseElement();
                    builder.CloseElement();
                }
            }
        }
    }

    private static double AngleAt(int i, int n) => -Math.PI / 2 + i * 2 * Math.PI / n;

    private static string BuildPolygon(double cx, double cy, double r, int n)
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < n; i++)
        {
            double a = AngleAt(i, n);
            double x = cx + r * Math.Cos(a);
            double y = cy + r * Math.Sin(a);
            if (i > 0) sb.Append(' ');
            sb.Append(x.ToString("F2", CultureInfo.InvariantCulture)).Append(',')
              .Append(y.ToString("F2", CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }

    /// <inheritdoc />
    private protected override IEnumerable<LegendEntry> GetLegendEntries()
    {
        if (Series is null) yield break;
        int i = 0;
        foreach (BOBChartSeries<TX, TY> s in Series)
        {
            yield return new LegendEntry(s.Label, s.Color ?? Palette.ColorAt(i));
            i++;
        }
    }
}
