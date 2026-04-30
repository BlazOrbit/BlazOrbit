namespace BlazOrbit.Components;

/// <summary>
/// Provides data for the filter changed event in data collection components.
/// </summary>
public sealed class DataCollectionFilterEventArgs : EventArgs
{
    /// <summary>
    /// The text used to filter the data collection.
    /// </summary>
    public required string FilterText { get; init; }
}

/// <summary>
/// Provides data for the page changed event in data collection components.
/// </summary>
public sealed class DataCollectionPageChangeEventArgs : EventArgs
{
    /// <summary>
    /// The current page number.
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public required int PageSize { get; init; }
}

/// <summary>
/// Provides data for the sort changed event in data collection components.
/// </summary>
public sealed class DataCollectionSortEventArgs : EventArgs
{
    /// <summary>
    /// The name of the column being sorted.
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// The direction of the sort.
    /// </summary>
    public required SortDirection Direction { get; init; }
}

/// <summary>
/// Provides data for the selection changed event in data collection components.
/// </summary>
/// <typeparam name="TItem">The type of the items in the collection.</typeparam>
public sealed class DataCollectionSelectionEventArgs<TItem> : EventArgs
{
    /// <summary>
    /// The set of currently selected items.
    /// </summary>
    public required IReadOnlySet<TItem> SelectedItems { get; init; }
}
