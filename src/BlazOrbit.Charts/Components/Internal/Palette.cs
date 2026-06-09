namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Default series-color palette. Charts pull colors from this list when the
/// consumer does not set <c>BOBChartSeries&lt;TX, TY&gt;.Color</c> explicitly,
/// indexed by series declaration order.
/// <para>
/// Colors are CSS custom-property references into the BlazOrbit palette so
/// they automatically follow the active theme (light / dark). Falls back to
/// hard-coded hex values if a host has not registered the BlazOrbit palette.
/// </para>
/// </summary>
internal static class Palette
{
    /// <summary>
    /// 8-color cycle. The first six slots reference BlazOrbit's canonical
    /// semantic tokens so palette changes propagate; slots 6-7 fall back
    /// to fixed hex values (no canonical "tertiary" / "neutral" tokens
    /// exist) and are only reached by charts with &gt; 6 series. Pass an
    /// explicit <c>BOBChartSeries.Color</c> to override per-series.
    /// </summary>
    private static readonly string[] _cycle =
    [
        "var(--palette-primary, #4f46e5)", "var(--palette-secondary, #ec4899)", "var(--palette-success, #10b981)",
        "var(--palette-warning, #f59e0b)", "var(--palette-error, #ef4444)", "var(--palette-info, #06b6d4)",
        "#8b5cf6", // violet - extension slot, no canonical token.
        "#64748b" // slate  - extension slot, no canonical token.
    ];

    /// <summary>Color for the n-th series, cycling when the count exceeds the palette.</summary>
    public static string ColorAt(int index) => _cycle[((index % _cycle.Length) + _cycle.Length) % _cycle.Length];
}