namespace BlazOrbit.Components;

/// <summary>How selection toggles propagate across a tree.</summary>
public enum TreeSelectionCascade
{
    /// <summary>No propagation — selection stays on the toggled node.</summary>
    None,
    /// <summary>Selection cascades down to descendants.</summary>
    Children,
    /// <summary>Selection cascades up to ancestors.</summary>
    Parents,
    /// <summary>Selection cascades both up and down.</summary>
    Both
}