using BlazOrbit.Charts.Abstractions;
using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Vertical bar chart with multi-series support. X axis is categorical
/// (typically <see cref="string"/> labels), Y axis is a linear numeric scale.
/// </summary>
/// <typeparam name="TX">Type of the X-axis categories. Compared via
/// <see cref="object.Equals(object?)"/>; <see cref="string"/> is the typical
/// choice.</typeparam>
/// <typeparam name="TY">Numeric type of the Y values
/// (<see cref="int"/>, <see cref="long"/>, <see cref="double"/>,
/// <see cref="decimal"/>, etc.).</typeparam>
public sealed class BOBBarChart<TX, TY> :
    BOBChartBase<TX, TY>,
    IHasSeries<TX, TY>,
    IHasAxes
    where TX : notnull
{
    /// <inheritdoc />
    [Parameter] public IEnumerable<BOBChartSeries<TX, TY>>? Series { get; set; }

    /// <inheritdoc />
    [Parameter] public BOBChartAxis XAxis { get; set; } = new();

    /// <inheritdoc />
    [Parameter] public BOBChartAxis YAxis { get; set; } = new();

    /// <summary>
    /// Width of each bar group as a fraction of the categorical band
    /// (0..1). 0.8 means each group fills 80% of its band, leaving 10%
    /// padding on either side. Series within a group share that width
    /// equally.
    /// </summary>
    [Parameter] public double BarGroupRatio { get; set; } = 0.8;

    /// <summary>
    /// Horizontal reference / threshold lines drawn across the plot area
    /// (e.g. SLO targets, budget caps). Rendered over the grid but under
    /// the bars so they stay visible without blocking hover.
    /// </summary>
    [Parameter] public IEnumerable<BOBChartReferenceLine>? ReferenceLines { get; set; }

    /// <summary>
    /// Layout strategy for multi-series bars: <see cref="BOBBarStackMode.None"/>
    /// (default, side-by-side), <see cref="BOBBarStackMode.Stacked"/>
    /// (cumulative totals per category) or
    /// <see cref="BOBBarStackMode.PercentStacked"/> (each category normalised
    /// to 100%).
    /// </summary>
    [Parameter] public BOBBarStackMode StackMode { get; set; } = BOBBarStackMode.None;

    /// <summary>
    /// Fired when the user clicks a bar. The argument carries the series
    /// label, the X / Y values and the point index — useful for drill-down
    /// navigation or row-selection patterns.
    /// </summary>
    [Parameter] public EventCallback<BOBChartClickArgs<TX, TY>> OnPointClick { get; set; }

    /// <summary>
    /// Fired when the user hovers a bar (mouseenter). Raised in addition to
    /// the native SVG <c>&lt;title&gt;</c> tooltip so callers can drive
    /// secondary UI off the same signal.
    /// </summary>
    [Parameter] public EventCallback<BOBChartHoverArgs<TX, TY>> OnDataHover { get; set; }

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int seriesCount = Series?.Count() ?? 0;
        return seriesCount switch
        {
            0 => "Bar chart with no data",
            1 => "Bar chart with one series",
            _ => $"Bar chart with {seriesCount} series",
        };
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Series is null)
        {
            return;
        }

        // Materialize series + collect domain data once (input may be a query).
        // Filter out series the user toggled hidden via the legend, but
        // preserve each retained series' original index so palette colors
        // stay tied to the source dataset across legend toggles.
        List<BOBChartSeries<TX, TY>> allSeries = Series.ToList();
        List<BOBChartSeries<TX, TY>> seriesList = new(allSeries.Count);
        List<int> originalIndices = new(allSeries.Count);
        for (int i = 0; i < allSeries.Count; i++)
        {
            if (!IsSeriesHidden(allSeries[i].Label))
            {
                seriesList.Add(allSeries[i]);
                originalIndices.Add(i);
            }
        }
        if (seriesList.Count == 0)
        {
            return;
        }

        List<TX> categories = seriesList
            .SelectMany(s => s.Points.Select(p => p.X))
            .Distinct()
            .ToList();

        if (categories.Count == 0)
        {
            return;
        }

        // Y-domain depends on stack mode:
        //   None:           use raw per-point values (existing behavior).
        //   Stacked:        use sum-of-all-series-per-category as the max.
        //   PercentStacked: fix [0, 100].
        List<double> domainValues = StackMode switch
        {
            BOBBarStackMode.Stacked => CategoryTotals(seriesList, categories),
            BOBBarStackMode.PercentStacked => new List<double> { 0, 100 },
            BOBBarStackMode.Bidirectional => CategoryBidirectionalExtremes(seriesList, categories),
            BOBBarStackMode.Waterfall => WaterfallExtremes(seriesList),
            _ => seriesList
                .SelectMany(s => s.Points.Select(p => Numeric.ToDouble(p.Y)))
                .ToList(),
        };

        // SVG viewport defaults when caller did not pin Width / Height.
        double svgWidth = EffectiveWidth;
        double svgHeight = EffectiveHeight;
        ChartLayout layout = ChartLayout.Default(svgWidth, svgHeight);

        // SVG Y grows downward, so RangeMin = bottom (PlotBottom) maps to
        // DomainMin and RangeMax = top (PlotTop) maps to DomainMax.
        // includeZero: bar heights only make sense against a zero baseline.
        // For PercentStacked we honor explicit Min/Max, but otherwise force [0, 100].
        double? minOverride = StackMode == BOBBarStackMode.PercentStacked ? (YAxis.Min ?? 0) : YAxis.Min;
        double? maxOverride = StackMode == BOBBarStackMode.PercentStacked ? (YAxis.Max ?? 100) : YAxis.Max;
        LinearScale yScale = new(domainValues, layout.PlotBottom, layout.PlotTop,
            minOverride, maxOverride, includeZero: true);
        CategoricalScale<TX> xScale = new(categories, layout.PlotLeft, layout.PlotRight);

        int seq = 100;
        RenderGrid(builder, ref seq, layout, yScale);
        RenderYAxisLabels(builder, ref seq, layout, yScale);
        RenderXAxisLabels(builder, ref seq, layout, xScale);
        RenderBars(builder, ref seq, layout, xScale, yScale, seriesList, originalIndices);
        // Reference lines render last so they sit on top of the bars —
        // thresholds / SLOs are guidance, the data shouldn't occlude them.
        ReferenceLineRenderer.Render(builder, ref seq, layout, yScale, ReferenceLines);
    }

    private void RenderGrid(RenderTreeBuilder builder, ref int seq, ChartLayout layout, LinearScale yScale)
    {
        if (!YAxis.ShowGrid)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-bar-chart__grid");

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

        builder.CloseElement(); // g
    }

    private void RenderYAxisLabels(RenderTreeBuilder builder, ref int seq, ChartLayout layout, LinearScale yScale)
    {
        if (!YAxis.ShowLabels)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-bar-chart__axis bob-bar-chart__axis--y");

        // Auto-derive a sensible format from the scale's tick step when the
        // caller did not pin one — avoids 16-decimal "G" output on
        // fractional ticks (zoomed / data-driven domains).
        string format = YAxis.Format ?? yScale.SuggestedFormat();

        foreach (double tick in yScale.Ticks())
        {
            double y = yScale.Project(tick);
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft - 8));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y + 4));
            builder.AddAttribute(seq++, "text-anchor", "end");
            builder.AddContent(seq++, tick.ToString(format, CultureInfo.InvariantCulture));
            builder.CloseElement();
        }

        builder.CloseElement(); // g
    }

    private void RenderXAxisLabels(RenderTreeBuilder builder, ref int seq, ChartLayout layout, CategoricalScale<TX> xScale)
    {
        if (!XAxis.ShowLabels)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-bar-chart__axis bob-bar-chart__axis--x");

        foreach (TX category in xScale.Categories)
        {
            double x = xScale.Center(category);
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddContent(seq++, category.ToString() ?? string.Empty);
            builder.CloseElement();
        }

        builder.CloseElement(); // g
    }

    private void RenderBars(
        RenderTreeBuilder builder,
        ref int seq,
        ChartLayout layout,
        CategoricalScale<TX> xScale,
        LinearScale yScale,
        List<BOBChartSeries<TX, TY>> seriesList,
        List<int> originalIndices)
    {
        double bandWidth = xScale.BandWidth;
        double groupWidth = bandWidth * BarGroupRatio;
        double barWidth = StackMode == BOBBarStackMode.None
            ? (seriesList.Count == 0 ? 0 : groupWidth / seriesList.Count)
            : groupWidth;     // stacked modes: single full-width bar per category.
        double zero = yScale.Project(0);

        // Waterfall: only the first series matters (semantically the deltas).
        // Track a single running cumulative across categories so each bar
        // floats from prev → prev + delta.
        double waterfallRunning = 0;

        // Per-category running cumulative total (in display units — raw for
        // Stacked, percent for PercentStacked). Allocated only when needed.
        Dictionary<TX, double>? running = StackMode == BOBBarStackMode.None
            ? null
            : new Dictionary<TX, double>();

        // Bidirectional uses two running totals per category: one for
        // positive values stacking up, one for negative values stacking
        // down. The single `running` map above is unused in this mode.
        Dictionary<TX, double>? runningPos = StackMode == BOBBarStackMode.Bidirectional
            ? new Dictionary<TX, double>()
            : null;
        Dictionary<TX, double>? runningNeg = StackMode == BOBBarStackMode.Bidirectional
            ? new Dictionary<TX, double>()
            : null;

        // Per-category totals for percentage normalization.
        Dictionary<TX, double>? totals = null;
        if (StackMode == BOBBarStackMode.PercentStacked)
        {
            totals = new Dictionary<TX, double>();
            foreach (BOBChartSeries<TX, TY> s in seriesList)
            {
                foreach (BOBChartPoint<TX, TY> pt in s.Points)
                {
                    double v = Math.Max(0, Numeric.ToDouble(pt.Y));
                    totals[pt.X] = (totals.TryGetValue(pt.X, out double t) ? t : 0) + v;
                }
            }
        }

        for (int s = 0; s < seriesList.Count; s++)
        {
            BOBChartSeries<TX, TY> series = seriesList[s];
            int paletteIdx = s < originalIndices.Count ? originalIndices[s] : s;
            string color = series.Color ?? Palette.ColorAt(paletteIdx);

            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-bar-chart__series");
            builder.AddAttribute(seq++, "data-bob-series", series.Label);

            int pointIndex = 0;
            foreach (BOBChartPoint<TX, TY> pt in series.Points)
            {
                double cx = xScale.Center(pt.X);
                double rawY = Numeric.ToDouble(pt.Y);

                // Convert to display units (raw for None/Stacked, % for PercentStacked).
                double displayY = rawY;
                if (StackMode == BOBBarStackMode.PercentStacked)
                {
                    double total = totals!.TryGetValue(pt.X, out double t) ? t : 0;
                    displayY = total > 0 ? Math.Max(0, rawY) / total * 100 : 0;
                }

                double barLeft, barTop, barHeight;
                if (StackMode == BOBBarStackMode.None)
                {
                    double yProjected = yScale.Project(displayY);
                    barLeft = cx - (groupWidth / 2) + (s * barWidth);
                    barTop = Math.Min(zero, yProjected);
                    barHeight = Math.Abs(zero - yProjected);
                }
                else if (StackMode == BOBBarStackMode.Waterfall && s == 0)
                {
                    // Each bar floats from waterfallRunning → waterfallRunning + delta.
                    // Color hint: positive = green-ish, negative = red-ish, but
                    // we keep the palette color so the user's series Color
                    // override still wins.
                    barLeft = cx - (groupWidth / 2);
                    double prev = waterfallRunning;
                    double next = prev + displayY;
                    waterfallRunning = next;
                    double topPx = yScale.Project(Math.Max(prev, next));
                    double bottomPx = yScale.Project(Math.Min(prev, next));
                    barTop = topPx;
                    barHeight = Math.Max(0, bottomPx - topPx);
                }
                else if (StackMode == BOBBarStackMode.Waterfall)
                {
                    // Skip secondary series entirely in waterfall mode.
                    pointIndex++;
                    continue;
                }
                else if (StackMode == BOBBarStackMode.Bidirectional)
                {
                    // Positive values stack up from the +running anchor;
                    // negative values stack down from the -running anchor.
                    // Each side keeps its own cumulative independent of the
                    // other so a [+10, -3, +4] series resolves to two
                    // segments above zero (10 + 4) and one below (-3).
                    barLeft = cx - (groupWidth / 2);
                    if (displayY >= 0)
                    {
                        double prev = runningPos!.TryGetValue(pt.X, out double r) ? r : 0;
                        double next = prev + displayY;
                        runningPos[pt.X] = next;
                        double topPx = yScale.Project(next);
                        double bottomPx = yScale.Project(prev);
                        barTop = topPx;
                        barHeight = Math.Max(0, bottomPx - topPx);
                    }
                    else
                    {
                        double prev = runningNeg!.TryGetValue(pt.X, out double r) ? r : 0;
                        double next = prev + displayY;  // displayY < 0
                        runningNeg[pt.X] = next;
                        double topPx = yScale.Project(prev);
                        double bottomPx = yScale.Project(next);
                        barTop = topPx;
                        barHeight = Math.Max(0, bottomPx - topPx);
                    }
                }
                else
                {
                    // Stacked / PercentStacked: clamp negatives to 0.
                    double clampedY = Math.Max(0, displayY);
                    double prevRunning = running!.TryGetValue(pt.X, out double r) ? r : 0;
                    double newRunning = prevRunning + clampedY;
                    running[pt.X] = newRunning;

                    double topPx = yScale.Project(newRunning);
                    double bottomPx = yScale.Project(prevRunning);
                    barLeft = cx - (groupWidth / 2);
                    barTop = topPx;
                    barHeight = Math.Max(0, bottomPx - topPx);
                }

                // Capture loop variables for the event handler closures —
                // C# foreach captures the iteration variables by reference
                // in older runtimes; explicit copies keep us defensive and
                // readable.
                BOBChartSeries<TX, TY> capturedSeries = series;
                BOBChartPoint<TX, TY> capturedPoint = pt;
                int capturedIndex = pointIndex;

                builder.OpenElement(seq++, "rect");
                builder.AddAttribute(seq++, "class", "bob-bar-chart__bar");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(barLeft));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(barTop));
                builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(barWidth));
                builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(barHeight));
                builder.AddAttribute(seq++, "fill", color);

                if (OnPointClick.HasDelegate)
                {
                    builder.AddAttribute(seq++, "onclick",
                        EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                            this,
                            _ => OnPointClick.InvokeAsync(new BOBChartClickArgs<TX, TY>
                            {
                                SeriesLabel = capturedSeries.Label,
                                X = capturedPoint.X,
                                Y = capturedPoint.Y,
                                PointIndex = capturedIndex,
                            })));
                    builder.AddAttribute(seq++, "cursor", "pointer");
                }

                bool fireHover = OnDataHover.HasDelegate;
                bool fireTooltip = HasCustomTooltip;
                string capturedColor = color;
                double tooltipPxX = barLeft + (barWidth / 2);
                double tooltipPxY = barTop;

                if (fireHover || fireTooltip)
                {
                    builder.AddAttribute(seq++, "onmouseenter",
                        EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                            this,
                            _ =>
                            {
                                if (fireTooltip)
                                {
                                    SetActiveTooltip(tooltipPxX, tooltipPxY,
                                        new BOBChartTooltipContext<TX, TY>
                                        {
                                            SeriesLabel = capturedSeries.Label,
                                            X = capturedPoint.X,
                                            Y = capturedPoint.Y,
                                            Color = capturedColor,
                                        });
                                }
                                return fireHover
                                    ? OnDataHover.InvokeAsync(new BOBChartHoverArgs<TX, TY>
                                    {
                                        SeriesLabel = capturedSeries.Label,
                                        X = capturedPoint.X,
                                        Y = capturedPoint.Y,
                                        PointIndex = capturedIndex,
                                    })
                                    : Task.CompletedTask;
                            }));

                    if (fireTooltip)
                    {
                        builder.AddAttribute(seq++, "onmouseleave",
                            EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                                this, _ => ClearActiveTooltip()));
                    }
                }

                // Native <title> only when no custom tooltip overlay is set —
                // two tooltip systems on the same node compete for hover focus.
                if (!fireTooltip)
                {
                    builder.OpenElement(seq++, "title");
                    builder.AddContent(seq++,
                        string.Format(CultureInfo.InvariantCulture,
                            "{0}: {1} = {2}",
                            series.Label,
                            pt.X,
                            pt.Y));
                    builder.CloseElement(); // title
                }

                builder.CloseElement(); // rect
                pointIndex++;
            }

            builder.CloseElement(); // g.bob-bar-chart__series
        }
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

    /// <summary>
    /// Compute the per-category total as the sum of every series' Y at that
    /// category. Drives both the Y-domain auto-scaling for
    /// <see cref="BOBBarStackMode.Stacked"/> and the percentage normalisation
    /// for <see cref="BOBBarStackMode.PercentStacked"/>.
    /// </summary>
    private static List<double> CategoryTotals(
        List<BOBChartSeries<TX, TY>> seriesList,
        List<TX> categories)
    {
        var totals = new Dictionary<TX, double>(categories.Count);
        foreach (TX cat in categories) totals[cat] = 0;

        foreach (BOBChartSeries<TX, TY> s in seriesList)
        {
            foreach (BOBChartPoint<TX, TY> pt in s.Points)
            {
                if (totals.ContainsKey(pt.X))
                {
                    // Negative values are clamped to 0 in pure-Stacked mode —
                    // see Bidirectional for negatives stacking downward.
                    totals[pt.X] += Math.Max(0, Numeric.ToDouble(pt.Y));
                }
            }
        }

        return totals.Values.ToList();
    }

    /// <summary>
    /// Compute the waterfall extreme values: the running cumulative crosses
    /// every interim total, so the Y domain must fit the range
    /// [running min, running max] inclusive of zero.
    /// </summary>
    private static List<double> WaterfallExtremes(List<BOBChartSeries<TX, TY>> seriesList)
    {
        if (seriesList.Count == 0) return new List<double> { 0 };
        double running = 0;
        double min = 0, max = 0;
        foreach (BOBChartPoint<TX, TY> pt in seriesList[0].Points)
        {
            double v = Numeric.ToDouble(pt.Y);
            running += v;
            if (running < min) min = running;
            if (running > max) max = running;
        }
        return new List<double> { min, max, 0 };
    }

    /// <summary>
    /// Compute the bidirectional extremes per category: the sum of all
    /// positive series values (upward stack ceiling) and the sum of all
    /// negative series values (downward stack floor). Both extremes are
    /// emitted so <see cref="LinearScale"/>'s auto-domain logic fits both
    /// the largest peak and the lowest trough.
    /// </summary>
    private static List<double> CategoryBidirectionalExtremes(
        List<BOBChartSeries<TX, TY>> seriesList,
        List<TX> categories)
    {
        var posTotals = new Dictionary<TX, double>(categories.Count);
        var negTotals = new Dictionary<TX, double>(categories.Count);
        foreach (TX cat in categories)
        {
            posTotals[cat] = 0;
            negTotals[cat] = 0;
        }

        foreach (BOBChartSeries<TX, TY> s in seriesList)
        {
            foreach (BOBChartPoint<TX, TY> pt in s.Points)
            {
                if (!posTotals.ContainsKey(pt.X)) continue;
                double y = Numeric.ToDouble(pt.Y);
                if (y >= 0) posTotals[pt.X] += y;
                else negTotals[pt.X] += y;
            }
        }

        var domain = new List<double>(categories.Count * 2);
        domain.AddRange(posTotals.Values);
        domain.AddRange(negTotals.Values);
        return domain;
    }
}
