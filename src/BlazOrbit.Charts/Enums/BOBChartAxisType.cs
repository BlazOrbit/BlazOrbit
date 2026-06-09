namespace BlazOrbit.Charts.Enums;

/// <summary>
/// Axis scale strategy used to project domain values onto pixel positions.
/// </summary>
public enum BOBChartAxisType
{
    /// <summary>
    /// Auto-detect from the series data type:
    /// <see cref="System.DateTime"/> / <see cref="System.DateTimeOffset"/> →
    /// <see cref="DateTime"/>, <see cref="System.IConvertible"/> numeric → <see cref="Linear"/>,
    /// otherwise → <see cref="Categorical"/>.
    /// </summary>
    Auto = 0,

    /// <summary>Continuous numeric scale, linear interpolation.</summary>
    Linear,

    /// <summary>Discrete categorical scale, equally-spaced ticks.</summary>
    Categorical,

    /// <summary>Continuous temporal scale (DateTime / DateTimeOffset / TimeSpan).</summary>
    DateTime
}