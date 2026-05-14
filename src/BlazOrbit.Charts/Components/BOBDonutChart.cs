namespace BlazOrbit.Charts.Components;

/// <summary>
/// Donut chart — annular distribution. Identical to <see cref="BOBPieChart{TY}"/>
/// but with a non-zero inner radius (default 0.6 of the outer radius) so the
/// centre is hollow. Useful for embedding a KPI label inside the ring.
/// </summary>
/// <typeparam name="TY">Numeric type of the slice values.</typeparam>
public sealed class BOBDonutChart<TY> : BOBPieChartBase<TY>
{
    /// <summary>
    /// Initializes the chart with the donut-typical inner-radius ratio (0.6).
    /// The consumer can still override <c>InnerRadius</c> at the parameter
    /// level — tighter rings (0.3) for compact dashboards, wider hollows
    /// (0.75) when the centre hosts a KPI label.
    /// </summary>
    public BOBDonutChart() => InnerRadius = 0.6;

    /// <inheritdoc />
    protected override string ChartTypeNoun => "Donut";
}