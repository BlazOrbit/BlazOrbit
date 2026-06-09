namespace BlazOrbit.Charts.Enums;

/// <summary>
/// Per-series chart-type override for <see cref="Components.BOBMixedChart{TX}"/>. Each
/// series declares which visual representation it uses; the host plot blends all
/// types into a single coordinate space (shared X axis, optional secondary Y axis).
/// </summary>
public enum BOBChartMixedSeriesType
{
    /// <summary>Vertical bar - categorical / discrete distribution.</summary>
    Bar = 0,

    /// <summary>Line - continuous trend.</summary>
    Line = 1,

    /// <summary>Area - line + filled region below.</summary>
    Area = 2
}
