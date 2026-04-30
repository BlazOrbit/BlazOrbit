namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a loading state.
/// </summary>
public interface IHasLoading
{
    /// <summary>When <see langword="true" />, the component shows a loading indicator.</summary>
    bool Loading { get; set; }
}
