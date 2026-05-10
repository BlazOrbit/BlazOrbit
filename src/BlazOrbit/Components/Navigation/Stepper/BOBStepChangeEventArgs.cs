namespace BlazOrbit.Components.Navigation;

/// <summary>
/// Mutable arguments raised by <c>BOBStepper.OnStepChange</c> before applying a step
/// transition. Set <see cref="Cancel"/> to <see langword="true"/> from a handler to keep
/// the active step at <see cref="From"/>.
/// </summary>
public sealed class BOBStepChangeEventArgs
{
    /// <summary>Index of the step the user is leaving.</summary>
    public int From { get; }
    /// <summary>Index of the step the user is moving to.</summary>
    public int To { get; }
    /// <summary>When set to <see langword="true"/>, the transition is aborted.</summary>
    public bool Cancel { get; set; }

    /// <summary>Initializes a new <see cref="BOBStepChangeEventArgs"/>.</summary>
    public BOBStepChangeEventArgs(int from, int to)
    {
        From = from;
        To = to;
    }
}
