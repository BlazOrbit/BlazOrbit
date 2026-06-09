namespace BlazOrbit.Components;

/// <summary>
/// Bridge surfaced by <c>BOBChipGroup&lt;TValue&gt;</c> to nested <see cref="BOBChip"/>
/// children. Decouples the chip from the group's value generic so a single
/// <c>BOBChip</c> definition can participate in any group regardless of payload type.
/// </summary>
public interface IChipGroupContainer
{
    /// <summary>Selection cardinality (single vs multiple) enforced by the group.</summary>
    ChipGroupSelectionMode Mode { get; }

    /// <summary>When <see langword="true"/>, the group is disabled and chip clicks should no-op.</summary>
    bool IsGroupDisabled { get; }

    /// <summary>Returns whether <paramref name="value"/> is currently in the group's selection.</summary>
    bool IsValueSelected(object? value);

    /// <summary>Toggles <paramref name="value"/> in/out of the selection, honouring <see cref="Mode"/>.</summary>
    Task ToggleAsync(object? value);
}