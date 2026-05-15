namespace BlazOrbit.Charts.Components;

/// <summary>
/// Pie chart - full circular distribution. Each <c>BOBChartSlice</c>'s value
/// is normalised to 100% across the slice set; slice order goes clockwise
/// from 12 o'clock following declaration order.
/// <para>
/// For an annular variant (donut), see <see cref="BOBDonutChart{TY}"/>.
/// </para>
/// </summary>
/// <typeparam name="TY">Numeric type of the slice values.</typeparam>
public sealed class BOBPieChart<TY> : BOBPieChartBase<TY>
{
    /// <inheritdoc />
    protected override string ChartTypeNoun => "Pie";
}