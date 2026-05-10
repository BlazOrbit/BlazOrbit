namespace BlazOrbit.Charts.Enums;

/// <summary>
/// Position of the legend block relative to the chart plot area.
/// </summary>
public enum BOBChartLegendPosition
{
    /// <summary>Hide the legend.</summary>
    None = 0,

    /// <summary>Render above the plot area.</summary>
    Top,

    /// <summary>Render below the plot area (default).</summary>
    Bottom,

    /// <summary>Render to the left of the plot area.</summary>
    Left,

    /// <summary>Render to the right of the plot area.</summary>
    Right,
}
