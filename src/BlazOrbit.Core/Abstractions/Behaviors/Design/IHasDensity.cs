namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a <see cref="BOBDensity" /> parameter.
/// </summary>
public interface IHasDensity
{
    /// <summary>Vertical density (gap) of the component.</summary>
    BOBDensity Density { get; set; }
}
