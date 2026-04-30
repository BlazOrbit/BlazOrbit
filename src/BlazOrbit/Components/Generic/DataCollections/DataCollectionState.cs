namespace BlazOrbit.Components;

/// <summary>
/// Maintains the state for data collection components including pagination, filtering, sorting, and selection.
/// </summary>
/// <typeparam name="TItem">The type of the items in the collection.</typeparam>
public sealed class DataCollectionState<TItem>
{
    private readonly HashSet<TItem> _selectedItems = [];

    /// <summary>
    /// The current page number.
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// The current filter text.
    /// </summary>
    public string FilterText { get; set; } = string.Empty;

    /// <summary>
    /// The number of items displayed per page.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// The set of currently selected items.
    /// </summary>
    public IReadOnlySet<TItem> SelectedItems => _selectedItems;

    /// <summary>
    /// The name of the column currently used for sorting.
    /// </summary>
    public string? SortColumn { get; set; }

    /// <summary>
    /// The current sort direction.
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.None;

    /// <summary>
    /// Clears all selected items.
    /// </summary>
    public void ClearSelection() => _selectedItems.Clear();

    /// <summary>
    /// Determines whether the specified item is selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns><see langword="true"/> if the item is selected; otherwise, <see langword="false"/>.</returns>
    public bool IsSelected(TItem item) => _selectedItems.Contains(item);

    /// <summary>
    /// Resets the current page to the first page.
    /// </summary>
    public void ResetPagination() => CurrentPage = 1;

    /// <summary>
    /// Selects all items in the specified collection.
    /// </summary>
    /// <param name="items">The items to select.</param>
    public void SelectAll(IEnumerable<TItem> items)
    {
        foreach (TItem item in items)
        {
            _selectedItems.Add(item);
        }
    }

    /// <summary>
    /// Toggles the selection state of the specified item.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    /// <param name="mode">The selection mode.</param>
    public void SelectItem(TItem item, SelectionMode mode)
    {
        if (mode == SelectionMode.Single)
        {
            _selectedItems.Clear();
        }

        if (_selectedItems.Contains(item))
        {
            _selectedItems.Remove(item);
        }
        else
        {
            _selectedItems.Add(item);
        }
    }

    /// <summary>
    /// Toggles the sort direction for the specified column.
    /// </summary>
    /// <param name="columnName">The name of the column to sort.</param>
    public void ToggleSort(string columnName)
    {
        if (SortColumn == columnName)
        {
            SortDirection = SortDirection switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.Ascending
            };

            if (SortDirection == SortDirection.None)
            {
                SortColumn = null;
            }
        }
        else
        {
            SortColumn = columnName;
            SortDirection = SortDirection.Ascending;
        }
    }
}
