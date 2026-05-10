using BlazOrbit.Abstractions;
using BlazOrbit.Charts.Abstractions;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Services.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Abstract base for every BOBCharts component (Bar, Line, Pie / Donut, Area).
/// Provides:
/// <list type="bullet">
/// <item>The <c>&lt;bob-component data-bob-component="…" data-bob-data-visualization-base&gt;</c>
///       host element shared by the family.</item>
/// <item>A child <c>&lt;svg&gt;</c> root sized by <see cref="Width"/> /
///       <see cref="Height"/> (or 100% when both are <c>null</c>).</item>
/// <item>The <see cref="RenderSvg"/> hook concrete charts override to emit
///       their geometry inside the SVG.</item>
/// <item>Optional Export-as-PNG button wired through <see cref="IChartJsInterop"/>.</item>
/// </list>
/// <para>
/// Concrete subclasses are <c>sealed</c> and live in the same namespace.
/// </para>
/// </summary>
/// <typeparam name="TX">Type of the X-axis values.</typeparam>
/// <typeparam name="TY">Type of the Y-axis values.</typeparam>
public abstract class BOBChartBase<TX, TY> : BOBComponentBase, IDataVisualizationFamilyComponent
{
    private ElementReference _svgRef;
    private ElementReference _hostRef;

    /// <summary>
    /// Backing set for series / slice labels the user has hidden via the
    /// interactive legend. Filtered out by concrete charts at render time.
    /// </summary>
    private readonly HashSet<string> _hiddenSeries = new(StringComparer.Ordinal);

    private bool _chartReadyFired;

    // ---- Responsive layout via ResizeObserver ----
    // Effective dimensions come from (priority):
    //   1. Explicit Width / Height parameters (caller wins).
    //   2. Measured host dims via ResizeObserver (auto-sized).
    //   3. Fallback constants (600 × 400).
    // The observer is installed lazily on first render and only when the
    // caller has NOT pinned both dimensions — explicit Width+Height skips
    // the JS interop entirely.
    private DotNetObjectReference<BOBChartBase<TX, TY>>? _dotnetRef;
    private string? _resizeHandle;
    private double? _measuredWidth;
    private double? _measuredHeight;

    /// <summary>
    /// Active tooltip state — set on data-point hover by concrete cartesian
    /// charts via <see cref="SetActiveTooltip"/>, cleared on mouseleave via
    /// <see cref="ClearActiveTooltip"/>. <c>null</c> when no point is hovered.
    /// </summary>
    private (double PixelX, double PixelY, Models.BOBChartTooltipContext<TX, TY> Context)? _activeTooltip;

    [Inject]
    private IChartJsInterop ChartInterop { get; set; } = default!;

    /// <summary>
    /// Optional explicit width in pixels. When <c>null</c> the chart fills its
    /// container.
    /// </summary>
    [Parameter] public int? Width { get; set; }

    /// <summary>
    /// Optional explicit height in pixels. When <c>null</c> the chart fills
    /// its container.
    /// </summary>
    [Parameter] public int? Height { get; set; }

    /// <summary>
    /// CSS aspect ratio applied when neither <see cref="Width"/> nor
    /// <see cref="Height"/> are set (e.g. <c>"16/9"</c>).
    /// </summary>
    [Parameter] public string AspectRatio { get; set; } = "16/9";

    /// <summary>
    /// When <c>true</c>, a small "Export PNG" button is rendered in the
    /// chart's top-right corner. Clicking it triggers a client-side
    /// SVG → PNG rasterization (white background) and a browser download.
    /// </summary>
    [Parameter] public bool ShowExportButton { get; set; }

    /// <summary>
    /// Filename (without extension) used for the PNG export. Defaults to
    /// <c>"chart"</c>; consumers should set this to something more
    /// descriptive (e.g. <c>"sales-by-quarter"</c>).
    /// </summary>
    [Parameter] public string ExportFileName { get; set; } = "chart";

    /// <summary>
    /// Whether the legend block is visible. The legend lists every series /
    /// slice with its color marker; clicking an entry toggles that
    /// series visibility. Default <c>true</c>.
    /// </summary>
    [Parameter] public bool ShowLegend { get; set; } = true;

    /// <summary>
    /// Anchor of the legend block relative to the plot area. <c>None</c>
    /// hides the legend entirely (equivalent to <see cref="ShowLegend"/> = false).
    /// </summary>
    [Parameter] public BOBChartLegendPosition LegendPosition { get; set; } = BOBChartLegendPosition.Bottom;

