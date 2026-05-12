namespace BlazOrbit.Charts.Models;

/// <summary>
/// Stroke style of a <see cref="BOBChartReferenceLine"/>.
/// </summary>
public enum BOBChartReferenceLineStyle
{
    /// <summary>Continuous solid line.</summary>
    Solid = 0,

    /// <summary>Dashed line — useful for soft thresholds (forecasts, projections).</summary>
    Dashed,

    /// <summary>Dotted line — minimal visual weight; works well for grid-tier baselines.</summary>
    Dotted
}

/// <summary>
/// Horizontal reference / threshold line drawn across the plot area at a
/// fixed Y value. Common uses: SLO targets ("99.9%"), budget thresholds,
/// year-over-year baselines, regulatory caps.
/// <para>
/// Reference lines are rendered <em>over</em> the chart geometry but <em>under</em>
/// data-point hover targets, so they remain visible without intercepting
/// click / hover interactions.
/// </para>
/// </summary>
public sealed class BOBChartReferenceLine
{
    /// <summary>
    /// Y-axis value at which the line is drawn. Projected through the
    /// chart's Y scale.
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// Optional label rendered next to the line (typically right-aligned).
    /// When <c>null</c> the line is unlabeled.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Stroke color. Accepts any CSS color string. Falls back to
    /// <c>var(--palette-warning)</c> when <c>null</c> — the conventional
    /// "attention" tone for thresholds.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>Stroke style. Default <see cref="BOBChartReferenceLineStyle.Dashed"/>.</summary>
    public BOBChartReferenceLineStyle Style { get; init; } = BOBChartReferenceLineStyle.Dashed;

    /// <summary>Stroke width in pixels. Default <c>1.5</c>.</summary>
    public double StrokeWidth { get; init; } = 1.5;
}