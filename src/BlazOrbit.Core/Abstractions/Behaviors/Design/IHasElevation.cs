namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports an <see cref="Elevation" /> parameter — a Material Design
/// elevation level (0–24) that resolves at render to (a) a derived <c>box-shadow</c> via
/// <see cref="BOBShadowPresets.Elevation(int, string?)"/> and (b) a surface-tint percentage
/// that consuming components can apply to lift the surface in dark mode.
///
/// <para>
/// <b>Precedence:</b> when a component implements both <see cref="IHasShadow"/> and
/// <see cref="IHasElevation"/>, an explicit <see cref="IHasShadow.Shadow"/> value wins for
/// <c>--bob-inline-shadow</c>; <see cref="Elevation"/> is still consulted to emit
/// <c>data-bob-elevation</c> and <c>--bob-inline-elevation-tint</c>.
/// </para>
/// </summary>
public interface IHasElevation
{
    /// <summary>
    /// Material Design elevation level. Clamped to <c>0..24</c> at render. <see langword="null"/>
    /// disables elevation entirely (no <c>data-bob-elevation</c>, no derived shadow, no tint var).
    /// </summary>
    int? Elevation { get; set; }
}
