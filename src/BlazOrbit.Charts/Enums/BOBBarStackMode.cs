namespace BlazOrbit.Charts.Enums;

/// <summary>
/// Layout strategy for multi-series bar charts.
/// </summary>
public enum BOBBarStackMode
{
    /// <summary>
    /// Default - each series renders its own bar within the category band,
    /// side-by-side. Bar width = <c>BarGroupRatio × bandWidth / seriesCount</c>.
    /// Series independent; no Y accumulation across series.
    /// </summary>
    None = 0,

    /// <summary>
    /// Stacked - series render on top of each other within a single bar per
    /// category. Each series segment starts at the cumulative Y of the
    /// previous series and extends by its own value. Y axis auto-scales to
    /// the largest cumulative total. Useful for "total broken down by
    /// dimension" stories (revenue by region, traffic by channel).
    /// </summary>
    Stacked,

    /// <summary>
    /// 100% stacked - same as <see cref="Stacked"/> but every category is
    /// normalised to <c>100%</c>. The Y axis is fixed at <c>[0, 100]</c>.
    /// Useful for "share-of" comparisons where absolute totals differ
    /// across categories but the proportional split is the message.
    /// </summary>
    PercentStacked,

    /// <summary>
    /// Bidirectional stacked - positive series values stack upward from the
    /// zero baseline, negative values stack downward, each side keeps its
    /// own running cumulative. Y domain auto-fits both extremes so the
    /// largest positive total and the largest (most negative) negative
    /// total both fit. Useful for budget variance, P&amp;L by segment,
    /// inflow vs outflow visualisations.
    /// </summary>
    Bidirectional,

    /// <summary>
    /// Waterfall - single-series mode where each bar represents a delta
    /// from the running cumulative of all previous bars. Positive deltas
    /// step the running total up; negative deltas step it down. Useful for
    /// "starting balance → adjustments → ending balance" stories
    /// (P&amp;L bridges, budget actuals vs plan, fund flow). Multi-series
    /// input is rejected - only the first series is rendered.
    /// </summary>
    Waterfall
}