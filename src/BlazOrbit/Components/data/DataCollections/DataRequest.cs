namespace BlazOrbit.Components;

/// <summary>
/// Describes the slice of rows a data-collection component wants the
/// <see cref="IDataCollectionDataSource{TItem}"/> to materialise. The primary pair is
/// <see cref="StartIndex"/> + <see cref="Count"/> — they map naturally to
/// <c>IQueryable.Skip(StartIndex).Take(Count)</c> on server-side queries and to
/// <c>Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderRequest</c>
/// on the client side. Sources that prefer page-based semantics derive the page index
/// via <see cref="Page"/>.
/// </summary>
/// <param name="StartIndex">
/// 0-based row offset within the filtered/sorted result set. Always <c>0</c> when the
/// consumer has not enabled pagination on the host component.
/// </param>
/// <param name="Count">
/// Maximum number of rows to return starting at <see cref="StartIndex"/>.
/// <see langword="null"/> means "no upper bound" — typically used when pagination is
/// disabled and the source should return the full filtered/sorted set in a single call.
/// </param>
/// <param name="GlobalFilter">
/// Free-text search entered in the toolbar filter box. Empty / <see langword="null"/>
/// when the consumer has not enabled global filtering or the user cleared the box.
/// </param>
/// <param name="Sorts">
/// Active sort descriptors in priority order (primary first). Empty when no sort is
/// applied. Each entry carries the column header (resolved against
/// <c>BOBDataColumn{TItem}.Header</c>) and the direction.
/// </param>
/// <param name="ColumnFilters">
/// Per-column filter entries keyed by column header (case-insensitive). Combined with
/// <see cref="GlobalFilter"/> using AND semantics — a row must satisfy every entry.
/// Empty when the consumer has not enabled per-column filtering.
/// </param>
public sealed record DataRequest(
    int StartIndex,
    int? Count,
    string? GlobalFilter,
    IReadOnlyList<SortDescriptor> Sorts,
    IReadOnlyDictionary<string, ColumnFilterEntry> ColumnFilters)
{
    /// <summary>
    /// 1-based page index derived from <see cref="StartIndex"/> and <see cref="Count"/>.
    /// Useful for sources whose backend exposes page-based pagination (REST APIs with
    /// <c>?page=N</c>, Cosmos DB continuation tokens, etc.). Returns <c>1</c> when
    /// <see cref="Count"/> is <see langword="null"/> or zero (no pagination enabled).
    /// </summary>
    public int Page => Count is > 0 ? (StartIndex / Count.Value) + 1 : 1;
}
