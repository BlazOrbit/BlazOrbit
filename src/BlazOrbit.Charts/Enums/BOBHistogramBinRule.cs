namespace BlazOrbit.Charts.Enums;

/// <summary>
/// Binning strategy for <see cref="Components.BOBHistogramChart{T}"/>.
/// </summary>
public enum BOBHistogramBinRule
{
    /// <summary>Sturges' formula: <c>⌈log₂(n) + 1⌉</c> bins. Default.</summary>
    Sturges = 0,

    /// <summary>Scott's normal-reference rule (3.5σ/n^(1/3)).</summary>
    Scott = 1,

    /// <summary>Freedman-Diaconis rule (2·IQR/n^(1/3)) - outlier robust.</summary>
    FreedmanDiaconis = 2,

    /// <summary>Use the explicit <c>BinCount</c> parameter.</summary>
    FixedCount = 3
}