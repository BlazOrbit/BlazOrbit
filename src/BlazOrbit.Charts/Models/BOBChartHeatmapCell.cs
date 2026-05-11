namespace BlazOrbit.Charts.Models;

/// <summary>
/// Cell in a <see cref="Components.BOBHeatmapChart{TX, TY}"/> grid:
/// (X-bucket, Y-bucket, intensity Value).
/// </summary>
/// <typeparam name="TX">Column key type.</typeparam>
/// <typeparam name="TY">Row key type.</typeparam>
public readonly record struct BOBChartHeatmapCell<TX, TY>(TX X, TY Y, double Value);
