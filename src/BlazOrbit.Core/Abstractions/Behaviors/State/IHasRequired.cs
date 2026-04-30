namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a required state.
/// </summary>
public interface IHasRequired
{
    /// <summary>When <see langword="true" />, forces the required state.</summary>
    bool Required { get; }

    /// <summary>Computed required state.</summary>
    bool IsRequired { get; }
}
