namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports transition animations.
/// </summary>
public interface IHasTransitions
{
    /// <summary>Transition classes applied to the component.</summary>
    BOBTransitions? Transitions { get; set; }
}
