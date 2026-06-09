using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Abstractions;

/// <summary>An option presented by a selection-capable component (dropdown, autocomplete, etc.).</summary>
public interface ISelectionOption
{
    /// <summary>Optional render fragment used in place of <see cref="DisplayText"/>.</summary>
    RenderFragment? Content { get; }

    /// <summary>Text shown in the list and in the selected-value chip.</summary>
    string DisplayText { get; }

    /// <summary>True when the option cannot be selected.</summary>
    bool IsDisabled { get; }

    /// <summary>Backing value carried into the bound model.</summary>
    object? Value { get; }
}

/// <summary>An option that can have nested child options (tree-style selection).</summary>
public interface IHierarchicalSelectionOption : ISelectionOption
{
    /// <summary>Direct children of this option.</summary>
    IReadOnlyList<IHierarchicalSelectionOption> Children { get; }

    /// <summary>Depth from the root (0 for top-level options).</summary>
    int Depth { get; }

    /// <summary>True when <see cref="Children"/> is non-empty.</summary>
    bool HasChildren { get; }

    /// <summary>Stable identifier within the tree.</summary>
    string Key { get; }

    /// <summary>Parent option, or null when this is a root.</summary>
    IHierarchicalSelectionOption? Parent { get; }
}
