namespace BlazOrbit.Components.Display;

/// <summary>
/// Direction of a <see cref="BOBStatCard.Delta"/>. Drives the trend icon and accent color.
/// </summary>
public enum BOBStatTrend
{
    /// <summary>No trend annotation; the delta is rendered neutral.</summary>
    None = 0,
    /// <summary>Positive change (rendered with success palette and an upward arrow).</summary>
    Up = 1,
    /// <summary>Negative change (rendered with error palette and a downward arrow).</summary>
    Down = 2,
    /// <summary>Plateau / no change (rendered with neutral palette and a flat arrow).</summary>
    Flat = 3
}