    /// <summary>
    /// Fired when the user toggles a legend entry. The argument is the
    /// <c>Label</c> of the affected series / slice. Fires <em>after</em> the
    /// hidden-series set has been mutated, so consumers can read the current
    /// visibility state via <see cref="IsSeriesHidden(string)"/>.
    /// </summary>
    [Parameter] public EventCallback<string> OnLegendToggle { get; set; }

    /// <summary>
    /// Fired exactly once, after the chart has completed its first render.
    /// Useful for triggering downstream actions that require the SVG to be
    /// in the DOM (e.g. measuring, programmatic export).
    /// </summary>
    [Parameter] public EventCallback OnChartReady { get; set; }

    /// <summary>
    /// Forces a specific theme palette regardless of the application's
    /// current theme. <see cref="BOBChartTheme.Inherit"/> (default) keeps
    /// the chart in sync with the host's <c>data-bob-theme</c>; <c>Light</c>
    /// or <c>Dark</c> emits a chart-local <c>data-bob-theme</c> override.
    /// </summary>
    [Parameter] public BOBChartTheme Theme { get; set; } = BOBChartTheme.Inherit;

    /// <summary>
    /// Custom tooltip render fragment. When set, replaces the native
    /// SVG <c>&lt;title&gt;</c> tooltip on cartesian charts (Bar / Line /
    /// Area) with an HTML overlay that follows the hovered data point.
    /// Receives a <see cref="Models.BOBChartTooltipContext{TX, TY}"/> with
    /// the active series label, X, Y and resolved color.
    /// <para>
    /// Pie / Donut continue to use the native <c>&lt;title&gt;</c> tooltip
    /// — for custom slice UI use <c>OnSliceHover</c> + your own panel.
    /// </para>
    /// </summary>
    [Parameter] public RenderFragment<Models.BOBChartTooltipContext<TX, TY>>? TooltipTemplate { get; set; }

    /// <summary>
    /// <c>string.Format</c>-style pattern for the default cartesian tooltip
    /// (used when <see cref="TooltipTemplate"/> is <c>null</c>). Receives
    /// <c>{0}</c> = label, <c>{1}</c> = X, <c>{2}</c> = Y. Default produces
    /// <c>"&lt;label&gt;: &lt;X&gt; = &lt;Y&gt;"</c>.
    /// </summary>
    [Parameter] public string? TooltipFormat { get; set; }

    /// <summary>
    /// Whether geometry transitions are animated when data changes — bar
    /// heights resize, line markers slide, pie slice arcs grow. Default
    /// <c>true</c>.
    /// <para>
    /// Animations are CSS-transition driven and automatically respect the
    /// user's <c>prefers-reduced-motion</c> setting; setting this to
    /// <c>false</c> is only needed when consumers want a hard cutover
    /// (e.g. live-streaming dashboards where smooth interpolation would
    /// trail behind the real value).
    /// </para>
    /// </summary>
    [Parameter] public bool Animated { get; set; } = true;

    /// <summary>
    /// Duration of the data-update transition, in milliseconds. Default
    /// <c>400</c>. Ignored when <see cref="Animated"/> is <c>false</c> or
    /// the user has opted into <c>prefers-reduced-motion</c>.
    /// </summary>
    [Parameter] public int AnimationDuration { get; set; } = 400;

    /// <summary>
    /// Concrete charts override this to emit SVG content. The supplied
    /// builder is positioned <em>inside</em> the <c>&lt;svg&gt;</c> root, so
    /// implementations only need to add geometry primitives (lines, paths,
    /// rects, text). Sequence numbers must remain unique within the override.
    /// </summary>
    /// <param name="builder">Render tree builder positioned inside the SVG root.</param>
    protected abstract void RenderSvg(RenderTreeBuilder builder);

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        // <bob-component data-bob-component="<kebab-name>" data-bob-data-visualization-base ...>
        // The Core builder pipeline already injects `data-bob-component` via
        // ToKebabCaseComponentName(typeof(this).Name) into ComputedAttributes,
        // so we just spread the dictionary. The data-visualization family
        // marker is added on top because Core's reflection does not know
        // about IDataVisualizationFamilyComponent (defined in this package).
        builder.OpenElement(0, "bob-component");
        builder.AddMultipleAttributes(1, ComputedAttributes!);
        builder.AddAttribute(2, "data-bob-data-visualization-base", string.Empty);

        if (ShowExportButton)
        {
            builder.AddAttribute(3, "data-bob-has-export", string.Empty);
        }

