namespace BlazOrbit.Charts.Models;

/// <summary>
/// Aggregate configuration object that bundles together the most common
/// chart-wide options. Every field is also surfaced as a top-level parameter
/// on the chart components themselves; this class exists for callers who
/// prefer to compose a single options bag and reuse it across charts.
/// </summary>
public sealed class BOBChartOptions
{
    /// <summary>Whether tooltips appear on hover. Defaults to <c>true</c>.</summary>
    public bool ShowTooltips { get; init; } = true;

    /// <summary>Whether axis gridlines are visible (Bar / Line / Area only).</summary>
    public bool ShowGrid { get; init; } = true;

    /// <summary>
    /// Whether transitions on data updates animate. Set to <c>false</c> to
    /// honour <c>prefers-reduced-motion</c> globally.
    /// </summary>
    public bool Animated { get; init; } = true;

    /// <summary>Animation duration in milliseconds. Default 400 ms.</summary>
    public int AnimationDuration { get; init; } = 400;
}