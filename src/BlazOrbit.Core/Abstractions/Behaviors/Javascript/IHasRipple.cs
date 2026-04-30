using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a ripple effect via JavaScript interop.
/// </summary>
public interface IHasRipple : IJsBehavior
{
    /// <summary>When <see langword="true" />, disables the ripple effect.</summary>
    bool DisableRipple { get; set; }

    /// <summary>Custom ripple color. Accepts any valid CSS color value.</summary>
    string? RippleColor { get; set; }

    /// <summary>Duration of the ripple animation in milliseconds.</summary>
    int? RippleDurationMs { get; set; }

    /// <summary>Gets the <see cref="ElementReference" /> that hosts the ripple effect.</summary>
    ElementReference GetRippleContainer();
}
