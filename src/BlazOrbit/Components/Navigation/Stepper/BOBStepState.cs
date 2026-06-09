namespace BlazOrbit.Components;

/// <summary>Visual state of a single <c>BOBStep</c> as resolved by the parent <c>BOBStepper</c>.</summary>
public enum BOBStepState
{
    /// <summary>Step lies ahead of the current position.</summary>
    Pending = 0,

    /// <summary>Step is the active position.</summary>
    Active = 1,

    /// <summary>Step lies behind the current position.</summary>
    Complete = 2,

    /// <summary>Step is flagged with an error (renders regardless of position).</summary>
    Error = 3
}