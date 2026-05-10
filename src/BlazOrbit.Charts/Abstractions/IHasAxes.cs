using BlazOrbit.Charts.Models;

namespace BlazOrbit.Charts.Abstractions;

/// <summary>
/// Implemented by chart types that render a Cartesian coordinate system with
/// X and Y axes (Bar, Line, Area). Pie / Donut do not implement this.
/// </summary>
public interface IHasAxes
{
    /// <summary>Configuration for the horizontal axis.</summary>
    BOBChartAxis XAxis { get; }

    /// <summary>Configuration for the vertical axis.</summary>
    BOBChartAxis YAxis { get; }
}
