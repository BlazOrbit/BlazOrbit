namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports an active state.
/// </summary>
public interface IHasActive
{
    /// <summary>When <see langword="true" />, forces the active state.</summary>
    public bool Active { get; set; }

    /// <summary>Computed active state, combining <see cref="Active" /> with internal conditions.</summary>
    public bool IsActive { get; }
}
