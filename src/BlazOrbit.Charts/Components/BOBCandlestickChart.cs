using BlazOrbit.Charts.Abstractions;
using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Candlestick / OHLC chart for financial time series. Each
/// <see cref="BOBChartCandlePoint{TX}"/> renders as a vertical wick from
/// <c>Low</c> to <c>High</c> with a body spanning <c>Open</c> to
/// <c>Close</c>. Bullish bars (<c>Close ≥ Open</c>) use
/// <see cref="UpColor"/>; bearish bars use <see cref="DownColor"/>. Set
/// <see cref="ShowVolumePane"/> to overlay a volume bar pane on the
/// bottom 20% of the plot, sharing the X axis.
/// </summary>
/// <typeparam name="TX">X-axis type (DateTime / int / etc).</typeparam>
public class BOBCandlestickChart<TX> :
    BOBChartBase<TX, double>,
    IHasAxes
    where TX : notnull
{
    /// <summary>The OHLC data, in plot order (oldest first).</summary>
    [Parameter] public IEnumerable<BOBChartCandlePoint<TX>>? Candles { get; set; }

    /// <inheritdoc />
    [Parameter] public BOBChartAxis XAxis { get; set; } = new();

    /// <inheritdoc />
    [Parameter] public BOBChartAxis YAxis { get; set; } = new();

    /// <summary>Bullish (Close ≥ Open) candle color. Default green.</summary>
    [Parameter] public string UpColor { get; set; } = "var(--palette-success, #16a34a)";

    /// <summary>Bearish (Close &lt; Open) candle color. Default red.</summary>
    [Parameter] public string DownColor { get; set; } = "var(--palette-error, #dc2626)";

    /// <summary>
    /// When <c>true</c>, the bottom 20% of the plot is reserved for a
    /// volume bar pane (one bar per candle, colored by direction). The
    /// Y axis labels apply to the price pane only. Default <c>false</c>.
    /// </summary>
    [Parameter] public bool ShowVolumePane { get; set; }

    /// <summary>
    /// Body width fraction (0..1) of the per-candle band. Default 0.7.
    /// </summary>
    [Parameter] public double BodyRatio { get; set; } = 0.7;

    /// <summary>
    /// Optional moving averages drawn on top of the candles (label,
    /// window, color). Useful for SMA / EMA overlays.
    /// </summary>
    [Parameter] public IEnumerable<BOBChartReferenceLine>? ReferenceLines { get; set; }

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int n = Candles?.Count() ?? 0;
        return n == 0 ? "Candlestick chart with no data" : $"Candlestick chart with {n} bars";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Candles is null) return;
        BOBChartCandlePoint<TX>[] candles = Candles.ToArray();
        if (candles.Length == 0) return;

        ChartLayout layout = ChartLayout.Default(EffectiveWidth, EffectiveHeight);

        // Split layout when volume pane is on: top 78% price, bottom 20% volume, 2% gap.
        double priceTop = layout.PlotTop;
        double priceBottom = ShowVolumePane
            ? layout.PlotTop + layout.PlotHeight * 0.78
            : layout.PlotBottom;
        double volTop = ShowVolumePane ? layout.PlotTop + layout.PlotHeight * 0.80 : 0;
        double volBottom = layout.PlotBottom;

        // Price scale spans observed High..Low (with margin).
        double priceMin = candles.Min(c => c.Low);
        double priceMax = candles.Max(c => c.High);
        LinearScale yPrice = new(
            new[] { priceMin, priceMax },
            priceBottom, priceTop, YAxis.Min, YAxis.Max);

        // Volume scale 0..max.
        double volMax = ShowVolumePane && candles.Any(c => c.Volume > 0)
            ? candles.Max(c => c.Volume) : 1;
        LinearScale? yVol = ShowVolumePane
            ? new LinearScale(new[] { 0.0, volMax }, volBottom, volTop, includeZero: true)
            : null;

        // X scale — categorical by index so candles render evenly spaced.
        IEnumerable<TX> xCats = candles.Select(c => c.X);
        CategoricalScale<TX> xScale = new(xCats, layout.PlotLeft, layout.PlotRight);
        double bandWidth = xScale.BandWidth;
        double bodyWidth = bandWidth * BodyRatio;

        int seq = 100;

        // Y price grid + labels.
        if (YAxis.ShowGrid)
        {
            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-candle-chart__grid");
            foreach (double tick in yPrice.Ticks())
            {
                double y = yPrice.Project(tick);
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
            builder.AddAttribute(seq++, "class", "bob-candle-chart__axis bob-candle-chart__axis--y");
            string format = YAxis.Format ?? yPrice.SuggestedFormat();
            foreach (double tick in yPrice.Ticks())
            {
                double y = yPrice.Project(tick);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft - 8));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y + 4));
                builder.AddAttribute(seq++, "text-anchor", "end");
                builder.AddContent(seq++, tick.ToString(format, CultureInfo.InvariantCulture));
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        // Candles + volume bars.
        foreach (BOBChartCandlePoint<TX> c in candles)
        {
            double cx = xScale.Center(c.X);
            bool up = c.Close >= c.Open;
            string color = up ? UpColor : DownColor;

            // Wick.
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "class", "bob-candle-chart__wick");
            builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(yPrice.Project(c.High)));
            builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(yPrice.Project(c.Low)));
            builder.AddAttribute(seq++, "stroke", color);
            builder.CloseElement();

            // Body.
            double bodyTop = yPrice.Project(Math.Max(c.Open, c.Close));
            double bodyBot = yPrice.Project(Math.Min(c.Open, c.Close));
            double bodyH = Math.Max(1, bodyBot - bodyTop);
            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", up ? "bob-candle-chart__body bob-candle-chart__body--up" : "bob-candle-chart__body bob-candle-chart__body--down");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cx - bodyWidth / 2));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(bodyTop));
            builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(bodyWidth));
            builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(bodyH));
            builder.AddAttribute(seq++, "fill", color);
            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, $"O {c.Open:G6} H {c.High:G6} L {c.Low:G6} C {c.Close:G6}");
            builder.CloseElement();
            builder.CloseElement();

            // Volume bar.
            if (yVol is not null && c.Volume > 0)
            {
                double vTop = yVol.Project(c.Volume);
                double vBot = yVol.Project(0);
                builder.OpenElement(seq++, "rect");
                builder.AddAttribute(seq++, "class", "bob-candle-chart__volume");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cx - bodyWidth / 2));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(vTop));
                builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(bodyWidth));
                builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(vBot - vTop));
                builder.AddAttribute(seq++, "fill", color);
                builder.AddAttribute(seq++, "fill-opacity", "0.45");
                builder.CloseElement();
            }
        }

        // Reference lines render after candles so threshold / SMA bands
        // stay visible on top of the price action.
        if (ReferenceLines is not null)
        {
            foreach (BOBChartReferenceLine rl in ReferenceLines)
            {
                double y = yPrice.Project(rl.Value);
                if (y < priceTop || y > priceBottom) continue;
                builder.OpenElement(seq++, "line");
                builder.AddAttribute(seq++, "class", "bob-chart__reference-line");
                builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(layout.PlotLeft));
                builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(layout.PlotRight));
                builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(y));
                builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(y));
                builder.AddAttribute(seq++, "stroke", rl.Color ?? "var(--palette-warning, #f59e0b)");
                builder.AddAttribute(seq++, "stroke-width", ChartLayout.ToInvariant(rl.StrokeWidth));
                if (rl.Style == BOBChartReferenceLineStyle.Dashed)
                    builder.AddAttribute(seq++, "stroke-dasharray", "6 4");
                else if (rl.Style == BOBChartReferenceLineStyle.Dotted)
                    builder.AddAttribute(seq++, "stroke-dasharray", "2 3");
                builder.CloseElement();
                if (!string.IsNullOrEmpty(rl.Label))
                {
                    builder.OpenElement(seq++, "text");
                    builder.AddAttribute(seq++, "class", "bob-chart__reference-label");
                    builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotRight - 6));
                    builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y - 4));
                    builder.AddAttribute(seq++, "fill", rl.Color ?? "var(--palette-warning, #f59e0b)");
                    builder.AddAttribute(seq++, "text-anchor", "end");
                    builder.AddContent(seq++, rl.Label);
                    builder.CloseElement();
                }
            }
        }

        // X axis labels — sample evenly to avoid crowding.
        if (XAxis.ShowLabels)
        {
            int sampleEvery = Math.Max(1, candles.Length / 8);
            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-candle-chart__axis bob-candle-chart__axis--x");
            for (int i = 0; i < candles.Length; i++)
            {
                if (i % sampleEvery != 0 && i != candles.Length - 1) continue;
                double cx = xScale.Center(candles[i].X);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cx));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddContent(seq++, candles[i].X switch
                {
                    DateTime dt => dt.ToString(XAxis.Format ?? "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    DateTimeOffset dto => dto.ToString(XAxis.Format ?? "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    _ => candles[i].X?.ToString() ?? string.Empty,
                });
                builder.CloseElement();
            }
            builder.CloseElement();
        }
    }
}
