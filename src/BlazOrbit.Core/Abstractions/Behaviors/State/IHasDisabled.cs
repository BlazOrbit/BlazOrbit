namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a disabled state.
/// </summary>
public interface IHasDisabled
{
    /// <summary>When <see langword="true" />, forces the disabled state.</summary>
    bool Disabled { get; set; }

    /// <summary>Computed disabled state, combining <see cref="Disabled" /> with internal conditions such as loading.</summary>
    bool IsDisabled { get; }
}