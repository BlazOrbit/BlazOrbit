using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Multi-pane stock chart - extends the candlestick paradigm with stacked indicator
/// panes (RSI, MACD, Volume) and overlay indicators (Bollinger Bands) on the price
/// pane. Panes share the X axis so the user can correlate price action with momentum
/// / volume signals.
/// </summary>
/// <typeparam name="TX">X-axis type (DateTime / int / etc).</typeparam>
public sealed class BOBStockChart<TX> : BOBChartBase<TX, double>
    where TX : notnull
{
    /// <summary>OHLC + volume series rendered in the main price pane.</summary>
    [Parameter]
    public IEnumerable<BOBChartCandlePoint<TX>>? Candles { get; set; }

    /// <summary>
    /// Indicators to render below the main pane (or overlaid for Bollinger). Each
    /// indicator owns one pane in this order; Volume + RSI + MACD stack vertically.
    /// </summary>
    [Parameter]
    public IReadOnlyList<BOBChartTechnicalIndicator>? Indicators { get; set; }

    /// <summary>Bullish (Close ≥ Open) candle color. Default green.</summary>
    [Parameter]
    public string UpColor { get; set; } = "var(--palette-success, #16a34a)";

    /// <summary>Bearish (Close &lt; Open) candle color. Default red.</summary>
    [Parameter]
    public string DownColor { get; set; } = "var(--palette-error, #dc2626)";

    /// <summary>RSI period. Default 14 (the canonical Wilder choice).</summary>
    [Parameter]
    public int RsiPeriod { get; set; } = 14;

    /// <summary>MACD fast EMA period. Default 12.</summary>
    [Parameter]
    public int MacdFast { get; set; } = 12;

    /// <summary>MACD slow EMA period. Default 26.</summary>
    [Parameter]
    public int MacdSlow { get; set; } = 26;

    /// <summary>MACD signal-line smoothing period. Default 9.</summary>
    [Parameter]
    public int MacdSignal { get; set; } = 9;

    /// <summary>Bollinger band period. Default 20.</summary>
    [Parameter]
    public int BollingerPeriod { get; set; } = 20;

    /// <summary>Bollinger band standard-deviation multiplier. Default 2.</summary>
    [Parameter]
    public double BollingerStdMultiplier { get; set; } = 2;

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int n = Candles?.Count() ?? 0;
        int p = StackedIndicators().Count;
        return $"Stock chart with {n} bars and {p} indicator pane(s)";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Candles is null)
        {
            return;
        }

        BOBChartCandlePoint<TX>[] candles = Candles.ToArray();
        if (candles.Length == 0)
        {
            return;
        }

        double[] closes = candles.Select(c => c.Close).ToArray();

        ChartLayout layout = ChartLayout.Default(EffectiveWidth, EffectiveHeight);
        IReadOnlyList<BOBChartTechnicalIndicator> stacked = StackedIndicators();
        bool overlayBollinger = Indicators?.Contains(BOBChartTechnicalIndicator.BollingerBands) == true;

        // Vertical split: price pane gets 70% when ≥1 stacked pane exists, else 100%.
        // Each stacked indicator gets an equal share of the remaining 30%.
        double priceShare = stacked.Count == 0 ? 1.0 : 0.70;
        double indicatorShare = stacked.Count == 0 ? 0 : (1.0 - priceShare) / stacked.Count;
        double paneGap = 8;

        double priceTop = layout.PlotTop;
        double priceBottom = priceTop + ((layout.PlotHeight - (paneGap * stacked.Count)) * priceShare);

        // Candles & X scale (shared across panes).
        IEnumerable<TX> xCats = candles.Select(c => c.X);
        CategoricalScale<TX> xScale = new(xCats, layout.PlotLeft, layout.PlotRight);
        double bandWidth = xScale.BandWidth;
        double bodyWidth = bandWidth * 0.7;

        // Price scale - High/Low extents on price pane.
        double priceMin = candles.Min(c => c.Low);
        double priceMax = candles.Max(c => c.High);
        if (overlayBollinger)
        {
            // Expand price scale to fit Bollinger bands when they're enabled - otherwise
            // the upper / lower band can clip on volatile rallies.
            (double?[] mid, double?[] up, double?[] low) = TechnicalIndicators.BollingerBands(closes, BollingerPeriod, BollingerStdMultiplier);
            foreach (double? v in up) { if (v is double d && d > priceMax) { priceMax = d; } }
            foreach (double? v in low) { if (v is double d && d < priceMin) { priceMin = d; } }
        }

        LinearScale yPrice = new([priceMin, priceMax], priceBottom, priceTop);
        int seq = 100;

        RenderPriceAxis(builder, ref seq, layout, yPrice, priceTop, priceBottom);
        RenderCandles(builder, ref seq, candles, xScale, yPrice, bodyWidth);
        if (overlayBollinger)
        {
            RenderBollingerOverlay(builder, ref seq, candles, closes, xScale, yPrice);
        }

        // Stacked indicator panes - Volume / RSI / MACD in order of appearance.
        double cursor = priceBottom + paneGap;
        for (int i = 0; i < stacked.Count; i++)
        {
            double paneHeight = (layout.PlotHeight - (paneGap * stacked.Count)) * indicatorShare;
            double paneTop = cursor;
            double paneBottom = cursor + paneHeight;
            cursor = paneBottom + paneGap;

            switch (stacked[i])
            {
                case BOBChartTechnicalIndicator.Volume:
                    RenderVolumePane(builder, ref seq, candles, xScale, paneTop, paneBottom, bodyWidth);
                    break;
                case BOBChartTechnicalIndicator.Rsi:
                    RenderRsiPane(builder, ref seq, closes, xScale, candles, paneTop, paneBottom);
                    break;
                case BOBChartTechnicalIndicator.Macd:
                    RenderMacdPane(builder, ref seq, closes, xScale, candles, paneTop, paneBottom, bodyWidth);
                    break;
            }

            // Pane separator line so the user sees the boundary.
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "x1", Squarified.F(layout.PlotLeft));
            builder.AddAttribute(seq++, "x2", Squarified.F(layout.PlotRight));
            builder.AddAttribute(seq++, "y1", Squarified.F(paneTop - (paneGap / 2)));
            builder.AddAttribute(seq++, "y2", Squarified.F(paneTop - (paneGap / 2)));
            builder.AddAttribute(seq++, "stroke", "var(--palette-border, #ddd)");
            builder.AddAttribute(seq++, "stroke-dasharray", "2 3");
            builder.CloseElement();
        }
    }

    private IReadOnlyList<BOBChartTechnicalIndicator> StackedIndicators()
    {
        if (Indicators is null)
        {
            return [];
        }

        // Bollinger overlays on the price pane - it doesn't claim its own pane.
        return Indicators
            .Where(i => i != BOBChartTechnicalIndicator.BollingerBands)
            .Distinct()
            .ToArray();
    }

    private static void RenderPriceAxis(
        RenderTreeBuilder builder, ref int seq,
        ChartLayout layout, LinearScale yPrice, double top, double bottom)
    {
        // Y axis labels on the left edge of the price pane.
        foreach (double tick in yPrice.Ticks())
        {
            double y = yPrice.Project(tick);
            if (y < top - 0.5 || y > bottom + 0.5)
            {
                continue;
            }

            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-stock-chart__axis-label");
            builder.AddAttribute(seq++, "x", Squarified.F(layout.PlotLeft - 8));
            builder.AddAttribute(seq++, "y", Squarified.F(y + 4));
            builder.AddAttribute(seq++, "text-anchor", "end");
            builder.AddAttribute(seq++, "font-size", "11");
            builder.AddContent(seq++, tick.ToString("N2", CultureInfo.InvariantCulture));
            builder.CloseElement();
        }
    }

    private void RenderCandles(
        RenderTreeBuilder builder, ref int seq,
        BOBChartCandlePoint<TX>[] candles, CategoricalScale<TX> xScale, LinearScale yPrice, double bodyWidth)
    {
        foreach (BOBChartCandlePoint<TX> c in candles)
        {
            double cx = xScale.Center(c.X);
            bool up = c.Close >= c.Open;
            string color = up ? UpColor : DownColor;

            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "class", "bob-stock-chart__wick");
            builder.AddAttribute(seq++, "x1", Squarified.F(cx));
            builder.AddAttribute(seq++, "x2", Squarified.F(cx));
            builder.AddAttribute(seq++, "y1", Squarified.F(yPrice.Project(c.High)));
            builder.AddAttribute(seq++, "y2", Squarified.F(yPrice.Project(c.Low)));
            builder.AddAttribute(seq++, "stroke", color);
            builder.CloseElement();

            double bodyTop = yPrice.Project(Math.Max(c.Open, c.Close));
            double bodyBot = yPrice.Project(Math.Min(c.Open, c.Close));
            double bodyH = Math.Max(1, bodyBot - bodyTop);
            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-stock-chart__body");
            builder.AddAttribute(seq++, "x", Squarified.F(cx - (bodyWidth / 2)));
            builder.AddAttribute(seq++, "y", Squarified.F(bodyTop));
            builder.AddAttribute(seq++, "width", Squarified.F(bodyWidth));
            builder.AddAttribute(seq++, "height", Squarified.F(bodyH));
            builder.AddAttribute(seq++, "fill", color);
            builder.CloseElement();
        }
    }

    private void RenderBollingerOverlay(
        RenderTreeBuilder builder, ref int seq,
        BOBChartCandlePoint<TX>[] candles, double[] closes,
        CategoricalScale<TX> xScale, LinearScale yPrice)
    {
        (double?[] mid, double?[] upper, double?[] lower) = TechnicalIndicators.BollingerBands(
            closes, BollingerPeriod, BollingerStdMultiplier);

        DrawBandLine(builder, ref seq, candles, xScale, yPrice, mid, "var(--palette-primary, #4f46e5)", "bob-stock-chart__bb-mid");
        DrawBandLine(builder, ref seq, candles, xScale, yPrice, upper, "var(--palette-info, #06b6d4)", "bob-stock-chart__bb-upper");
        DrawBandLine(builder, ref seq, candles, xScale, yPrice, lower, "var(--palette-info, #06b6d4)", "bob-stock-chart__bb-lower");
    }

    private static void DrawBandLine(
        RenderTreeBuilder builder, ref int seq,
        BOBChartCandlePoint<TX>[] candles, CategoricalScale<TX> xScale, LinearScale yPrice,
        double?[] values, string color, string cssClass)
    {
        string? path = BuildPath(values, candles, xScale, yPrice);
        if (path is null)
        {
            return;
        }

        builder.OpenElement(seq++, "path");
        builder.AddAttribute(seq++, "class", cssClass);
        builder.AddAttribute(seq++, "d", path);
        builder.AddAttribute(seq++, "fill", "none");
        builder.AddAttribute(seq++, "stroke", color);
        builder.AddAttribute(seq++, "stroke-width", "1.5");
        builder.AddAttribute(seq++, "stroke-opacity", "0.8");
        builder.CloseElement();
    }

    private static void RenderVolumePane(
        RenderTreeBuilder builder, ref int seq,
        BOBChartCandlePoint<TX>[] candles, CategoricalScale<TX> xScale,
        double top, double bottom, double bodyWidth)
    {
        double maxVol = candles.Max(c => c.Volume);
        if (maxVol <= 0)
        {
            return;
        }

        LinearScale yVol = new([0.0, maxVol], bottom, top, includeZero: true);
        foreach (BOBChartCandlePoint<TX> c in candles)
        {
            double cx = xScale.Center(c.X);
            double y = yVol.Project(c.Volume);
            double h = bottom - y;
            string color = c.Close >= c.Open
                ? "var(--palette-success, #16a34a)"
                : "var(--palette-error, #dc2626)";

            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-stock-chart__volume");
            builder.AddAttribute(seq++, "x", Squarified.F(cx - (bodyWidth / 2)));
            builder.AddAttribute(seq++, "y", Squarified.F(y));
            builder.AddAttribute(seq++, "width", Squarified.F(bodyWidth));
            builder.AddAttribute(seq++, "height", Squarified.F(h));
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "opacity", "0.65");
            builder.CloseElement();
        }
    }

    private void RenderRsiPane(
        RenderTreeBuilder builder, ref int seq,
        double[] closes, CategoricalScale<TX> xScale,
        BOBChartCandlePoint<TX>[] candles, double top, double bottom)
    {
        double?[] rsi = TechnicalIndicators.Rsi(closes, RsiPeriod);
        LinearScale yRsi = new([0.0, 100.0], bottom, top, 0, 100);

        // Reference lines at 30 (oversold) and 70 (overbought).
        foreach ((double level, string color) in new[]
                 {
                     (30.0, "var(--palette-success, #16a34a)"),
                     (70.0, "var(--palette-error, #dc2626)")
                 })
        {
            double y = yRsi.Project(level);
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "x1", Squarified.F(xScale.RangeMin));
            builder.AddAttribute(seq++, "x2", Squarified.F(xScale.RangeMax));
            builder.AddAttribute(seq++, "y1", Squarified.F(y));
            builder.AddAttribute(seq++, "y2", Squarified.F(y));
            builder.AddAttribute(seq++, "stroke", color);
            builder.AddAttribute(seq++, "stroke-dasharray", "3 3");
            builder.AddAttribute(seq++, "stroke-opacity", "0.6");
            builder.CloseElement();
        }

        DrawBandLine(builder, ref seq, candles, xScale, yRsi, rsi,
            "var(--palette-primary, #4f46e5)", "bob-stock-chart__rsi");
    }

    private void RenderMacdPane(
        RenderTreeBuilder builder, ref int seq,
        double[] closes, CategoricalScale<TX> xScale,
        BOBChartCandlePoint<TX>[] candles, double top, double bottom, double bodyWidth)
    {
        (double?[] macd, double?[] signal, double?[] hist) = TechnicalIndicators.Macd(
            closes, MacdFast, MacdSlow, MacdSignal);

        // Find overall min/max for symmetric scale around 0.
        double max = 0;
        foreach (double?[] s in new[] { macd, signal, hist })
        {
            foreach (double? v in s)
            {
                if (v is double d)
                {
                    double abs = Math.Abs(d);
                    if (abs > max) { max = abs; }
                }
            }
        }

        if (max <= 0)
        {
            return;
        }

        LinearScale yMacd = new([-max, max], bottom, top);
        double zeroY = yMacd.Project(0);

        // Histogram as colored bars centred on zero.
        for (int i = 0; i < candles.Length; i++)
        {
            if (hist[i] is not double h)
            {
                continue;
            }

            double cx = xScale.Center(candles[i].X);
            double y = yMacd.Project(h);
            double y0 = Math.Min(y, zeroY);
            double height = Math.Abs(y - zeroY);
            string color = h >= 0
                ? "var(--palette-success, #16a34a)"
                : "var(--palette-error, #dc2626)";

            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-stock-chart__macd-hist");
            builder.AddAttribute(seq++, "x", Squarified.F(cx - (bodyWidth / 2)));
            builder.AddAttribute(seq++, "y", Squarified.F(y0));
            builder.AddAttribute(seq++, "width", Squarified.F(bodyWidth));
            builder.AddAttribute(seq++, "height", Squarified.F(height));
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "opacity", "0.55");
            builder.CloseElement();
        }

        // Zero baseline.
        builder.OpenElement(seq++, "line");
        builder.AddAttribute(seq++, "x1", Squarified.F(xScale.RangeMin));
        builder.AddAttribute(seq++, "x2", Squarified.F(xScale.RangeMax));
        builder.AddAttribute(seq++, "y1", Squarified.F(zeroY));
        builder.AddAttribute(seq++, "y2", Squarified.F(zeroY));
        builder.AddAttribute(seq++, "stroke", "var(--palette-border, #ddd)");
        builder.CloseElement();

        DrawBandLine(builder, ref seq, candles, xScale, yMacd, macd,
            "var(--palette-primary, #4f46e5)", "bob-stock-chart__macd-line");
        DrawBandLine(builder, ref seq, candles, xScale, yMacd, signal,
            "var(--palette-warning, #f59e0b)", "bob-stock-chart__macd-signal");
    }

    // Builds an SVG path string from a series of nullable values aligned with the
    // candle X axis. Null entries break the line - the path resumes with a fresh M on
    // the next defined point.
    private static string? BuildPath(
        double?[] values, BOBChartCandlePoint<TX>[] candles,
        CategoricalScale<TX> xScale, LinearScale yScale)
    {
        System.Text.StringBuilder sb = new();
        bool needsMove = true;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] is not double v)
            {
                needsMove = true;
                continue;
            }

            double x = xScale.Center(candles[i].X);
            double y = yScale.Project(v);
            sb.Append(needsMove ? "M " : "L ");
            sb.Append(x.ToString("F2", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append(y.ToString("F2", CultureInfo.InvariantCulture));
            sb.Append(' ');
            needsMove = false;
        }

        string s = sb.ToString();
        return string.IsNullOrEmpty(s) ? null : s;
    }
}
