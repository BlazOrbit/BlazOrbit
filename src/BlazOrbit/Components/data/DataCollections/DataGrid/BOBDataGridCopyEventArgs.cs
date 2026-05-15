namespace BlazOrbit.Components;

/// <summary>
/// Payload delivered to <see cref="BOBDataGrid{TItem}.OnCopy"/> after the grid has written
/// the snapshot to the system clipboard. The consumer typically uses
/// <see cref="Items"/>.Count for a confirmation toast, or inspects <see cref="Tsv"/> /
/// <see cref="Html"/> for logging and analytics.
/// </summary>
/// <typeparam name="TItem">Row item type.</typeparam>
public sealed class BOBDataGridCopyEventArgs<TItem>
{
    /// <summary>Snapshot of rows that were serialised — the current filtered + sorted view.</summary>
    public required IReadOnlyList<TItem> Items { get; init; }

    /// <summary>Tab-separated payload that was written as <c>text/plain</c>.</summary>
    public required string Tsv { get; init; }

    /// <summary>HTML <c>&lt;table&gt;</c> payload that was written as <c>text/html</c>.</summary>
    public required string Html { get; init; }
}