        // Legend position drives the host's flex direction via CSS attribute
        // selectors, so the SVG and the legend block lay out automatically.
        BOBChartLegendPosition effectivePos = ShowLegend ? LegendPosition : BOBChartLegendPosition.None;
        if (effectivePos != BOBChartLegendPosition.None)
        {
            builder.AddAttribute(31, "data-bob-legend-position",
                effectivePos.ToString().ToLowerInvariant());
        }

        // Theme override: emit a chart-local `data-bob-theme` so BlazOrbit's
        // palette CSS rules ([data-bob-theme="dark"] { --palette-*: … })
        // scope the requested palette to this chart only.
        if (Theme != BOBChartTheme.Inherit)
        {
            builder.AddAttribute(32, "data-bob-theme",
                Theme.ToString().ToLowerInvariant());
        }

        // Animations: emit `data-bob-animated` + a CSS custom property so the
        // CSS bundle's `[data-bob-animated] .bob-X__Y { transition: … var(--bob-chart-anim-duration) }`
        // rules pick it up. CSS automatically downgrades to no-transition
        // under `prefers-reduced-motion: reduce`.
        if (Animated)
        {
            builder.AddAttribute(34, "data-bob-animated", string.Empty);
            builder.AddAttribute(35, "style",
                $"--bob-chart-anim-duration: {AnimationDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)}ms;");
        }

        // ElementReference capture must come AFTER all conditional
        // attribute additions on the open element, otherwise Blazor's
        // RenderTreeBuilder rejects subsequent AddAttribute calls.
        builder.AddElementReferenceCapture(33, r => _hostRef = r);

