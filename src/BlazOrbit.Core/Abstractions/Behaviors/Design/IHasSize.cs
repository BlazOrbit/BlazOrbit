namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a <see cref="BOBSize" /> parameter.
/// </summary>
public interface IHasSize
{
    /// <summary>Visual size of the component.</summary>
    BOBSize Size { get; set; }
}
