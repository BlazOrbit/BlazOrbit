namespace BlazOrbit.Components;

/// <summary>
/// Delegates each <see cref="LoadAsync"/> call to a user-supplied async provider.
/// Used for server-side paging where the database (or a remote API) resolves the
/// filters / sorts / page and returns the matching slice plus the total filtered
/// count — the host component never sees the full set, so memory stays bounded
/// regardless of how many rows live behind the source.
/// </summary>
/// <typeparam name="TItem">Row item type.</typeparam>
/// <example>
/// <code language="csharp">
/// private readonly RemoteDataSource&lt;Order&gt; _orders = new(async (request, ct) =&gt;
/// {
///     OrdersResponse response = await api.QueryAsync(request, ct);
///     return new DataResult&lt;Order&gt;(response.Items, response.TotalCount);
/// });
/// </code>
/// </example>
public sealed class RemoteDataSource<TItem> : IDataCollectionDataSource<TItem>
{
    private readonly Func<DataRequest, CancellationToken, Task<DataResult<TItem>>> _provider;

    /// <summary>Constructs a remote source with a cancellation-aware provider.</summary>
    /// <param name="provider">
    /// Async loader invoked on every <see cref="LoadAsync"/>. The token raised when a
    /// newer call supersedes this one — long-running implementations should observe it.
    /// </param>
    public RemoteDataSource(Func<DataRequest, CancellationToken, Task<DataResult<TItem>>> provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    /// <summary>
    /// Convenience overload for providers that don't propagate cancellation.
    /// Internally adapts to the cancellation-aware delegate.
    /// </summary>
    /// <param name="provider">Async loader invoked on every <see cref="LoadAsync"/>.</param>
    public RemoteDataSource(Func<DataRequest, Task<DataResult<TItem>>> provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = (request, _) => provider(request);
    }

    /// <inheritdoc />
    public Task<DataResult<TItem>> LoadAsync(DataRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _provider(request, cancellationToken);
    }
}
