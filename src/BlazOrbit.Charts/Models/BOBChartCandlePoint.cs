namespace BlazOrbit.Charts.Models;

/// <summary>
/// OHLC (open / high / low / close) data point for
/// <see cref="Components.BOBCandlestickChart{TX}"/>. Optional
/// <see cref="Volume"/> drives a paired volume bar pane when the
/// chart is configured with <c>ShowVolumePane=true</c>.
/// </summary>
/// <typeparam name="TX">Type of the X-axis (typically DateTime / int).</typeparam>
public readonly record struct BOBChartCandlePoint<TX>(
    TX X,
    double Open,
    double High,
    double Low,
    double Close,
    double Volume = 0);