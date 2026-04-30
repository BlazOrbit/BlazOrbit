namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a <see cref="FullWidth" /> parameter.
/// </summary>
public interface IHasFullWidth
{
    /// <summary>When <see langword="true" />, the component spans the full width of its container.</summary>
    bool FullWidth { get; set; }
}
