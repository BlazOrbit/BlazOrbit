using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Abstractions;

public interface ISelectionOption
{
    RenderFragment? Content { get; }
    string DisplayText { get; }
    bool IsDisabled { get; }
    object? Value { get; }
}

public interface IHierarchicalSelectionOption : ISelectionOption
{
    IReadOnlyList<IHierarchicalSelectionOption> Children { get; }
    int Depth { get; }
    bool HasChildren { get; }
    string Key { get; }
    IHierarchicalSelectionOption? Parent { get; }
}