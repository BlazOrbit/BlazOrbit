namespace BlazOrbit.Charts.Enums;

/// <summary>
/// Technical-analysis indicator rendered in a <see cref="Components.BOBStockChart{TX}"/>
/// pane. Each indicator owns its own pane stacked below the main price chart; the X
/// axis is shared across all panes so cursor / crosshair / zoom stay synchronised.
/// </summary>
public enum BOBChartTechnicalIndicator
{
    /// <summary>Volume bar pane — one bar per candle, coloured by candle direction.</summary>
    Volume = 0,

    /// <summary>
    /// Relative Strength Index (RSI) — momentum oscillator bounded [0, 100]. Reference
    /// lines drawn at 30 (oversold) and 70 (overbought). Period defaults to 14.
    /// </summary>
    Rsi = 1,

    /// <summary>
    /// Moving Average Convergence Divergence (MACD) — fast EMA minus slow EMA, plus a
    /// signal line (EMA of MACD) and a histogram of the difference. Default 12 / 26 / 9.
    /// </summary>
    Macd = 2,

    /// <summary>
    /// Bollinger Bands — moving average ± N standard deviations. Drawn on the main
    /// price pane (not its own pane) so the bands sit alongside the candles.
    /// </summary>
    BollingerBands = 3
}
