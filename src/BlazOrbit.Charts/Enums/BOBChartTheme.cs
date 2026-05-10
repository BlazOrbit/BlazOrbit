namespace BlazOrbit.Charts.Enums;

/// <summary>
/// Override for the chart's theme. By default the chart inherits from the
/// active <c>BOBPalette</c> cascading value; setting this parameter on the
/// component forces a specific theme regardless of the application's current
/// theme.
/// </summary>
public enum BOBChartTheme
{
    /// <summary>Inherit from the active BlazOrbit palette (default).</summary>
    Inherit = 0,

    /// <summary>Force the light palette tokens.</summary>
    Light,

    /// <summary>Force the dark palette tokens.</summary>
    Dark,
}
