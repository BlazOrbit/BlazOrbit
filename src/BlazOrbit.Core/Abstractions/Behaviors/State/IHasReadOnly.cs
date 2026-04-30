namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a read-only state.
/// </summary>
public interface IHasReadOnly
{
    /// <summary>When <see langword="true" />, forces the read-only state.</summary>
    bool ReadOnly { get; }

    /// <summary>Computed read-only state.</summary>
    bool IsReadOnly { get; }
}
