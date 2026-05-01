namespace BlazOrbit.Components;

/// <summary>
/// Resolves an integer elevation level (0–24) to the Material Design 3 surface-tint percentage
/// that consuming components apply on top of <c>var(--palette-surface)</c> via <c>color-mix</c>.
///
/// <para>
/// The returned percentage is the <b>dark-mode</b> M3 curve (0/5/8/11/12/14, plateau-14% from
/// level 5 onwards). In light mode, components either gate the consumption of
/// <c>--bob-inline-elevation-tint</c> behind <c>[data-bob-theme="dark"]</c> selectors, or accept
/// that <c>color-mix(... 0%)</c> with the same value visually approximates light-mode behaviour
/// (no perceptible tint at low levels).
/// </para>
///
/// <para>
/// The tint <i>color</i> is decided by the CSS layer
/// (default: <c>var(--palette-surface-contrast)</c>); this class owns only the <i>amount</i>.
/// </para>
/// </summary>
public static class BOBElevationPresets
{
    /// <summary>
    /// Material Design 3 surface-tint percentage for the given elevation level.
    /// <list type="bullet">
    ///   <item><description><c>0</c> → 0%</description></item>
    ///   <item><description><c>1</c> → 5%</description></item>
    ///   <item><description><c>2</c> → 8%</description></item>
    ///   <item><description><c>3</c> → 11%</description></item>
    ///   <item><description><c>4</c> → 12%</description></item>
    ///   <item><description><c>5..24</c> → 14% (plateau)</description></item>
    /// </list>
    /// Levels outside <c>0..24</c> are clamped.
    /// </summary>
    public static int SurfaceTintPercent(int level)
    {
        level = Math.Clamp(level, 0, 24);
        return level switch
        {
            0 => 0,
            1 => 5,
            2 => 8,
            3 => 11,
            4 => 12,
            _ => 14,
        };
    }
}
