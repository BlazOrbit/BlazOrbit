namespace BlazOrbit.Components.Internal;

/// <summary>
/// Visual modes for the internal <c>_BOBBtn</c> primitive. Drives the
/// <c>data-bob-variant</c> attribute so the scoped CSS can switch between the
/// solid color-shift treatment and the transparent ghost treatment used by
/// dismiss / close affordances across the library.
/// </summary>
public enum BOBBtnVariant
{
    /// <summary>Filled-text button with color shift on hover/active. The historical default.</summary>
    Default = 0,
    /// <summary>Transparent background with opacity dim until hover/focus — typical icon-only dismiss / close affordance.</summary>
    Ghost = 1,
}