        // Inner <svg width="..." height="..." style="aspect-ratio: 16/9;">
        builder.OpenElement(4, "svg");
        builder.AddAttribute(5, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(6, "role", "img");
        builder.AddAttribute(7, "aria-label", BuildAriaLabel());

        if (Width is int w)
        {
            builder.AddAttribute(8, "width", w.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (Height is int h)
        {
            builder.AddAttribute(9, "height", h.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (Width is null && Height is null && !string.IsNullOrEmpty(AspectRatio))
        {
            builder.AddAttribute(10, "style", $"aspect-ratio: {AspectRatio}; width: 100%;");
        }

        // Capture an ElementReference on the <svg> so the export button can
        // hand it to the JS interop. Stored regardless of ShowExportButton —
        // future hooks (resize observer, etc.) can reuse it.
        builder.AddElementReferenceCapture(11, r => _svgRef = r);

        // Hand off to concrete subclass
        RenderSvg(builder);

        builder.CloseElement(); // svg

        if (ShowExportButton)
        {
            RenderExportButton(builder);
        }

        if (effectivePos != BOBChartLegendPosition.None)
        {
            RenderLegend(builder);
        }

        if (_activeTooltip is { } tip)
        {
            RenderTooltipOverlay(builder, tip.PixelX, tip.PixelY, tip.Context);
        }

        // Concrete-chart hook for additional HTML overlays (crosshair readout,
        // future zoom / brush controls). Default = no-op.
        RenderHtmlOverlays(builder);

        builder.CloseElement(); // bob-component
    }

    /// <summary>
    /// Concrete charts override to inject extra HTML overlays into the
    /// <c>bob-component</c> root <em>after</em> the SVG, tooltip, export
    /// button and legend. Used by the line / area crosshair readout.
    /// </summary>
    private protected virtual void RenderHtmlOverlays(RenderTreeBuilder builder) { }

    private void RenderTooltipOverlay(
        RenderTreeBuilder builder,
        double pixelX,
        double pixelY,
        Models.BOBChartTooltipContext<TX, TY> ctx)
    {
        builder.OpenElement(60, "div");
        builder.AddAttribute(61, "class", "bob-chart__tooltip");
        builder.AddAttribute(62, "role", "tooltip");
        builder.AddAttribute(63, "style",
            string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "left: {0:F1}px; top: {1:F1}px;", pixelX, pixelY));

        if (TooltipTemplate is not null)
        {
            builder.AddContent(64, TooltipTemplate(ctx));
        }
        else
        {
            string formatted = string.IsNullOrEmpty(TooltipFormat)
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0}: {1} = {2}", ctx.SeriesLabel, ctx.X, ctx.Y)
                : string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    TooltipFormat, ctx.SeriesLabel, ctx.X, ctx.Y);

            builder.OpenElement(64, "span");
            builder.AddAttribute(65, "class", "bob-chart__tooltip-marker");
            builder.AddAttribute(66, "style", $"background-color: {ctx.Color};");
            builder.CloseElement();

            builder.OpenElement(67, "span");
            builder.AddAttribute(68, "class", "bob-chart__tooltip-text");
            builder.AddContent(69, formatted);
            builder.CloseElement();
        }

        builder.CloseElement(); // div.bob-chart__tooltip
    }

    /// <summary>
    /// Concrete cartesian charts call this from a per-point hover handler to
    /// pin the custom tooltip overlay at the supplied pixel coordinates.
    /// No-op when neither <see cref="TooltipTemplate"/> nor a non-default
    /// <see cref="TooltipFormat"/> are set.
    /// </summary>
    private protected void SetActiveTooltip(double pixelX, double pixelY,
        Models.BOBChartTooltipContext<TX, TY> ctx)
    {
        if (TooltipTemplate is null && string.IsNullOrEmpty(TooltipFormat))
        {
            return;
        }
        _activeTooltip = (pixelX, pixelY, ctx);
        StateHasChanged();
    }

    /// <summary>Concrete cartesian charts call this from mouseleave to dismiss the overlay.</summary>
    private protected void ClearActiveTooltip()
    {
        if (_activeTooltip is null) return;
        _activeTooltip = null;
        StateHasChanged();
    }

    /// <summary>Whether the chart currently shows a custom HTML tooltip overlay.</summary>
    private protected bool HasCustomTooltip =>
        TooltipTemplate is not null || !string.IsNullOrEmpty(TooltipFormat);

    /// <summary>
    /// Concrete charts override to provide the legend entries (label + color
    /// + index, in declaration order). Returning an empty enumeration omits
    /// the legend block even when <see cref="ShowLegend"/> is <c>true</c>.
    /// </summary>
    private protected virtual IEnumerable<LegendEntry> GetLegendEntries() =>
        Array.Empty<LegendEntry>();

    /// <summary>
    /// Per-entry data emitted by <see cref="GetLegendEntries"/>.
    /// </summary>
    /// <param name="Label">Series / slice label, also the toggle key.</param>
    /// <param name="Color">Resolved color (palette or explicit override).</param>
    private protected readonly record struct LegendEntry(string Label, string Color);

    private void RenderLegend(RenderTreeBuilder builder)
    {
        List<LegendEntry> entries = GetLegendEntries().ToList();
        if (entries.Count == 0) return;

        builder.OpenElement(40, "ul");
        builder.AddAttribute(41, "class", "bob-chart__legend");
        builder.AddAttribute(42, "role", "list");

        int seq = 50;
        foreach (LegendEntry entry in entries)
        {
            bool hidden = _hiddenSeries.Contains(entry.Label);

            builder.OpenElement(seq++, "li");
            builder.AddAttribute(seq++, "class", "bob-chart__legend-item");
            if (hidden)
            {
                builder.AddAttribute(seq++, "data-bob-hidden", string.Empty);
            }

            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "class", "bob-chart__legend-button");
            builder.AddAttribute(seq++, "aria-pressed", hidden ? "false" : "true");
            string capturedLabel = entry.Label;
            builder.AddAttribute(seq++, "onclick",
                EventCallback.Factory.Create(this, () => ToggleSeriesAsync(capturedLabel)));

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "bob-chart__legend-marker");
            builder.AddAttribute(seq++, "style", $"background-color: {entry.Color};");
            builder.AddAttribute(seq++, "aria-hidden", "true");
            builder.CloseElement(); // span marker

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "bob-chart__legend-label");
            builder.AddContent(seq++, entry.Label);
            builder.CloseElement(); // span label

