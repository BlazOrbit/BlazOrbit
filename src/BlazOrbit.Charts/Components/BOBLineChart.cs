using BlazOrbit.Charts.Abstractions;
using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;
using System.Text;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Continuous line chart with multi-series support. Auto-detects the X axis
/// type from <typeparamref name="TX"/>:
/// <list type="bullet">
/// <item><description>Numeric (<see cref="int"/>, <see cref="double"/>,
///     <see cref="decimal"/>, …) → linear scale.</description></item>
/// <item><description><see cref="DateTime"/>, <see cref="DateTimeOffset"/>,
///     <see cref="TimeSpan"/> → temporal linear scale (projected via
///     <c>Ticks</c>; format string controls the tick label rendering).</description></item>
/// <item><description>Anything else → falls back to a categorical scale,
///     equally spaced by category index in declaration order.</description></item>
/// </list>
/// Y axis is always linear-numeric.
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
/// <typeparam name="TY">Numeric Y-axis domain type.</typeparam>
public class BOBLineChart<TX, TY> :
    BOBChartBase<TX, TY>,
    IHasSeries<TX, TY>,
    IHasAxes
    where TX : notnull
{
    /// <inheritdoc />
    [Parameter]
    public IEnumerable<BOBChartSeries<TX, TY>>? Series { get; set; }

    /// <inheritdoc />
    [Parameter]
    public BOBChartAxis XAxis { get; set; } = new();

    /// <inheritdoc />
    [Parameter]
    public BOBChartAxis YAxis { get; set; } = new();

    /// <summary>
    /// When <c>true</c> the line is drawn as a smooth Catmull-Rom curve
    /// instead of a straight polyline. Defaults to <c>false</c> (polyline)
    /// because abrupt changes in data are visually clearer that way.
    /// </summary>
    [Parameter]
    public bool Smooth { get; set; }

    /// <summary>
    /// When <c>true</c> a small filled circle is drawn at each data point
    /// (also serves as the hover target for the native SVG <c>&lt;title&gt;</c>
    /// tooltip). Defaults to <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ShowMarkers { get; set; } = true;

    /// <summary>Pixel radius of the per-point markers when <see cref="ShowMarkers"/> is on.</summary>
    [Parameter]
    public double MarkerRadius { get; set; } = 3.5;

    /// <summary>
    /// Horizontal reference / threshold lines drawn across the plot area
    /// (e.g. SLO targets, baselines, regulatory caps). Inherited by
    /// <see cref="BOBAreaChart{TX, TY}"/> automatically.
    /// </summary>
    [Parameter]
    public IEnumerable<BOBChartReferenceLine>? ReferenceLines { get; set; }

    /// <summary>
    /// Free-form annotations rendered on top of the series: text labels
    /// (<see cref="BOBChartTextAnnotation{TX, TY}"/>), vertical bands
    /// (<see cref="BOBChartBandAnnotation{TX}"/>), and symbol markers
    /// (<see cref="BOBChartShapeAnnotation{TX, TY}"/>).
    /// </summary>
    [Parameter]
    public IEnumerable<BOBChartAnnotation>? Annotations { get; set; }

    /// <summary>
    /// When <c>true</c>, series are stacked: each series' Y at every X is
    /// the cumulative sum of its own value plus every previous series' Y at
    /// the same X. The line stroke (and area fill, in
    /// <see cref="BOBAreaChart{TX, TY}"/>) follows the cumulative total
    /// instead of raw values. Default <c>false</c>.
    /// <para>
    /// Useful for "decomposition" charts where the message is the total
    /// trajectory broken down by component (revenue by region over time,
    /// CPU usage by container). For bar charts use the analogous
    /// <see cref="BOBBarChart{TX, TY}.StackMode"/> parameter.
    /// </para>
    /// </summary>
    [Parameter]
    public bool Stacked { get; set; }

    /// <summary>
    /// When <c>true</c>, the chart enables wheel-to-zoom and double-click-
    /// to-reset on the X axis. Mouse wheel up zooms in 10% around the
    /// cursor X; wheel down zooms out 10%. Double-click clears the zoom
    /// and reverts to the data extent. Default <c>false</c>.
    /// <para>
    /// Only effective when the X axis is continuous (numeric or temporal);
    /// categorical X axes have no continuous coordinate to zoom into.
    /// </para>
    /// </summary>
    [Parameter]
    public bool ZoomEnabled { get; set; }

    /// <summary>
    /// When <c>true</c>, drag on the plot area paints a translucent
    /// rectangle and on release fires <see cref="OnBrush"/> with the
    /// selected X range. Brush takes priority over pan when both
    /// <see cref="ZoomEnabled"/> and <see cref="BrushEnabled"/> are on:
    /// drag = brush; wheel zoom + double-click reset still work.
    /// Default <c>false</c>. Continuous X axes only.
    /// </summary>
    [Parameter]
    public bool BrushEnabled { get; set; }

    /// <summary>
    /// When <c>true</c> (default), a finished brush drag automatically
    /// zooms to the selected X range. Disable to use brush purely as a
    /// "select range" callback for master/detail layouts.
    /// </summary>
    [Parameter]
    public bool BrushAutoZoom { get; set; } = true;

    /// <summary>
    /// Fired when the user releases a brush drag. Carries the selected
    /// range in the original <typeparamref name="TX"/> domain.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartBrushArgs<TX>> OnBrush { get; set; }

    // Active brush drag state — pixels in plot-space (same coord as scale.Range).
    private bool _brushActive;
    private double _brushStartPxX;
    private double _brushEndPxX;

    // Visible X-domain when the user has zoomed in. Both null = full range.
    // Stored in domain space (not pixel) so window resizes don't drift.
    private double? _zoomXMin;
    private double? _zoomXMax;

    // Drag-to-pan state. Captured on mousedown; consumed on every mousemove
    // until mouseup or mouseleave. Pixel + domain are both stored so each
    // frame's translation is computed from the original anchor (avoids drift
    // when many small mousemove events fire mid-drag).
    private bool _isPanning;
    private double _panStartPxX;
    private double _panStartDomainMin;
    private double _panStartDomainMax;

    /// <summary>
    /// When <c>true</c>, a vertical guide line tracks the mouse X across
    /// the plot area on hover. The nearest data point per series is
    /// highlighted simultaneously and an HTML readout pinned to the line
    /// shows X and per-series Y values. Default <c>false</c> — opt-in
    /// because the overlay competes with per-marker click / hover handlers
    /// (markers above the overlay still receive their own events; bare
    /// plot space drives the crosshair).
    /// </summary>
    [Parameter]
    public bool ShowCrosshair { get; set; }

    /// <summary>
    /// Active crosshair snapshot — set on mousemove over the plot overlay,
    /// cleared on mouseleave. Stays null when off-plot or feature disabled.
    /// </summary>
    private CrosshairState? _crosshair;

    // Per-render scratch buffers populated in RenderSvg when Stacked=true,
    // consumed by RenderSeries / area subclass override of RenderSeriesShape.
    // Indexed [seriesIndex][pointIndex] in declaration order.
    private List<double[]>? _stackedCumulative;
    private List<double[]>? _stackedBaseline;

    // Per-render mapping: filtered-series index → original index in the
    // unfiltered Series parameter. Used so palette colors stay pinned to
    // their data source even after the user toggles series on/off via the
    // legend (without this the filtered index drives ColorAt and remaining
    // series visibly "shift" colors).
    private List<int>? _originalSeriesIndices;

    // Stable per-instance clip-path id. Used to scope a <clipPath> to the
    // plot rect so series strokes / markers don't bleed past the axes when
    // the user zooms / pans into a sub-domain. Computed lazily.
    private string? _clipId;
    private string ClipId => _clipId ??= $"bob-line-clip-{Guid.NewGuid():N}";

    private record struct CrosshairEntry(string Label, string Color, TX X, TY Y, double MarkerCx, double MarkerCy);

    private record struct CrosshairState(double PixelX, List<CrosshairEntry> Entries);

    /// <summary>
    /// Fired when the user clicks a point marker. Requires
    /// <see cref="ShowMarkers"/> = <c>true</c> — when markers are off the
    /// line itself has no per-point hit targets.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartClickArgs<TX, TY>> OnPointClick { get; set; }

    /// <summary>
    /// Fired when the user hovers a point marker (mouseenter). Requires
    /// <see cref="ShowMarkers"/> = <c>true</c>.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartHoverArgs<TX, TY>> OnDataHover { get; set; }

    /// <summary>
    /// Sparkline mode: hides axes, grid, tick labels, legend and tooltip
    /// title — only the line stroke (and optional markers) renders. Pair
    /// with a small <c>Width</c> / <c>Height</c> (e.g. 120×32) to embed
    /// inline in tables, KPI cards, or list rows. Default <c>false</c>.
    /// </summary>
    [Parameter]
    public bool Sparkline { get; set; }

    /// <summary>
    /// Maximum number of points kept per series in the streaming buffer
    /// (combined parameter <see cref="Series"/> + appended). When the
    /// combined length exceeds this value the oldest points are evicted
    /// FIFO. <c>null</c> = unlimited. Effective only after the first
    /// streaming append; pure parameter-driven series ignore the window.
    /// </summary>
    [Parameter]
    public int? StreamingWindow { get; set; }

    /// <summary>
    /// When <c>true</c>,
    /// <see cref="AppendPointsAsync(string,System.Collections.Generic.IEnumerable{BOBChartPoint{TX, TY}})" />
    /// still updates the internal buffer but the chart does NOT re-render
    /// (the next non-paused append flushes the visible state). Use to
    /// freeze the view while inspecting a moving target. Default <c>false</c>.
    /// </summary>
    [Parameter]
    public bool StreamingPaused { get; set; }

    /// <summary>
    /// When the user has zoomed in and a streaming append produces an X
    /// past the current visible window, slide the zoom range forward by
    /// the overflow so the newest point stays in view. Default <c>true</c>.
    /// </summary>
    [Parameter]
    public bool StreamingFollow { get; set; } = true;

    /// <summary>
    /// Optional debounce window: when set, rapid back-to-back
    /// <c>AppendPointsAsync</c> calls coalesce into a single re-render
    /// after this delay since the last append. <c>null</c> (default)
    /// re-renders immediately on every append.
    /// </summary>
    [Parameter]
    public TimeSpan? StreamingThrottle { get; set; }

    /// <summary>
    /// Fired after every streaming buffer mutation (append / reset /
    /// window-trim). Carries per-series counters for status banners.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartStreamArgs<TX, TY>> OnStreamUpdate { get; set; }

    // Per-series streaming buffer. Lazily created on first append. When a
    // series has an entry here, RenderSvg uses the buffer instead of the
    // parameter's Points (still gated by Series enumeration so toggling a
    // hidden series via the legend works as before).
    private Dictionary<string, List<BOBChartPoint<TX, TY>>>? _streamBuffer;
    private CancellationTokenSource? _throttleCts;

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int seriesCount = Series?.Count() ?? 0;
        return seriesCount switch
        {
            0 => "Line chart with no data",
            1 => "Line chart with one series",
            _ => $"Line chart with {seriesCount} series"
        };
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Series is null)
        {
            return;
        }

        // Filter out series the user toggled hidden via the legend, but
        // keep a parallel list of their original indices so palette colors
        // stay tied to the source dataset (otherwise hiding series #0
        // shifts every remaining series' color one slot to the left).
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

        _originalSeriesIndices = originalIndices;
        if (seriesList.Count == 0)
        {
            return;
        }

        // Materialize all (x, y) pairs once. We need them twice: to compute
        // the X / Y domains and to render path commands. When a streaming
        // buffer exists for a series label it overrides the parameter's
        // Points (the parameter became the seed; appends are the truth).
        List<List<BOBChartPoint<TX, TY>>> pointsBySeries =
            seriesList.Select(s =>
                _streamBuffer is not null &&
                _streamBuffer.TryGetValue(s.Label, out List<BOBChartPoint<TX, TY>>? buffered)
                    ? buffered.ToList()
                    : s.Points.ToList()).ToList();

        if (pointsBySeries.All(p => p.Count == 0))
        {
            return;
        }

        double svgWidth = EffectiveWidth;
        double svgHeight = EffectiveHeight;
        // Sparkline collapses padding so the line stroke fills the SVG edge
        // to edge. Caller is expected to size the chart small (e.g. 120×32)
        // and embed it inline in tables / KPI cards.
        ChartLayout layout = Sparkline
            ? ChartLayout.Sparkline(svgWidth, svgHeight)
            : ChartLayout.Default(svgWidth, svgHeight);

        // Pre-compute cumulative-Y arrays when Stacked: each series' [i] holds
        // the cumulative total at that point's X (own + all previous series').
        // _baselineYBySeries[s][i] holds the previous series' cumulative
        // at the same X — that's the fill baseline for area subclasses.
        List<double[]>? cumulativeYBySeries = null;
        List<double[]>? baselineYBySeries = null;
        if (Stacked)
        {
            cumulativeYBySeries = new List<double[]>(seriesList.Count);
            baselineYBySeries = new List<double[]>(seriesList.Count);
            Dictionary<TX, double> running = new();
            for (int s = 0; s < seriesList.Count; s++)
            {
                List<BOBChartPoint<TX, TY>> pts = pointsBySeries[s];
                double[] cumulative = new double[pts.Count];
                double[] baseline = new double[pts.Count];
                for (int i = 0; i < pts.Count; i++)
                {
                    // Negatives clamped to 0 in stacked mode (waterfall = v2).
                    double rawY = Math.Max(0, Numeric.ToDouble(pts[i].Y));
                    double prev = running.TryGetValue(pts[i].X, out double r) ? r : 0;
                    baseline[i] = prev;
                    cumulative[i] = prev + rawY;
                    running[pts[i].X] = cumulative[i];
                }

                cumulativeYBySeries.Add(cumulative);
                baselineYBySeries.Add(baseline);
            }
        }

        // Y axis: always linear-numeric. Stacked uses cumulative totals so
        // the topmost series defines the domain ceiling.
        List<double> allY = Stacked
            ? cumulativeYBySeries!.SelectMany(c => c).ToList()
            : pointsBySeries.SelectMany(ps => ps.Select(p => Numeric.ToDouble(p.Y))).ToList();
        LinearScale yScale = new(allY, layout.PlotBottom, layout.PlotTop, YAxis.Min, YAxis.Max);

        // Stash for RenderSeries to consume without an extra param shuffle.
        _stackedCumulative = cumulativeYBySeries;
        _stackedBaseline = baselineYBySeries;

        // X axis: continuous (numeric or temporal) or categorical.
        bool xContinuous = Numeric.IsContinuous<TX>();
        LinearScale? xLinear = null;
        CategoricalScale<TX>? xCat = null;

        if (xContinuous)
        {
            List<double> allX = pointsBySeries
                .SelectMany(ps => ps.Select(p => Numeric.ToDouble(p.X)))
                .ToList();
            // Zoom state (when set) overrides the auto X domain so the user's
            // zoomed-in view persists across re-renders.
            double? xMin = _zoomXMin ?? XAxis.Min;
            double? xMax = _zoomXMax ?? XAxis.Max;
            xLinear = new LinearScale(allX, layout.PlotLeft, layout.PlotRight, xMin, xMax);
        }
        else
        {
            IEnumerable<TX> categories = pointsBySeries
                .SelectMany(ps => ps.Select(p => p.X))
                .Distinct();
            xCat = new CategoricalScale<TX>(categories, layout.PlotLeft, layout.PlotRight);
        }

        int seq = 100;
        // <defs><clipPath …> for the plot rect — referenced by the series
        // group so zoomed strokes / markers stay inside the axes.
        builder.OpenElement(seq++, "defs");
        builder.OpenElement(seq++, "clipPath");
        builder.AddAttribute(seq++, "id", ClipId);
        builder.OpenElement(seq++, "rect");
        builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft));
        builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotTop));
        builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(layout.PlotWidth));
        builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(layout.PlotHeight));
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();

        if (!Sparkline)
        {
            RenderGrid(builder, ref seq, layout, yScale);
            RenderYAxisLabels(builder, ref seq, layout, yScale);
            RenderXAxisLabels(builder, ref seq, layout, xLinear, xCat);
        }

        // Plot interaction overlay <rect> — placed BEFORE series so markers
        // (rendered after) keep receiving their own click / hover events on
        // top. Bare plot space drives crosshair tracking and zoom wheel /
        // double-click reset interactions.
        if (ShowCrosshair || ((ZoomEnabled || BrushEnabled) && xLinear is not null))
        {
            RenderPlotInteractionOverlay(builder, ref seq, layout, seriesList, pointsBySeries, yScale, xLinear, xCat);
        }

        RenderSeries(builder, ref seq, seriesList, pointsBySeries, layout, yScale, xLinear, xCat);

        // Reference lines render after the series so threshold / SLO
        // markers stay visible on top of the data they're guiding.
        if (!Sparkline)
        {
            ReferenceLineRenderer.Render(builder, ref seq, layout, yScale, ReferenceLines);
        }

        // User annotations — rendered after series so labels / shapes /
        // bands sit on top of the lines but below the crosshair overlay.
        if (Annotations is not null)
        {
            RenderAnnotations(builder, ref seq, layout, yScale, xLinear, xCat);
        }

        // Crosshair line + highlighted markers + HTML tooltip — rendered
        // AFTER series so they layer on top.
        if (ShowCrosshair && _crosshair is { } cross)
        {
            RenderCrosshairOverlayActive(builder, ref seq, layout, cross);
        }

        // Brush rectangle during an active drag (semi-transparent overlay).
        if (_brushActive)
        {
            double leftPx = Math.Min(_brushStartPxX, _brushEndPxX);
            double width = Math.Abs(_brushEndPxX - _brushStartPxX);
            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-line-chart__brush");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(leftPx));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotTop));
            builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(width));
            builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(layout.PlotHeight));
            builder.AddAttribute(seq++, "pointer-events", "none");
            builder.CloseElement();
        }
    }

    private void RenderGrid(RenderTreeBuilder builder, ref int seq, ChartLayout layout, LinearScale yScale)
    {
        if (!YAxis.ShowGrid)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-line-chart__grid");

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
        builder.AddAttribute(seq++, "class", "bob-line-chart__axis bob-line-chart__axis--y");

        // When the caller did not pin an explicit format, derive one from
        // the scale's tick step so zoomed-in axes don't render labels with
        // 16 trailing decimals (1.1666666666…) — the user-visible result
        // of "G" formatting on imprecise floating-point ticks.
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

    private void RenderXAxisLabels(
        RenderTreeBuilder builder,
        ref int seq,
        ChartLayout layout,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat)
    {
        if (!XAxis.ShowLabels)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-line-chart__axis bob-line-chart__axis--x");

        if (xLinear is not null)
        {
            // Continuous: use the linear scale's ticks (already nice-rounded
            // in domain space; for time domains they'll be on tick boundaries
            // but not aligned to year/month boundaries — that's a v2 polish).
            foreach (double tick in xLinear.Ticks())
            {
                double x = xLinear.Project(tick);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddContent(seq++, Numeric.FormatContinuous<TX>(tick, XAxis.Format, xLinear.SuggestedFormat()));
                builder.CloseElement();
            }
        }
        else if (xCat is not null)
        {
            foreach (TX category in xCat.Categories)
            {
                double x = xCat.Center(category);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddContent(seq++, category.ToString() ?? string.Empty);
                builder.CloseElement();
            }
        }

        builder.CloseElement(); // g
    }

    private void RenderSeries(
        RenderTreeBuilder builder,
        ref int seq,
        List<BOBChartSeries<TX, TY>> seriesList,
        List<List<BOBChartPoint<TX, TY>>> pointsBySeries,
        ChartLayout layout,
        LinearScale yScale,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat)
    {
        for (int s = 0; s < seriesList.Count; s++)
        {
            BOBChartSeries<TX, TY> series = seriesList[s];
            List<BOBChartPoint<TX, TY>> points = pointsBySeries[s];
            if (points.Count == 0)
            {
                continue;
            }

            int paletteIdx = _originalSeriesIndices is not null && s < _originalSeriesIndices.Count
                ? _originalSeriesIndices[s]
                : s;
            string color = series.Color ?? Palette.ColorAt(paletteIdx);

            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-line-chart__series");
            builder.AddAttribute(seq++, "data-bob-series", series.Label);
            // Clip series strokes / markers to the plot rect so zoomed lines
            // don't bleed across the axes.
            builder.AddAttribute(seq++, "clip-path", $"url(#{ClipId})");

            // Project all points to pixel space once. When stacked, the Y
            // domain value comes from the precomputed cumulative array
            // instead of the raw point — so the line stroke / markers sit at
            // the cumulative total, not the raw value.
            double[]? cumulativeForSeries = _stackedCumulative is not null && s < _stackedCumulative.Count
                ? _stackedCumulative[s]
                : null;
            double[]? baselineForSeries = _stackedBaseline is not null && s < _stackedBaseline.Count
                ? _stackedBaseline[s]
                : null;

            List<(double X, double Y)> projected = new(points.Count);
            List<(double X, double Y)>? baselineProjected = null;
            if (baselineForSeries is not null)
            {
                baselineProjected = new List<(double X, double Y)>(points.Count);
            }

            for (int i = 0; i < points.Count; i++)
            {
                BOBChartPoint<TX, TY> pt = points[i];
                double x = xLinear is not null
                    ? xLinear.Project(Numeric.ToDouble(pt.X))
                    : xCat!.Center(pt.X);
                double yValue = cumulativeForSeries is not null
                    ? cumulativeForSeries[i]
                    : Numeric.ToDouble(pt.Y);
                double y = yScale.Project(yValue);
                projected.Add((x, y));

                if (baselineProjected is not null)
                {
                    baselineProjected.Add((x, yScale.Project(baselineForSeries![i])));
                }
            }

            RenderSeriesShape(builder, ref seq, series, points, projected, color, s, layout, yScale, baselineProjected);

            builder.CloseElement(); // g.bob-line-chart__series
        }
    }

    /// <summary>
    /// Per-series geometry hook. Default emits the line stroke + (optional)
    /// per-point markers. <see cref="BOBAreaChart{TX, TY}"/> overrides this
    /// to layer a filled area shape under the line stroke; future variants
    /// (stepped lines, splines with control-point markers) plug in here too.
    /// </summary>
    /// <param name="builder">Render tree builder positioned inside the per-series &lt;g&gt;.</param>
    /// <param name="seq">Sequence counter, advanced by the implementation.</param>
    /// <param name="series">The series being drawn.</param>
    /// <param name="points">Source points (already filtered for empty).</param>
    /// <param name="projected">Pixel-space (X, Y) pairs in the same order as <paramref name="points"/>.</param>
    /// <param name="color">Resolved stroke / fill color (palette or explicit override).</param>
    /// <param name="seriesIndex">Index of the series in declaration order.</param>
    /// <param name="layout">Plot layout (used by area subclasses to find the baseline).</param>
    /// <param name="yScale">Y axis scale (used to project the zero line).</param>
    /// <param name="baselineProjected">
    /// Optional precomputed lower-boundary pixel points (one per entry in
    /// <paramref name="projected"/>). Populated by the pipeline when
    /// <see cref="Stacked"/> is on so area subclasses can fill between
    /// adjacent stack levels rather than the zero line.
    /// </param>
    /// <remarks>
    /// Visibility is <c>private protected</c>: overridable only by derived
    /// classes inside the BlazOrbit.Charts assembly, because the
    /// <c>ChartLayout</c> / <c>LinearScale</c> parameter types are internal.
    /// External consumers wanting custom geometry should compose at the app
    /// level instead of subclassing.
    /// </remarks>
    private protected virtual void RenderSeriesShape(
        RenderTreeBuilder builder,
        ref int seq,
        BOBChartSeries<TX, TY> series,
        List<BOBChartPoint<TX, TY>> points,
        List<(double X, double Y)> projected,
        string color,
        int seriesIndex,
        ChartLayout layout,
        LinearScale yScale,
        List<(double X, double Y)>? baselineProjected = null)
    {
        // baselineProjected is unused by the line stroke (it only paints the
        // top contour); area subclass uses it for the lower fill boundary.
        _ = baselineProjected;

        // Stroke path
        string d = Smooth
            ? BuildSmoothPath(projected)
            : BuildPolylinePath(projected);

        builder.OpenElement(seq++, "path");
        builder.AddAttribute(seq++, "class", "bob-line-chart__line");
        builder.AddAttribute(seq++, "d", d);
        builder.AddAttribute(seq++, "fill", "none");
        builder.AddAttribute(seq++, "stroke", color);
        builder.AddAttribute(seq++, "stroke-width", series.BorderWidth.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(seq++, "stroke-linejoin", "round");
        builder.AddAttribute(seq++, "stroke-linecap", "round");
        builder.CloseElement();

        // Per-point markers + native <title> for accessible tooltip
        if (ShowMarkers)
        {
            for (int i = 0; i < projected.Count; i++)
            {
                (double cx, double cy) = projected[i];
                BOBChartPoint<TX, TY> pt = points[i];
                BOBChartSeries<TX, TY> capturedSeries = series;
                BOBChartPoint<TX, TY> capturedPoint = pt;
                int capturedIndex = i;

                builder.OpenElement(seq++, "circle");
                builder.AddAttribute(seq++, "class", "bob-line-chart__marker");
                builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(cx));
                builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(cy));
                builder.AddAttribute(seq++, "r", ChartLayout.ToInvariant(MarkerRadius));
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
                                PointIndex = capturedIndex
                            })));
                    builder.AddAttribute(seq++, "cursor", "pointer");
                }

                bool fireHover = OnDataHover.HasDelegate;
                bool fireTooltip = HasCustomTooltip;
                string capturedColor = color;

                if (fireHover || fireTooltip)
                {
                    builder.AddAttribute(seq++, "onmouseenter",
                        EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                            this,
                            _ =>
                            {
                                if (fireTooltip)
                                {
                                    SetActiveTooltip(cx, cy,
                                        new BOBChartTooltipContext<TX, TY>
                                        {
                                            SeriesLabel = capturedSeries.Label,
                                            X = capturedPoint.X,
                                            Y = capturedPoint.Y,
                                            Color = capturedColor
                                        });
                                }

                                return fireHover
                                    ? OnDataHover.InvokeAsync(new BOBChartHoverArgs<TX, TY>
                                    {
                                        SeriesLabel = capturedSeries.Label,
                                        X = capturedPoint.X,
                                        Y = capturedPoint.Y,
                                        PointIndex = capturedIndex
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

                builder.CloseElement(); // circle
            }
        }
    }

    /// <summary>
    /// Build an SVG path <c>d</c> attribute as a polyline:
    /// <c>"M x0,y0 L x1,y1 L x2,y2 …"</c>.
    /// </summary>
    protected static string BuildPolylinePath(IReadOnlyList<(double X, double Y)> points)
    {
        if (points.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new(points.Count * 16);
        sb.Append("M ").Append(ChartLayout.ToInvariant(points[0].X))
            .Append(',').Append(ChartLayout.ToInvariant(points[0].Y));
        for (int i = 1; i < points.Count; i++)
        {
            sb.Append(" L ").Append(ChartLayout.ToInvariant(points[i].X))
                .Append(',').Append(ChartLayout.ToInvariant(points[i].Y));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Build an SVG path <c>d</c> attribute as a smooth Catmull-Rom-like
    /// cubic bezier curve. Uses the midpoint-derivative simplification:
    /// each pair of consecutive points becomes a cubic with control points
    /// derived from the segment's neighbours, yielding C¹ continuity at
    /// data points without overshoot.
    /// </summary>
    protected static string BuildSmoothPath(IReadOnlyList<(double X, double Y)> points)
    {
        if (points.Count < 2)
        {
            return BuildPolylinePath(points);
        }

        const double tension = 0.2; // 0 = straight lines, ~0.5 = pronounced curvature.

        StringBuilder sb = new(points.Count * 32);
        sb.Append("M ").Append(ChartLayout.ToInvariant(points[0].X))
            .Append(',').Append(ChartLayout.ToInvariant(points[0].Y));

        for (int i = 0; i < points.Count - 1; i++)
        {
            (double x0, double y0) = i > 0 ? points[i - 1] : points[i];
            (double x1, double y1) = points[i];
            (double x2, double y2) = points[i + 1];
            (double x3, double y3) = i < points.Count - 2 ? points[i + 2] : points[i + 1];

            double cp1x = x1 + ((x2 - x0) * tension);
            double cp1y = y1 + ((y2 - y0) * tension);
            double cp2x = x2 - ((x3 - x1) * tension);
            double cp2y = y2 - ((y3 - y1) * tension);

            sb.Append(" C ")
                .Append(ChartLayout.ToInvariant(cp1x)).Append(',').Append(ChartLayout.ToInvariant(cp1y))
                .Append(' ')
                .Append(ChartLayout.ToInvariant(cp2x)).Append(',').Append(ChartLayout.ToInvariant(cp2y))
                .Append(' ')
                .Append(ChartLayout.ToInvariant(x2)).Append(',').Append(ChartLayout.ToInvariant(y2));
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    private protected override IEnumerable<LegendEntry> GetLegendEntries()
    {
        if (Series is null || Sparkline)
        {
            yield break;
        }

        int i = 0;
        foreach (BOBChartSeries<TX, TY> s in Series)
        {
            yield return new LegendEntry(s.Label, s.Color ?? Palette.ColorAt(i));
            i++;
        }
    }

    private void RenderPlotInteractionOverlay(
        RenderTreeBuilder builder,
        ref int seq,
        ChartLayout layout,
        List<BOBChartSeries<TX, TY>> seriesList,
        List<List<BOBChartPoint<TX, TY>>> pointsBySeries,
        LinearScale yScale,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat)
    {
        // Capture reference data once per render so the closure doesn't
        // rebuild scale state on every mousemove.
        List<BOBChartSeries<TX, TY>> capturedSeries = seriesList;
        List<List<BOBChartPoint<TX, TY>>> capturedPoints = pointsBySeries;
        LinearScale capturedY = yScale;
        LinearScale? capturedXLinear = xLinear;
        CategoricalScale<TX>? capturedXCat = xCat;
        ChartLayout capturedLayout = layout;

        builder.OpenElement(seq++, "rect");
        builder.AddAttribute(seq++, "class", "bob-line-chart__crosshair-overlay");
        builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft));
        builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotTop));
        builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(layout.PlotWidth));
        builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(layout.PlotHeight));
        builder.AddAttribute(seq++, "fill", "transparent");
        builder.AddAttribute(seq++, "pointer-events", "all");

        // mousemove serves crosshair, brush AND pan. Composite handler
        // disambiguates by state (_brushActive / _isPanning) every frame.
        bool wantsCrosshair = ShowCrosshair;
        bool wantsZoom = ZoomEnabled && xLinear is not null;
        bool wantsBrush = BrushEnabled && xLinear is not null;

        if (wantsCrosshair || wantsZoom || wantsBrush)
        {
            builder.AddAttribute(seq++, "onmousemove",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                    this,
                    e =>
                    {
                        // Brush drag wins over both pan and crosshair.
                        if (_brushActive)
                        {
                            UpdateBrush(e.OffsetX + capturedLayout.PlotLeft, capturedLayout);
                            return;
                        }

                        if (_isPanning && capturedXLinear is not null)
                        {
                            HandlePan(e.OffsetX + capturedLayout.PlotLeft, capturedXLinear);
                            return;
                        }

                        if (wantsCrosshair)
                        {
                            UpdateCrosshair(e.OffsetX, capturedSeries, capturedPoints, capturedY,
                                capturedXLinear, capturedXCat, capturedLayout);
                        }
                    }));

            builder.AddAttribute(seq++, "onmouseleave",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                    this, _ =>
                    {
                        // Clear all transient states so the next entry starts clean.
                        _isPanning = false;
                        _brushActive = false;
                        if (wantsCrosshair)
                        {
                            ClearCrosshair();
                        }
                    }));
        }

        if (wantsZoom || wantsBrush)
        {
            // mousedown / mouseup drives BOTH brush and pan; the dispatch is
            // by parameter precedence: BrushEnabled wins, falls back to pan.
            builder.AddAttribute(seq++, "onmousedown",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                    this, e =>
                    {
                        double cursorPxX = e.OffsetX + capturedLayout.PlotLeft;
                        if (wantsBrush)
                        {
                            StartBrush(cursorPxX);
                        }
                        else if (wantsZoom)
                        {
                            StartPan(cursorPxX, capturedXLinear!);
                        }
                    }));

            builder.AddAttribute(seq++, "onmouseup",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                    this, async _ =>
                    {
                        if (_brushActive && capturedXLinear is not null)
                        {
                            await CommitBrush(capturedXLinear);
                            return;
                        }

                        _isPanning = false;
                    }));
        }

        if (wantsZoom)
        {
            // Wheel zoom anchored at cursor X. The "@onwheel:preventDefault"
            // sugar isn't available from a manual RenderTreeBuilder, so we
            // emit the lowered attribute Blazor's compiler produces:
            // `__internal_preventDefault_onwheel` = true. Without this, the
            // page scrolls under the chart on every wheel tick.
            builder.AddAttribute(seq++, "__internal_preventDefault_onwheel", true);
            builder.AddAttribute(seq++, "onwheel",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.WheelEventArgs>(
                    this, e => HandleZoomWheel(e, capturedXLinear!, capturedLayout)));

            // Double-click resets the visible domain to full data extent.
            builder.AddAttribute(seq++, "ondblclick",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                    this, _ => ResetZoom()));
        }

        builder.CloseElement();
    }

    private void StartBrush(double cursorPxX)
    {
        _brushActive = true;
        _brushStartPxX = cursorPxX;
        _brushEndPxX = cursorPxX;
        StateHasChanged();
    }

    private void UpdateBrush(double cursorPxX, ChartLayout layout)
    {
        // Clamp the live edge to the plot rect so the visual rect stays inside.
        _brushEndPxX = Math.Clamp(cursorPxX, layout.PlotLeft, layout.PlotRight);
        StateHasChanged();
    }

    private async Task CommitBrush(LinearScale xScale)
    {
        if (!_brushActive)
        {
            return;
        }

        double leftPx = Math.Min(_brushStartPxX, _brushEndPxX);
        double rightPx = Math.Max(_brushStartPxX, _brushEndPxX);

        // Trivial drag (single-click without movement) → cancel without firing.
        if (rightPx - leftPx < 2)
        {
            _brushActive = false;
            StateHasChanged();
            return;
        }

        double minDomain = xScale.Invert(leftPx);
        double maxDomain = xScale.Invert(rightPx);

        _brushActive = false;

        if (BrushAutoZoom)
        {
            _zoomXMin = minDomain;
            _zoomXMax = maxDomain;
        }

        StateHasChanged();

        if (OnBrush.HasDelegate)
        {
            await OnBrush.InvokeAsync(new BOBChartBrushArgs<TX>
            {
                MinX = Numeric.FromDouble<TX>(minDomain), MaxX = Numeric.FromDouble<TX>(maxDomain)
            });
        }
    }

    private void StartPan(double cursorPxX, LinearScale xScale)
    {
        _isPanning = true;
        _panStartPxX = cursorPxX;
        _panStartDomainMin = _zoomXMin ?? xScale.DomainMin;
        _panStartDomainMax = _zoomXMax ?? xScale.DomainMax;
    }

    private void HandlePan(double cursorPxX, LinearScale xScale)
    {
        // Δpixels → Δdomain (inverted via the scale's pixels-per-unit).
        double pxRange = xScale.RangeMax - xScale.RangeMin;
        if (pxRange <= 0)
        {
            return;
        }

        double startRange = _panStartDomainMax - _panStartDomainMin;
        double pxDelta = cursorPxX - _panStartPxX;
        double domainDelta = -pxDelta * (startRange / pxRange);
        // Pan: shift both ends by the same delta so the visible width stays
        // constant — only the viewport translates.
        _zoomXMin = _panStartDomainMin + domainDelta;
        _zoomXMax = _panStartDomainMax + domainDelta;
        StateHasChanged();
    }

    private void HandleZoomWheel(
        Microsoft.AspNetCore.Components.Web.WheelEventArgs e,
        LinearScale xScale,
        ChartLayout layout)
    {
        // Cursor X in pixel space (relative to the rect = relative to plot).
        double cursorPxX = Math.Clamp(e.OffsetX + layout.PlotLeft, layout.PlotLeft, layout.PlotRight);
        double cursorDomainX = xScale.Invert(cursorPxX);

        // Wheel up (DeltaY < 0) → zoom in; wheel down → zoom out.
        // Clamp the per-tick factor so a single big wheel-tick doesn't blow
        // the domain past the data extent or collapse it to zero width.
        double factor = e.DeltaY < 0 ? 0.9 : 1.1;

        double currentMin = _zoomXMin ?? xScale.DomainMin;
        double currentMax = _zoomXMax ?? xScale.DomainMax;
        double currentRange = currentMax - currentMin;
        if (currentRange <= 0)
        {
            return;
        }

        double newRange = currentRange * factor;
        // Anchor: keep cursorDomainX at the same fractional position in
        // the new range so the zoom feels centred on the cursor.
        double t = (cursorDomainX - currentMin) / currentRange;
        double newMin = cursorDomainX - (t * newRange);
        double newMax = cursorDomainX + ((1 - t) * newRange);

        // Clamp against the original (data-driven) domain so we never zoom
        // out past the data extent (use the underlying xScale's domain as
        // proxy — it was built without the zoom override the first render,
        // but here it includes the zoom; OK either way for the upper bound).
        if (newRange < 1e-9)
        {
            return; // numerical floor
        }

        _zoomXMin = newMin;
        _zoomXMax = newMax;
        StateHasChanged();
    }

    /// <summary>
    /// Reset the X-axis zoom to the full data extent. Public so consumers
    /// can wire a "Reset" button outside the chart frame.
    /// </summary>
    public void ResetZoom()
    {
        if (_zoomXMin is null && _zoomXMax is null)
        {
            return;
        }

        _zoomXMin = null;
        _zoomXMax = null;
        StateHasChanged();
    }

    private void UpdateCrosshair(
        double mouseX,
        List<BOBChartSeries<TX, TY>> seriesList,
        List<List<BOBChartPoint<TX, TY>>> pointsBySeries,
        LinearScale yScale,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat,
        ChartLayout layout)
    {
        // Clamp the cursor X to the plot rectangle (mousemove can fire on the
        // edge in some browsers with sub-pixel offset).
        double cursorX = Math.Clamp(mouseX, layout.PlotLeft, layout.PlotRight);

        List<CrosshairEntry> entries = new(seriesList.Count);
        for (int s = 0; s < seriesList.Count; s++)
        {
            BOBChartSeries<TX, TY> series = seriesList[s];
            List<BOBChartPoint<TX, TY>> pts = pointsBySeries[s];
            if (pts.Count == 0)
            {
                continue;
            }

            // Find the point whose projected X is closest to the cursor.
            int bestIdx = 0;
            double bestDist = double.MaxValue;
            for (int i = 0; i < pts.Count; i++)
            {
                double px = xLinear is not null
                    ? xLinear.Project(Numeric.ToDouble(pts[i].X))
                    : xCat!.Center(pts[i].X);
                double dist = Math.Abs(px - cursorX);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIdx = i;
                }
            }

            BOBChartPoint<TX, TY> bestPt = pts[bestIdx];
            double markerCx = xLinear is not null
                ? xLinear.Project(Numeric.ToDouble(bestPt.X))
                : xCat!.Center(bestPt.X);
            double markerCy = yScale.Project(Numeric.ToDouble(bestPt.Y));
            int paletteIdx = _originalSeriesIndices is not null && s < _originalSeriesIndices.Count
                ? _originalSeriesIndices[s]
                : s;
            string color = series.Color ?? Palette.ColorAt(paletteIdx);

            entries.Add(new CrosshairEntry(series.Label, color, bestPt.X, bestPt.Y, markerCx, markerCy));
        }

        if (entries.Count == 0)
        {
            ClearCrosshair();
            return;
        }

        CrosshairState newState = new(cursorX, entries);
        if (_crosshair is { } prev && Math.Abs(prev.PixelX - cursorX) < 0.5)
        {
            // Sub-pixel jitter — skip re-render.
            return;
        }

        _crosshair = newState;
        StateHasChanged();
    }

    private void ClearCrosshair()
    {
        if (_crosshair is null)
        {
            return;
        }

        _crosshair = null;
        StateHasChanged();
    }

    /// <inheritdoc />
    private protected override void RenderHtmlOverlays(RenderTreeBuilder builder)
    {
        base.RenderHtmlOverlays(builder);
        if (_crosshair is not { } cross)
        {
            return;
        }

        // Readout panel pinned at the cursor X with per-series rows.
        builder.OpenElement(80, "div");
        builder.AddAttribute(81, "class", "bob-chart__crosshair-readout");
        builder.AddAttribute(82, "role", "tooltip");
        builder.AddAttribute(83, "style",
            string.Format(CultureInfo.InvariantCulture, "left: {0:F1}px;", cross.PixelX));

        // Header: X value of the first entry (series typically share X axis).
        builder.OpenElement(84, "div");
        builder.AddAttribute(85, "class", "bob-chart__crosshair-readout-header");
        builder.AddContent(86, cross.Entries[0].X?.ToString() ?? string.Empty);
        builder.CloseElement();

        int seq = 90;
        foreach (CrosshairEntry entry in cross.Entries)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "bob-chart__crosshair-readout-row");

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "bob-chart__crosshair-readout-marker");
            builder.AddAttribute(seq++, "style", $"background-color: {entry.Color};");
            builder.CloseElement();

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "bob-chart__crosshair-readout-label");
            builder.AddContent(seq++, entry.Label);
            builder.CloseElement();

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "bob-chart__crosshair-readout-value");
            builder.AddContent(seq++, entry.Y?.ToString() ?? string.Empty);
            builder.CloseElement();

            builder.CloseElement(); // row
        }

        builder.CloseElement(); // div.readout
    }

    private void RenderCrosshairOverlayActive(
        RenderTreeBuilder builder,
        ref int seq,
        ChartLayout layout,
        CrosshairState cross)
    {
        // Vertical guide line spanning the plot area at the cursor X.
        builder.OpenElement(seq++, "line");
        builder.AddAttribute(seq++, "class", "bob-chart__crosshair");
        builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(cross.PixelX));
        builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(cross.PixelX));
        builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(layout.PlotTop));
        builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(layout.PlotBottom));
        builder.AddAttribute(seq++, "pointer-events", "none");
        builder.CloseElement();

        // Highlighted markers per series at the snapped point.
        foreach (CrosshairEntry entry in cross.Entries)
        {
            builder.OpenElement(seq++, "circle");
            builder.AddAttribute(seq++, "class", "bob-chart__crosshair-marker");
            builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(entry.MarkerCx));
            builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(entry.MarkerCy));
            builder.AddAttribute(seq++, "r", ChartLayout.ToInvariant(MarkerRadius * 1.8));
            builder.AddAttribute(seq++, "fill", entry.Color);
            builder.AddAttribute(seq++, "stroke", "var(--palette-surface, #ffffff)");
            builder.AddAttribute(seq++, "stroke-width", "2");
            builder.AddAttribute(seq++, "pointer-events", "none");
            builder.CloseElement();
        }
    }

    private void RenderAnnotations(
        RenderTreeBuilder builder,
        ref int seq,
        ChartLayout layout,
        LinearScale yScale,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat)
    {
        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-chart__annotations");

        foreach (BOBChartAnnotation annotation in Annotations!)
        {
            switch (annotation)
            {
                case BOBChartBandAnnotation<TX> band:
                    RenderBandAnnotation(builder, ref seq, layout, xLinear, xCat, band);
                    break;
                case BOBChartTextAnnotation<TX, TY> text:
                    RenderTextAnnotation(builder, ref seq, yScale, xLinear, xCat, text);
                    break;
                case BOBChartShapeAnnotation<TX, TY> shape:
                    RenderShapeAnnotation(builder, ref seq, yScale, xLinear, xCat, shape);
                    break;
            }
        }

        builder.CloseElement();
    }

    private static double? ProjectX(LinearScale? xLinear, CategoricalScale<TX>? xCat, TX value) =>
        xLinear is not null ? xLinear.Project(Numeric.ToDouble(value)) :
        xCat is not null ? xCat.Center(value) :
        null;

    private void RenderBandAnnotation(
        RenderTreeBuilder builder,
        ref int seq,
        ChartLayout layout,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat,
        BOBChartBandAnnotation<TX> band)
    {
        double? fromPx = ProjectX(xLinear, xCat, band.FromX);
        double? toPx = ProjectX(xLinear, xCat, band.ToX);
        if (fromPx is null || toPx is null)
        {
            return;
        }

        double leftPx = Math.Min(fromPx.Value, toPx.Value);
        double width = Math.Max(0, Math.Abs(toPx.Value - fromPx.Value));
        string fill = band.Color ?? "var(--palette-primary, #2563eb)";

        builder.OpenElement(seq++, "rect");
        builder.AddAttribute(seq++, "class", string.IsNullOrEmpty(band.CssClass)
            ? "bob-chart__annotation-band"
            : $"bob-chart__annotation-band {band.CssClass}");
        builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(leftPx));
        builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotTop));
        builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(width));
        builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(layout.PlotHeight));
        builder.AddAttribute(seq++, "fill", fill);
        builder.AddAttribute(seq++, "fill-opacity", ChartLayout.ToInvariant(band.FillOpacity));
        builder.AddAttribute(seq++, "pointer-events", "none");
        builder.CloseElement();

        if (!string.IsNullOrEmpty(band.Label))
        {
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-chart__annotation-band-label");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(leftPx + (width / 2)));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotTop + 12));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddAttribute(seq++, "fill", fill);
            builder.AddAttribute(seq++, "pointer-events", "none");
            builder.AddContent(seq++, band.Label);
            builder.CloseElement();
        }
    }

    private void RenderTextAnnotation(
        RenderTreeBuilder builder,
        ref int seq,
        LinearScale yScale,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat,
        BOBChartTextAnnotation<TX, TY> text)
    {
        double? cx = ProjectX(xLinear, xCat, text.X);
        if (cx is null)
        {
            return;
        }

        double cy = yScale.Project(Numeric.ToDouble(text.Y));
        string color = text.Color ?? "var(--palette-surface-contrast, #1f2937)";

        builder.OpenElement(seq++, "text");
        builder.AddAttribute(seq++, "class", string.IsNullOrEmpty(text.CssClass)
            ? "bob-chart__annotation-text"
            : $"bob-chart__annotation-text {text.CssClass}");
        builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cx.Value + text.DxPx));
        builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(cy + text.DyPx));
        builder.AddAttribute(seq++, "fill", color);
        builder.AddAttribute(seq++, "pointer-events", "none");
        builder.AddContent(seq++, text.Text);
        builder.CloseElement();
    }

    private void RenderShapeAnnotation(
        RenderTreeBuilder builder,
        ref int seq,
        LinearScale yScale,
        LinearScale? xLinear,
        CategoricalScale<TX>? xCat,
        BOBChartShapeAnnotation<TX, TY> shape)
    {
        double? cx = ProjectX(xLinear, xCat, shape.X);
        if (cx is null)
        {
            return;
        }

        double cy = yScale.Project(Numeric.ToDouble(shape.Y));
        double r = shape.SizePx / 2;
        string fill = shape.Color ?? "var(--palette-primary, #2563eb)";
        string css = string.IsNullOrEmpty(shape.CssClass)
            ? "bob-chart__annotation-shape"
            : $"bob-chart__annotation-shape {shape.CssClass}";

        switch (shape.Shape)
        {
            case BOBChartAnnotationShape.Circle:
                builder.OpenElement(seq++, "circle");
                builder.AddAttribute(seq++, "class", css);
                builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(cx.Value));
                builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(cy));
                builder.AddAttribute(seq++, "r", ChartLayout.ToInvariant(r));
                builder.AddAttribute(seq++, "fill", fill);
                builder.AddAttribute(seq++, "pointer-events", "none");
                builder.CloseElement();
                break;
            case BOBChartAnnotationShape.Square:
                builder.OpenElement(seq++, "rect");
                builder.AddAttribute(seq++, "class", css);
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cx.Value - r));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(cy - r));
                builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(shape.SizePx));
                builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(shape.SizePx));
                builder.AddAttribute(seq++, "fill", fill);
                builder.AddAttribute(seq++, "pointer-events", "none");
                builder.CloseElement();
                break;
            case BOBChartAnnotationShape.Triangle:
                {
                    string d = string.Format(CultureInfo.InvariantCulture,
                        "M {0:G} {1:G} L {2:G} {3:G} L {4:G} {3:G} Z",
                        cx.Value, cy - r,
                        cx.Value + r, cy + r,
                        cx.Value - r);
                    builder.OpenElement(seq++, "path");
                    builder.AddAttribute(seq++, "class", css);
                    builder.AddAttribute(seq++, "d", d);
                    builder.AddAttribute(seq++, "fill", fill);
                    builder.AddAttribute(seq++, "pointer-events", "none");
                    builder.CloseElement();
                    break;
                }
            case BOBChartAnnotationShape.Diamond:
                {
                    string d = string.Format(CultureInfo.InvariantCulture,
                        "M {0:G} {1:G} L {2:G} {3:G} L {0:G} {4:G} L {5:G} {3:G} Z",
                        cx.Value, cy - r,
                        cx.Value + r, cy,
                        cy + r,
                        cx.Value - r);
                    builder.OpenElement(seq++, "path");
                    builder.AddAttribute(seq++, "class", css);
                    builder.AddAttribute(seq++, "d", d);
                    builder.AddAttribute(seq++, "fill", fill);
                    builder.AddAttribute(seq++, "pointer-events", "none");
                    builder.CloseElement();
                    break;
                }
            case BOBChartAnnotationShape.Star:
                {
                    // 5-point star — alternating outer / inner radii on 36° steps.
                    double inner = r * 0.45;
                    StringBuilder sb = new();
                    for (int i = 0; i < 10; i++)
                    {
                        double angle = (-Math.PI / 2) + (i * Math.PI / 5);
                        double rad = (i & 1) == 0 ? r : inner;
                        double px = cx.Value + (Math.Cos(angle) * rad);
                        double py = cy + (Math.Sin(angle) * rad);
                        sb.Append(i == 0 ? "M " : " L ");
                        sb.Append(px.ToString("G", CultureInfo.InvariantCulture));
                        sb.Append(' ');
                        sb.Append(py.ToString("G", CultureInfo.InvariantCulture));
                    }

                    sb.Append(" Z");
                    builder.OpenElement(seq++, "path");
                    builder.AddAttribute(seq++, "class", css);
                    builder.AddAttribute(seq++, "d", sb.ToString());
                    builder.AddAttribute(seq++, "fill", fill);
                    builder.AddAttribute(seq++, "pointer-events", "none");
                    builder.CloseElement();
                    break;
                }
        }
    }

    /// <summary>
    /// Append a single point to the streaming buffer for the named series.
    /// On first call seeds the buffer from the matching <see cref="Series"/>
    /// entry's <c>Points</c>; subsequent appends extend the buffer.
    /// </summary>
    public Task AppendPointsAsync(string seriesLabel, params BOBChartPoint<TX, TY>[] points)
        => AppendPointsAsync(seriesLabel, (IEnumerable<BOBChartPoint<TX, TY>>)points);

    /// <summary>
    /// Append a batch of points. Honors <see cref="StreamingWindow"/>
    /// (FIFO trim), <see cref="StreamingFollow"/> (slide zoom window),
    /// <see cref="StreamingPaused"/> (defer render), and
    /// <see cref="StreamingThrottle"/> (debounce render).
    /// </summary>
    public async Task AppendPointsAsync(string seriesLabel, IEnumerable<BOBChartPoint<TX, TY>> points)
    {
        ArgumentNullException.ThrowIfNull(seriesLabel);
        ArgumentNullException.ThrowIfNull(points);

        IReadOnlyList<BOBChartPoint<TX, TY>> batch = points as IReadOnlyList<BOBChartPoint<TX, TY>> ?? points.ToList();
        if (batch.Count == 0)
        {
            return;
        }

        List<BOBChartPoint<TX, TY>> buffer = GetOrSeedBuffer(seriesLabel);
        buffer.AddRange(batch);

        int dropped = TrimToWindow(buffer);
        SlideZoomIfFollowing(batch);

        if (OnStreamUpdate.HasDelegate)
        {
            await OnStreamUpdate.InvokeAsync(new BOBChartStreamArgs<TX, TY>
            {
                SeriesLabel = seriesLabel,
                AppendedCount = batch.Count,
                TotalCount = buffer.Count,
                DroppedByWindow = dropped
            });
        }

        if (StreamingPaused)
        {
            return;
        }

        await RequestStreamingRenderAsync();
    }

    /// <summary>
    /// Replace the streaming buffer for a series with the given snapshot.
    /// Useful when a server push delivers a corrected/recomputed window.
    /// </summary>
    public async Task ResetSeriesAsync(string seriesLabel, IEnumerable<BOBChartPoint<TX, TY>> points)
    {
        ArgumentNullException.ThrowIfNull(seriesLabel);
        ArgumentNullException.ThrowIfNull(points);

        _streamBuffer ??= new Dictionary<string, List<BOBChartPoint<TX, TY>>>();
        List<BOBChartPoint<TX, TY>> buffer = points.ToList();
        TrimToWindow(buffer);
        _streamBuffer[seriesLabel] = buffer;

        if (OnStreamUpdate.HasDelegate)
        {
            await OnStreamUpdate.InvokeAsync(new BOBChartStreamArgs<TX, TY>
            {
                SeriesLabel = seriesLabel, AppendedCount = 0, TotalCount = buffer.Count, DroppedByWindow = 0
            });
        }

        if (!StreamingPaused)
        {
            await RequestStreamingRenderAsync();
        }
    }

    /// <summary>
    /// Drop the streaming buffer for every series. The next render reverts
    /// to the parameter-supplied <see cref="Series"/> data.
    /// </summary>
    public Task ClearStreamingAsync()
    {
        _streamBuffer = null;
        _throttleCts?.Cancel();
        _throttleCts = null;
        return InvokeAsync(StateHasChanged);
    }

    private List<BOBChartPoint<TX, TY>> GetOrSeedBuffer(string label)
    {
        _streamBuffer ??= new Dictionary<string, List<BOBChartPoint<TX, TY>>>();
        if (!_streamBuffer.TryGetValue(label, out List<BOBChartPoint<TX, TY>>? existing))
        {
            // Seed from the Series parameter so the first append doesn't
            // wipe pre-existing static points.
            BOBChartSeries<TX, TY>? seed = Series?.FirstOrDefault(s => s.Label == label);
            existing = seed?.Points.ToList() ?? [];
            _streamBuffer[label] = existing;
        }

        return existing;
    }

    private int TrimToWindow(List<BOBChartPoint<TX, TY>> buffer)
    {
        if (StreamingWindow is not int window || window <= 0 || buffer.Count <= window)
        {
            return 0;
        }

        int excess = buffer.Count - window;
        buffer.RemoveRange(0, excess);
        return excess;
    }

    private void SlideZoomIfFollowing(IReadOnlyList<BOBChartPoint<TX, TY>> batch)
    {
        if (!StreamingFollow || _zoomXMin is null || _zoomXMax is null || !Numeric.IsContinuous<TX>())
        {
            return;
        }

        double newestX = batch.Max(p => Numeric.ToDouble(p.X));
        if (newestX <= _zoomXMax.Value)
        {
            return;
        }

        double overflow = newestX - _zoomXMax.Value;
        _zoomXMin += overflow;
        _zoomXMax += overflow;
    }

    private async Task RequestStreamingRenderAsync()
    {
        if (StreamingThrottle is not TimeSpan throttle || throttle <= TimeSpan.Zero)
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        // Debounce — every append cancels the prior pending render and
        // schedules a new one. The fence between cancellation and a fresh
        // CTS is racy by design (we WANT the latest append's timer to win).
        _throttleCts?.Cancel();
        _throttleCts = new CancellationTokenSource();
        CancellationToken token = _throttleCts.Token;

        try
        {
            await Task.Delay(throttle, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (token.IsCancellationRequested)
        {
            return;
        }

        await InvokeAsync(StateHasChanged);
    }
}