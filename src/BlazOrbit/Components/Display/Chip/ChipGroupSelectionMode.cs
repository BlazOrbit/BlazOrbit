namespace BlazOrbit.Components;

/// <summary>
/// Selection cardinality enforced by <see cref="BOBChipGroup{TValue}"/>. Drives whether a
/// freshly-tapped chip clears any prior selection (<see cref="Single"/>) or coexists with
/// previously-selected chips (<see cref="Multiple"/>).
/// </summary>
public enum ChipGroupSelectionMode
{
    /// <summary>At most one chip selected at a time. Tapping the same chip again clears the selection.</summary>
    Single = 0,

    /// <summary>Any number of chips selected. Tapping toggles individual membership.</summary>
    Multiple = 1
}