namespace BlazOrbit.Components;

/// <summary>
/// Strategy that materialises the rows shown by a data-collection component
/// (<c>BOBDataGrid</c>, <c>BOBDataCards</c>). Built-in implementations:
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="InMemoryDataSource{TItem}"/> wraps an in-process
///       <see cref="IEnumerable{TItem}"/> and runs the filter / sort / page pipeline
///       locally — the default, backwards-compatible path that mirrors the legacy
///       <c>Items</c> parameter.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="RemoteDataSource{TItem}"/> delegates each page request to a
///       user-supplied async provider — typical for server-side paging where the
///       database resolves filters / sorts and returns the matching slice plus the
///       total count.
///     </description>
///   </item>
/// </list>
/// Consumers can implement the interface directly for bespoke sources (SignalR
/// streams, GraphQL clients, hybrid caches).
/// </summary>
/// <typeparam name="TItem">Row item type.</typeparam>
public interface IDataCollectionDataSource<TItem>
{
    /// <summary>
    /// Loads the slice described by <paramref name="request"/>. Implementations honour
    /// the page / page size, global filter, per-column filters and sort descriptors,
    /// and return both the materialised page and the total row count after filtering
    /// (the host component uses the total to compute pagination — the slice itself
    /// only contains the visible page).
    /// </summary>
    /// <param name="request">User intent describing the desired slice.</param>
    /// <param name="cancellationToken">
    /// Token raised when a newer <see cref="LoadAsync"/> call supersedes this one
    /// (typing fast in a column filter, rapid pagination clicks). Long-running
    /// implementations should observe the token to avoid leaking work after the
    /// host component has moved on.
    /// </param>
    /// <returns>Page slice plus total filtered row count.</returns>
    Task<DataResult<TItem>> LoadAsync(DataRequest request, CancellationToken cancellationToken = default);
}
