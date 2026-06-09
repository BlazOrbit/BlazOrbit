using BlazOrbit.Charts.Enums;

namespace BlazOrbit.Charts.Models;

/// <summary>
/// Per-axis configuration. Both X and Y axes share the same shape; chart
/// types ignore the irrelevant fields (e.g. <see cref="Format"/> on a
/// categorical axis is a no-op).
/// </summary>
public sealed class BOBChartAxis
{
    /// <summary>Optional axis title rendered next to the tick labels.</summary>
    public string? Title { get; init; }

    /// <summary>Scale strategy. <see cref="BOBChartAxisType.Auto"/> by default.</summary>
    public BOBChartAxisType Type { get; init; } = BOBChartAxisType.Auto;

    /// <summary>Whether to render the gridline at each major tick.</summary>
    public bool ShowGrid { get; init; } = true;

    /// <summary>Whether to render the tick labels.</summary>
    public bool ShowLabels { get; init; } = true;

    /// <summary>
    /// Optional <c>string.Format</c> pattern applied to numeric or temporal
    /// tick labels (e.g. <c>"N0"</c>, <c>"yyyy-MM"</c>, <c>"€ {0:N2}"</c>).
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Optional explicit minimum domain value. <c>null</c> = auto-compute from
    /// the data.
    /// </summary>
    public double? Min { get; init; }

    /// <summary>
    /// Optional explicit maximum domain value. <c>null</c> = auto-compute from
    /// the data.
    /// </summary>
    public double? Max { get; init; }
}