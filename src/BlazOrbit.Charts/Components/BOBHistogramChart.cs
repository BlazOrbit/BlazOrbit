using BlazOrbit.Charts.Abstractions;
using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Histogram — distribution of a single numeric variable. Auto-bins the
/// raw <see cref="Values"/> using <see cref="BinRule"/> and renders each
/// bin as a vertical bar. Includes a quantile readout (<c>Median</c>,
/// <c>P95</c>) optionally shown via the <see cref="ShowQuantileLines"/>
/// flag.
/// </summary>
/// <typeparam name="T">Numeric value type (int, double, decimal, …).</typeparam>
public class BOBHistogramChart<T> :
    BOBChartBase<int, int>,
    IHasAxes
    where T : struct
{
    /// <summary>Raw observations to bin and plot. Repeated values increase a bin's count.</summary>
    [Parameter]
    public IEnumerable<T>? Values { get; set; }

    /// <summary>Binning rule. Default <see cref="BOBHistogramBinRule.Sturges"/>.</summary>
    [Parameter]
    public BOBHistogramBinRule BinRule { get; set; } = BOBHistogramBinRule.Sturges;

    /// <summary>Bin count when <see cref="BinRule"/> is <see cref="BOBHistogramBinRule.FixedCount"/>.</summary>
    [Parameter]
    public int BinCount { get; set; } = 10;

    /// <summary>Optional fill color override; falls back to the active palette's first color.</summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// When <c>true</c>, draws vertical guide lines at the median and 95th
    /// percentile of <see cref="Values"/>. Default <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ShowQuantileLines { get; set; }

    /// <summary>Number-format used for bin label ranges. Default <c>"G"</c>.</summary>
    [Parameter]
    public string? BinLabelFormat { get; set; }

    /// <inheritdoc />
    [Parameter]
    public BOBChartAxis XAxis { get; set; } = new();

    /// <inheritdoc />
    [Parameter]
    public BOBChartAxis YAxis { get; set; } = new();

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int n = Values?.Count() ?? 0;
        return n == 0 ? "Histogram with no data" : $"Histogram of {n} observations";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Values is null)
        {
            return;
        }

        double[] raw = Values.Select(v => Convert.ToDouble(v, CultureInfo.InvariantCulture)).ToArray();
        if (raw.Length == 0)
        {
            return;
        }

        Binning.Rule rule = BinRule switch
        {
            BOBHistogramBinRule.Scott => Binning.Rule.Scott,
            BOBHistogramBinRule.FreedmanDiaconis => Binning.Rule.FreedmanDiaconis,
            BOBHistogramBinRule.FixedCount => Binning.Rule.FixedCount,
            _ => Binning.Rule.Sturges
        };
        Binning.Bin[] bins = Binning.Compute(raw, rule, BinCount);
        if (bins.Length == 0)
        {
            return;
        }

        ChartLayout layout = ChartLayout.Default(EffectiveWidth, EffectiveHeight);
        int maxCount = bins.Max(b => b.Count);
        LinearScale yScale = new(
            [0.0, (double)maxCount],
            layout.PlotBottom, layout.PlotTop, YAxis.Min, YAxis.Max,
            true);

        double bandWidth = layout.PlotWidth / bins.Length;
        string fill = Color ?? Palette.ColorAt(0);
        string format = BinLabelFormat ?? "0.##";

        int seq = 100;
        // Y grid + Y labels.
        if (YAxis.ShowGrid)
        {
            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-histogram-chart__grid");
            foreach (double tick in yScale.Ticks())
            {
                double y = yScale.Project(tick);
                builder.OpenElement(seq++, "line");
                builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(layout.PlotLeft));
                builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(layout.PlotRight));
                builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(y));
                builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(y));
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        if (YAxis.ShowLabels)
        {
            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-histogram-chart__axis bob-histogram-chart__axis--y");
            string yFormat = YAxis.Format ?? yScale.SuggestedFormat();
            foreach (double tick in yScale.Ticks())
            {
                double y = yScale.Project(tick);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft - 8));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y + 4));
                builder.AddAttribute(seq++, "text-anchor", "end");
                builder.AddContent(seq++, tick.ToString(yFormat, CultureInfo.InvariantCulture));
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        // Bars + X bin range labels.
        for (int i = 0; i < bins.Length; i++)
        {
            Binning.Bin bin = bins[i];
            double xLeft = layout.PlotLeft + (i * bandWidth);
            double yTop = yScale.Project(bin.Count);
            double yBottom = yScale.Project(0);
            double height = Math.Max(0, yBottom - yTop);

            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-histogram-chart__bar");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(xLeft + 1));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(yTop));
            builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(Math.Max(0, bandWidth - 2)));
            builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(height));
            builder.AddAttribute(seq++, "fill", fill);
            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++,
                string.Format(CultureInfo.InvariantCulture,
                    "[{0:" + format + "}, {1:" + format + "}) — {2}", bin.Lower, bin.Upper, bin.Count));
            builder.CloseElement();
            builder.CloseElement();
        }

        if (XAxis.ShowLabels)
        {
            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-histogram-chart__axis bob-histogram-chart__axis--x");
            // Sample 5-7 evenly-spaced edges so dense histograms don't
            // crowd the X axis with overlapping numbers.
            int sampleEvery = Math.Max(1, bins.Length / 6);
            for (int i = 0; i < bins.Length; i++)
            {
                if (i % sampleEvery != 0 && i != bins.Length - 1)
                {
                    continue;
                }

                double xCenter = layout.PlotLeft + (i * bandWidth) + (bandWidth / 2);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(xCenter));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddContent(seq++, bins[i].Lower.ToString(format, CultureInfo.InvariantCulture));
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        if (ShowQuantileLines)
        {
            double[] sorted = raw.OrderBy(v => v).ToArray();
            double median = Binning.Quantile(sorted, 0.5);
            double p95 = Binning.Quantile(sorted, 0.95);
            // Both quantile lines map across the data extent — projected to
            // pixel x using the same bin geometry: locate the bin then
            // interpolate within it.
            DrawQuantileLine(builder, ref seq, layout, bins, bandWidth, median, "Median",
                "var(--palette-primary, #2563eb)");
            DrawQuantileLine(builder, ref seq, layout, bins, bandWidth, p95, "P95", "var(--palette-error, #dc2626)");
        }
    }

    private static void DrawQuantileLine(RenderTreeBuilder builder, ref int seq,
        ChartLayout layout, Binning.Bin[] bins, double bandWidth, double value,
        string label, string color)
    {
        double min = bins[0].Lower;
        double max = bins[^1].Upper;
        if (Math.Abs(max - min) < double.Epsilon)
        {
            return;
        }

        double t = (value - min) / (max - min);
        double x = layout.PlotLeft + (t * layout.PlotWidth);
        builder.OpenElement(seq++, "line");
        builder.AddAttribute(seq++, "class", "bob-histogram-chart__quantile");
        builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(x));
        builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(x));
        builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(layout.PlotTop));
        builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(layout.PlotBottom));
        builder.AddAttribute(seq++, "stroke", color);
        builder.AddAttribute(seq++, "stroke-width", "1.5");
        builder.AddAttribute(seq++, "stroke-dasharray", "4 3");
        builder.AddAttribute(seq++, "pointer-events", "none");
        builder.CloseElement();
        builder.OpenElement(seq++, "text");
        builder.AddAttribute(seq++, "class", "bob-histogram-chart__quantile-label");
        builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x + 4));
        builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotTop + 12));
        builder.AddAttribute(seq++, "fill", color);
        builder.AddContent(seq++, label);
        builder.CloseElement();
    }
}