using BlazOrbit.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

/// <summary>
/// Single per-row action rendered as an icon-only button at the end of
/// each row / inside each card. Combine multiple actions in a list and
/// pass via <c>RowActions</c> on <see cref="BOBDataGrid{TItem}"/> /
/// <see cref="BOBDataCards{TItem}"/>.
/// <para>
/// Visibility and enabled state can both depend on the row item — pass
/// predicates to <see cref="Visible"/> / <see cref="Enabled"/> to support
/// per-row policies (e.g. hide "Delete" on read-only rows).
/// </para>
/// </summary>
/// <typeparam name="TItem">Row item type.</typeparam>
public sealed class DataCollectionRowAction<TItem>
{
    /// <summary>Accessible label exposed to screen readers + tooltip text.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Icon to display. When <see langword="null"/> the action falls back to the <see cref="Label"/> as button text.</summary>
    public IconKey? Icon { get; init; }

    /// <summary>
    /// Optional palette color for the icon — semantic shorthand
    /// (<c>PaletteColor.Error</c> for delete, <c>PaletteColor.Success</c>
    /// for approve, etc.). Default is the inherited theme color.
    /// </summary>
    public PaletteColor? Color { get; init; }

    /// <summary>Per-row visibility predicate. <see langword="null"/> means always visible.</summary>
    public Func<TItem, bool>? Visible { get; init; }

    /// <summary>Per-row enabled predicate. <see langword="null"/> means always enabled.</summary>
    public Func<TItem, bool>? Enabled { get; init; }

    /// <summary>Async handler invoked when the user activates the action.</summary>
    public Func<TItem, Task>? OnClick { get; init; }
}

/// <summary>
/// Single bulk action rendered in the toolbar when at least one row is
/// selected. Receives the current selection on activation. Use for
/// "Delete selected", "Export selected", "Approve all" workflows.
/// </summary>
/// <typeparam name="TItem">Row item type.</typeparam>
public sealed class DataCollectionBulkAction<TItem>
{
    /// <summary>Accessible label rendered as button text + ARIA label.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Optional leading icon.</summary>
    public IconKey? Icon { get; init; }

    /// <summary>Optional palette color (e.g. <c>Error</c> for destructive bulk actions).</summary>
    public PaletteColor? Color { get; init; }

    /// <summary>
    /// Visibility predicate evaluated against the current selection.
    /// <see langword="null"/> means always visible while at least one row
    /// is selected.
    /// </summary>
    public Func<IReadOnlyCollection<TItem>, bool>? Visible { get; init; }

    /// <summary>
    /// Enabled predicate. Receives the selection — return
    /// <see langword="false"/> to disable the button (e.g. when the
    /// action requires homogeneous types and the selection is mixed).
    /// </summary>
    public Func<IReadOnlyCollection<TItem>, bool>? Enabled { get; init; }

    /// <summary>Async handler invoked with the current selection.</summary>
    public Func<IReadOnlyCollection<TItem>, Task>? OnClick { get; init; }
}
