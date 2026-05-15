namespace BlazOrbit.Components;

/// <summary>
/// Page returned by an <see cref="IDataCollectionDataSource{TItem}"/> in response to a
/// <see cref="DataRequest"/>. Carries the materialised slice for the requested page plus
/// the total row count after filtering (used by the host component to compute
/// pagination — the slice itself only contains the visible page).
/// </summary>
/// <param name="Items">Materialised page of rows for the requested page index.</param>
/// <param name="TotalCount">
/// Total number of rows matching the request's filters before pagination. Drives the
/// pagination footer ("Page X of Y") and the live-region announcement.
/// </param>
/// <typeparam name="TItem">Row item type.</typeparam>
public sealed record DataResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount);
