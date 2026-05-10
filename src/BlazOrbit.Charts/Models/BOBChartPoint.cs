namespace BlazOrbit.Charts.Models;

/// <summary>
/// A single (X, Y) data point in a chart series.
/// </summary>
/// <typeparam name="TX">Type of the X-axis value.</typeparam>
/// <typeparam name="TY">Type of the Y-axis value.</typeparam>
/// <param name="X">X-axis value.</param>
/// <param name="Y">Y-axis value.</param>
public readonly record struct BOBChartPoint<TX, TY>(TX X, TY Y);