            builder.CloseElement(); // button
            builder.CloseElement(); // li
        }

        builder.CloseElement(); // ul
    }

    /// <summary>
    /// Whether the series / slice with the given <paramref name="label"/>
    /// is currently hidden via the legend toggle. Concrete charts query
    /// this in their <c>RenderSvg</c> to filter geometry.
    /// </summary>
    private protected bool IsSeriesHidden(string label) => _hiddenSeries.Contains(label);

    /// <summary>
    /// Toggle the visibility of a series / slice by label. Updates internal
    /// state, fires <see cref="OnLegendToggle"/>, then triggers a re-render.
    /// </summary>
    public async Task ToggleSeriesAsync(string label)
    {
        if (string.IsNullOrEmpty(label)) return;
        if (!_hiddenSeries.Add(label))
        {
            _hiddenSeries.Remove(label);
        }

        if (OnLegendToggle.HasDelegate)
        {
            await OnLegendToggle.InvokeAsync(label);
        }

        StateHasChanged();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender || IsDisposed) return;

        // Install ResizeObserver only when at least one dimension is auto.
        // When BOTH Width and Height are pinned the chart is fixed-size and
        // does not need to react to container changes — skip the JS interop.
        if (Width is null || Height is null)
        {
            try
            {
                _dotnetRef = DotNetObjectReference.Create(this);
                _resizeHandle = await ChartInterop.ObserveResizeAsync(
                    _hostRef,
                    DotNetObjectReference.Create<object>(this));

                if (IsDisposed)
                {
                    // Raced with dispose during await: tear down what we just created.
                    if (_resizeHandle is not null)
                    {
                        await ChartInterop.UnobserveResizeAsync(_resizeHandle);
                    }
                    _dotnetRef?.Dispose();
                    _dotnetRef = null;
                    return;
                }
            }
            catch (Microsoft.JSInterop.JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (TaskCanceledException) { }
        }

        if (!_chartReadyFired && OnChartReady.HasDelegate)
        {
            _chartReadyFired = true;
            try
            {
                await OnChartReady.InvokeAsync();
            }
            catch (ObjectDisposedException) { }
            catch (TaskCanceledException) { }
        }
    }

    /// <summary>
    /// Invoked by the JS-side <c>ResizeObserver</c> on every host-element
    /// dimension change. Rounds to integer pixels (sub-pixel oscillation in
    /// the observer would force needless re-renders).
    /// </summary>
    [JSInvokable]
    public Task OnResize(double width, double height)
    {
        if (IsDisposed) return Task.CompletedTask;

        // Only re-render when the rounded value actually changed — observers
        // can fire several times per second during a drag-resize.
        double w = Math.Round(width);
        double h = Math.Round(height);
        bool changed = false;
        if (Width is null && _measuredWidth != w)
        {
            _measuredWidth = w;
            changed = true;
        }
        if (Height is null && _measuredHeight != h)
        {
            _measuredHeight = h;
            changed = true;
        }
        if (changed) StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Effective chart width in pixels: explicit <see cref="Width"/> wins,
    /// then the latest <c>ResizeObserver</c> measurement, then a fallback
    /// of 600. Concrete charts read this instead of <see cref="Width"/>.
    /// </summary>
    private protected double EffectiveWidth =>
        Width ?? _measuredWidth ?? 600;

    /// <summary>
    /// Effective chart height in pixels: same priority as
    /// <see cref="EffectiveWidth"/>, fallback 400.
    /// </summary>
    private protected double EffectiveHeight =>
        Height ?? _measuredHeight ?? 400;

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        if (_resizeHandle is not null)
        {
            try
            {
                await ChartInterop.UnobserveResizeAsync(_resizeHandle);
            }
            catch (Microsoft.JSInterop.JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (TaskCanceledException) { }
            _resizeHandle = null;
        }

        _dotnetRef?.Dispose();
        _dotnetRef = null;

        await base.DisposeAsync();
    }

    /// <summary>
    /// Default ARIA label for the chart's <c>&lt;svg&gt;</c> root. Subclasses
    /// override to inject series / value summaries (improves screen-reader
    /// experience).
    /// </summary>
    protected virtual string BuildAriaLabel() => "Chart";

    private void RenderExportButton(RenderTreeBuilder builder)
    {
        builder.OpenElement(20, "button");
        builder.AddAttribute(21, "type", "button");
        builder.AddAttribute(22, "class", "bob-chart__export-button");
        builder.AddAttribute(23, "aria-label", "Export chart as PNG");
        builder.AddAttribute(24, "title", "Export PNG");
        builder.AddAttribute(25, "onclick", EventCallback.Factory.Create(this, ExportAsPngAsync));
        // Minimal inline SVG icon — avoids depending on BOBSvgIcon for a single glyph.
        builder.AddMarkupContent(26,
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" width=\"16\" height=\"16\" aria-hidden=\"true\">"
            + "<path fill=\"currentColor\" d=\"M5 20h14v-2H5v2zM12 4l-5 5h3v6h4V9h3l-5-5z\"/>"
            + "</svg>");
        builder.CloseElement();
    }

    /// <summary>
    /// Trigger a PNG export of the chart's <c>&lt;svg&gt;</c>. Public so
    /// consumers can wire the export to their own UI (e.g. an action menu)
    /// without enabling <see cref="ShowExportButton"/>.
    /// </summary>
    public async Task ExportAsPngAsync()
    {
        if (IsDisposed)
        {
            return;
        }

        try
        {
            await ChartInterop.ExportSvgAsPngAsync(_svgRef, ExportFileName, Width, Height);
        }
        catch (Microsoft.JSInterop.JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
        catch (TaskCanceledException) { }
    }
}
