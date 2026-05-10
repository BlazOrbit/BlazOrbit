using BlazOrbit.Charts.Models;

namespace BlazOrbit.Charts.Abstractions;

/// <summary>
/// Implemented by chart types that consume one or more typed data series
/// over an X / Y plane (Bar, Line, Area).
/// </summary>
/// <typeparam name="TX">Type of the X-axis values (e.g. <see cref="System.DateTime"/>, <see cref="string"/>, <see cref="double"/>).</typeparam>
/// <typeparam name="TY">Type of the Y-axis values (typically a numeric type).</typeparam>
public interface IHasSeries<TX, TY>
{
    /// <summary>
    /// The series rendered by the chart, in declaration order. Order affects
    /// stacking, legend listing and z-index.
    /// </summary>
    IEnumerable<BOBChartSeries<TX, TY>>? Series { get; }
}
