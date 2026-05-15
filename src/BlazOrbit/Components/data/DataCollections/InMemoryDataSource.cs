namespace BlazOrbit.Components;

/// <summary>
/// Wraps an in-process <see cref="IEnumerable{TItem}"/> behind the
/// <see cref="IDataCollectionDataSource{TItem}"/> contract. Used implicitly by
/// <see cref="BOBDataCollectionBase{TItem, TComponent, TVariant}"/> when the consumer
/// passes the legacy <c>Items</c> parameter — the component injects its column
/// registry and optional custom global filter via the internal hooks so the locally
/// applied pipeline matches the historical behaviour exactly.
/// <para>
/// Consumers building their own data-collection components can construct the source
/// directly to share the filter / sort / page semantics — the constructor accepts a
/// bare enumerable, the column metadata is wired separately when needed.
/// </para>
/// </summary>
/// <typeparam name="TItem">Row item type.</typeparam>
public sealed class InMemoryDataSource<TItem> : IDataCollectionDataSource<TItem>
{
    /// <summary>Constructs an in-memory source over <paramref name="items"/>.</summary>
    /// <param name="items">Backing collection. The source enumerates it on every <see cref="LoadAsync"/>.</param>
    public InMemoryDataSource(IEnumerable<TItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items = items;
    }

    /// <summary>Backing collection passed to the constructor.</summary>
    public IEnumerable<TItem> Items { get; }

    /// <summary>
    /// Column metadata used to resolve filter / sort descriptors against the row type.
    /// Wired by the host component when the source is consumed implicitly; left
    /// <see langword="null"/> when the source is used standalone (then filters / sorts
    /// pass through as no-ops because there are no <c>ValueSelector</c>s to invoke).
    /// </summary>
    internal IReadOnlyList<DataColumnRegistration<TItem>>? Columns { get; set; }

    /// <summary>
    /// Optional consumer-supplied global filter predicate. Mirrors the legacy
    /// <c>BOBDataCollectionBase.CustomFilter</c> parameter — when set, it drives the
    /// global filter match instead of the per-column substring fallback.
    /// </summary>
    internal Func<TItem, string, bool>? CustomGlobalFilter { get; set; }

    /// <inheritdoc />
    public Task<DataResult<TItem>> LoadAsync(DataRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<DataColumnRegistration<TItem>> columns = Columns ?? [];

        IEnumerable<TItem> filtered = DataCollectionPipeline.ApplyFilter(
            Items,
            request.GlobalFilter,
            request.ColumnFilters,
            columns,
            CustomGlobalFilter);

        // Materialise the filtered set once so we know the total count without
        // re-enumerating; downstream sort + paginate operate on the list view.
        List<TItem> filteredList = filtered.ToList();
        int total = filteredList.Count;

        IEnumerable<TItem> sorted = DataCollectionPipeline.ApplySort(filteredList, request.Sorts, columns);

        IEnumerable<TItem> paged = request.Count is int take and > 0
            ? sorted.Skip(request.StartIndex).Take(take)
            : sorted.Skip(request.StartIndex);

        return Task.FromResult(new DataResult<TItem>(paged.ToList(), total));
    }
}
