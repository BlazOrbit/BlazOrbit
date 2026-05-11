namespace BlazOrbit.Charts.Models;

/// <summary>
/// A single (X, Y, Size) data point for <see cref="Components.BOBScatterChart{TX, TY}"/>
/// in bubble mode. The third dimension <see cref="Size"/> is rendered as
/// the marker radius after auto-scaling against the rest of the series.
/// Use <see cref="BOBChartPoint{TX, TY}"/> when only X / Y matter.
/// </summary>
/// <typeparam name="TX">Type of the X-axis value.</typeparam>
/// <typeparam name="TY">Type of the Y-axis value.</typeparam>
public readonly record struct BOBChartBubblePoint<TX, TY>(TX X, TY Y, double Size);
