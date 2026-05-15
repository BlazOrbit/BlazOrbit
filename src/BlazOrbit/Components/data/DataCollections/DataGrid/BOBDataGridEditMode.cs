namespace BlazOrbit.Components;

/// <summary>
/// Inline-edit strategy for <see cref="BOBDataGrid{TItem}"/>. Drives when changes
/// flow back to the row item and which UI affordances appear in the toolbar.
/// </summary>
public enum BOBDataGridEditMode
{
    /// <summary>No inline editing. Cells render the read-only template (default).</summary>
    None = 0,

    /// <summary>
    /// Cell-level editing: double-click an editable cell to activate an input. The value
    /// is written back to the row item on blur or <c>Enter</c>; <c>Escape</c> cancels.
    /// Each cell is its own edit transaction — the grid raises <c>OnRowSave</c> after
    /// every successful save so the consumer can persist the change.
    /// </summary>
    Cell = 1,

    /// <summary>
    /// Batch editing: every change is staged in memory until the user clicks Save (apply
    /// to all rows) or Cancel (discard). Useful for forms-over-data scenarios where the
    /// user wants to review multiple edits before committing.
    /// </summary>
    Batch = 2
}
