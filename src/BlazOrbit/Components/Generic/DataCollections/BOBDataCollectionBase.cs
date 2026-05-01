using BlazOrbit.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Components;

/// <summary>
/// Shared base for data-collection components (grid, cards) that own column registration,
/// sorting, filtering, paging and selection state.
/// </summary>
/// <typeparam name="TItem">Row item type.</typeparam>
/// <typeparam name="TComponent">Concrete derived component type (CRTP).</typeparam>
/// <typeparam name="TVariant">Variant type owned by the derived component.</typeparam>
public abstract class BOBDataCollectionBase<TItem, TComponent, TVariant>
    : BOBVariantComponentBase<TComponent, TVariant>,
      IHasDensity,
      IHasSize,
      IHasShadow,
      IHasBorder,
      IHasBackgroundColor,
      IDataCollectionFamilyComponent
    where TComponent : BOBDataCollectionBase<TItem, TComponent, TVariant>
    where TVariant : Variant
{
    protected readonly DataColumnRegistry<TItem> ColumnRegistry = new();
    protected readonly DataCollectionState<TItem> State = new();
    protected List<DataColumnRegistration<TItem>> RegisteredColumns = [];
    protected List<DataColumnRegistration<TItem>> VisibleColumns = [];
    protected List<TItem> FilteredItems = [];
    protected List<TItem> ProcessedItems = [];
    protected bool ColumnsBuilt;
    protected bool PreventRowKeyDown;
    protected int TotalPages;
    protected int PaginationStart;
    protected int PaginationEnd;
    protected string? LiveRegionMessage;

    /// <summary>Column definitions rendered inside the data collection.</summary>
    [Parameter] public RenderFragment? Columns { get; set; }
    /// <summary>Data items to display.</summary>
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    /// <summary>Vertical density (gap) of rows.</summary>
    [Parameter] public BOBDensity Density { get; set; } = BOBDensity.Standard;
    /// <summary>Visual size of the data collection.</summary>
    [Parameter] public BOBSize Size { get; set; } = BOBSize.Medium;
    /// <summary>When <see langword="true" /> (default), rows highlight on hover.</summary>
    [Parameter] public bool Hoverable { get; set; } = true;
    /// <summary>Shadow style applied to the container.</summary>
    [Parameter] public ShadowStyle? Shadow { get; set; }

    /// <summary>Row selection mode.</summary>
    [Parameter] public SelectionMode SelectionMode { get; set; } = SelectionMode.None;
    /// <summary>Currently selected items. Use with two-way binding.</summary>
    [Parameter] public HashSet<TItem>? SelectedItems { get; set; }
    /// <summary>Raised when the selection changes.</summary>
    [Parameter] public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>When <see langword="true" />, column headers are clickable for sorting.</summary>
    [Parameter] public bool Sortable { get; set; }
    /// <summary>Name of the column to sort by on initial render.</summary>
    [Parameter] public string? DefaultSortColumn { get; set; }
    /// <summary>Initial sort direction. Defaults to <see cref="SortDirection.Ascending" />.</summary>
    [Parameter] public SortDirection? DefaultSortDirection { get; set; }

    /// <summary>When <see langword="true" />, a search box filters the displayed items.</summary>
    [Parameter] public bool Filterable { get; set; }
    /// <summary>Placeholder text for the filter search box.</summary>
    [Parameter] public string FilterPlaceholder { get; set; } = "Search...";
    /// <summary>Custom predicate used to filter items. Receives the item and the search text.</summary>
    [Parameter] public Func<TItem, string, bool>? CustomFilter { get; set; }

    /// <summary>Number of items per page. When <see langword="null" />, pagination is disabled.</summary>
    [Parameter] public int? PageSize { get; set; }
    /// <summary>Available page sizes shown in the page-size selector.</summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = [10, 20, 50, 100];
    /// <summary>When <see langword="true" />, a dropdown lets the user change the page size.</summary>
    [Parameter] public bool ShowPageSizeSelector { get; set; }

    /// <summary>When <see langword="true" />, only visible rows are rendered (improves performance for large lists).</summary>
    [Parameter] public bool EnableVirtualization { get; set; }
    /// <summary>Fixed height of the container. Required for virtualization and fixed header.</summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>Custom template rendered when there are no items to display.</summary>
    [Parameter] public RenderFragment? EmptyContent { get; set; }
    /// <summary>Custom template rendered while <see cref="Loading" /> is <see langword="true" />.</summary>
    [Parameter] public RenderFragment? LoadingContent { get; set; }
    /// <summary>When <see langword="true" />, the loading template is shown.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Border style applied to the container.</summary>
    [Parameter] public BorderStyle? Border { get; set; }
    /// <summary>Background color of the container. Accepts any valid CSS color value.</summary>
    [Parameter] public string? BackgroundColor { get; set; }

    /// <summary>Alternating row style pattern (e.g. zebra striping).</summary>
    [Parameter] public RowStylePattern? ItemPattern { get; set; }

    /// <summary>Raised when a row is clicked and <see cref="SelectionMode" /> is not active.</summary>
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }
    /// <summary>Raised when the sort column or direction changes.</summary>
    [Parameter] public EventCallback<DataCollectionSortEventArgs> OnSort { get; set; }
    /// <summary>Raised when the filter text changes.</summary>
    [Parameter] public EventCallback<DataCollectionFilterEventArgs> OnFilter { get; set; }
    /// <summary>Raised when the current page changes.</summary>
    [Parameter] public EventCallback<DataCollectionPageChangeEventArgs> OnPageChange { get; set; }

    protected bool IsInteractiveRow => OnRowClick.HasDelegate || SelectionMode != SelectionMode.None;

    protected bool UsePerItemPatternStyles
        => ItemPattern != null &&
        (!ItemPattern.IsCssExpressible || (EnableVirtualization && !string.IsNullOrEmpty(Height)));

    public override void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
    {
        base.BuildComponentDataAttributes(dataAttributes);

        if (Hoverable)
        {
            dataAttributes[FeatureDefinitions.DataAttributes.Hoverable] = "true";
        }

        if (ItemPattern != null && !UsePerItemPatternStyles)
        {
            string? patternAttr = ItemPattern.GetPatternDataAttribute();
            if (patternAttr != null)
            {
                dataAttributes[FeatureDefinitions.DataAttributes.RowPattern] = patternAttr;
            }
        }
    }

    public override void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
    {
        base.BuildComponentCssVariables(cssVariables);

        if (ItemPattern != null && !UsePerItemPatternStyles)
        {
            foreach (KeyValuePair<string, string> kv in ItemPattern.GetContainerCssVariables())
            {
                cssVariables[kv.Key] = kv.Value;
            }
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (PageSize.HasValue)
        {
            State.PageSize = PageSize.Value;
        }

        if (!string.IsNullOrEmpty(DefaultSortColumn))
        {
            State.SortColumn = DefaultSortColumn;
            State.SortDirection = DefaultSortDirection ?? SortDirection.Ascending;
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (SelectedItems != null)
        {
            State.ClearSelection();
            foreach (TItem item in SelectedItems)
            {
                State.SelectItem(item, SelectionMode.Multiple);
            }
        }

        ProcessData();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender && Columns != null && !ColumnsBuilt)
        {
            RegisteredColumns = ColumnRegistry.Columns.ToList();
            ColumnRegistry.Clear();
            ColumnsBuilt = true;
            ProcessData();
            StateHasChanged();
        }
    }

    protected void ProcessData()
    {
        if (!ColumnsBuilt || Items == null)
        {
            FilteredItems = [];
            ProcessedItems = [];
            VisibleColumns = [];
            LiveRegionMessage = null;
            return;
        }

        VisibleColumns = RegisteredColumns.Where(c => c.Visible).ToList();
        FilteredItems = ApplyFilter(Items).ToList();
        IEnumerable<TItem> sorted = ApplySort(FilteredItems);
        CalculatePaginationInfo();
        ProcessedItems = ApplyPagination(sorted).ToList();
        UpdateLiveRegionMessage();
    }

    protected void UpdateLiveRegionMessage()
    {
        if (!FilteredItems.Any())
        {
            LiveRegionMessage = "No data available.";
            return;
        }

        List<string> parts = [];

        if (!string.IsNullOrEmpty(State.SortColumn))
        {
            string direction = State.SortDirection == SortDirection.Ascending ? "ascending" : "descending";
            parts.Add($"Sorted by {State.SortColumn} ({direction})");
        }

        int filteredCount = FilteredItems.Count;
        int totalCount = Items?.Count() ?? 0;
        string rowWord = filteredCount == 1 ? "row" : "rows";

        if (!string.IsNullOrWhiteSpace(State.FilterText))
        {
            parts.Add($"{filteredCount} {rowWord} visible of {totalCount} total");
        }
        else
        {
            parts.Add($"{filteredCount} {rowWord}");
        }

        if (PageSize.HasValue && TotalPages > 0)
        {
            parts.Add($"Page {State.CurrentPage} of {TotalPages}");
        }

        LiveRegionMessage = string.Join(". ", parts) + ".";
    }

    protected IEnumerable<TItem> ApplyFilter(IEnumerable<TItem> items)
    {
        if (string.IsNullOrWhiteSpace(State.FilterText))
        {
            return items;
        }

        if (CustomFilter != null)
        {
            return items.Where(item => CustomFilter(item, State.FilterText));
        }

        string searchText = State.FilterText.ToLowerInvariant();
        List<DataColumnRegistration<TItem>> filterableColumns = RegisteredColumns
            .Where(c => c.Filterable && c.ValueSelector != null)
            .ToList();

        if (!filterableColumns.Any())
        {
            filterableColumns = RegisteredColumns.Where(c => c.ValueSelector != null).ToList();
        }

        return items.Where(item =>
            filterableColumns.Any(col =>
            {
                object? value = col.ValueSelector?.Invoke(item);
                return value?.ToString()?.ToLowerInvariant().Contains(searchText) ?? false;
            }));
    }

    protected IEnumerable<TItem> ApplySort(IEnumerable<TItem> items)
    {
        if (string.IsNullOrEmpty(State.SortColumn))
        {
            return items;
        }

        DataColumnRegistration<TItem>? column = RegisteredColumns.FirstOrDefault(c => c.Header == State.SortColumn);
        if (column?.ValueSelector == null)
        {
            return items;
        }

        if (column.CustomComparer != null)
        {
            List<TItem> list = items.ToList();
            Comparison<TItem> comparison = State.SortDirection == SortDirection.Ascending
                ? new Comparison<TItem>(column.CustomComparer)
                : (x, y) => column.CustomComparer(y, x);
            list.Sort(comparison);
            return list;
        }

        return State.SortDirection == SortDirection.Ascending
            ? items.OrderBy(item => column.ValueSelector(item))
            : items.OrderByDescending(item => column.ValueSelector(item));
    }

    protected IEnumerable<TItem> ApplyPagination(IEnumerable<TItem> items)
    {
        if (!PageSize.HasValue)
        {
            return items;
        }

        int skip = (State.CurrentPage - 1) * State.PageSize;
        return items.Skip(skip).Take(State.PageSize);
    }

    protected void CalculatePaginationInfo()
    {
        if (!PageSize.HasValue || !FilteredItems.Any())
        {
            TotalPages = 0;
            PaginationStart = 0;
            PaginationEnd = 0;
            return;
        }

        TotalPages = (int)Math.Ceiling(FilteredItems.Count / (double)State.PageSize);

        if (State.CurrentPage > TotalPages)
        {
            State.CurrentPage = TotalPages > 0 ? TotalPages : 1;
        }

        PaginationStart = ((State.CurrentPage - 1) * State.PageSize) + 1;
        PaginationEnd = Math.Min(State.CurrentPage * State.PageSize, FilteredItems.Count);
    }

    protected async Task HandleSort(DataColumnRegistration<TItem> column)
    {
        if (!column.Sortable || string.IsNullOrEmpty(column.Header))
        {
            return;
        }

        State.ToggleSort(column.Header);
        State.ResetPagination();
        ProcessData();

        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = column.Header,
                Direction = State.SortDirection
            });
        }
    }

    protected async Task HandleSortSelectChange(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            State.SortColumn = null;
            State.SortDirection = SortDirection.None;
        }
        else
        {
            State.SortColumn = value;
            if (State.SortDirection == SortDirection.None)
            {
                State.SortDirection = SortDirection.Ascending;
            }
        }

        State.ResetPagination();
        ProcessData();

        if (OnSort.HasDelegate && !string.IsNullOrEmpty(State.SortColumn))
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = State.SortColumn,
                Direction = State.SortDirection
            });
        }
    }

    protected async Task ToggleSortDirectionClicked(MouseEventArgs e)
    {
        State.SortDirection = State.SortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;
        ProcessData();

        if (OnSort.HasDelegate && !string.IsNullOrEmpty(State.SortColumn))
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = State.SortColumn,
                Direction = State.SortDirection
            });
        }
    }

    protected async Task HandleFilterChange(ChangeEventArgs e)
    {
        State.FilterText = e.Value?.ToString() ?? string.Empty;
        State.ResetPagination();
        ProcessData();

        if (OnFilter.HasDelegate)
        {
            await OnFilter.InvokeAsync(new DataCollectionFilterEventArgs
            {
                FilterText = State.FilterText
            });
        }
    }

    protected async Task HandleFilterInputChange(string? value) => await HandleFilterChange(new ChangeEventArgs { Value = value });

    protected async Task ClearFilterClicked(MouseEventArgs e) => ClearFilter();

    protected void ClearFilter()
    {
        State.FilterText = string.Empty;
        State.ResetPagination();
        ProcessData();
    }

    protected async Task HandlePageSizeSelectChange(string? value)
    {
        if (int.TryParse(value, out int size))
        {
            await HandlePageSizeChange(new ChangeEventArgs { Value = size });
        }
    }

    protected async Task HandleSelectRow(TItem item)
    {
        State.SelectItem(item, SelectionMode);
        await NotifySelectionChanged();
    }

    protected async Task HandleSelectAll(ChangeEventArgs e)
    {
        if (e.Value is bool isChecked && isChecked)
        {
            State.SelectAll(ProcessedItems);
        }
        else
        {
            State.ClearSelection();
        }

        await NotifySelectionChanged();
    }

    protected async Task ClearSelectionClicked(MouseEventArgs e) => await ClearSelection();

    protected async Task ClearSelection()
    {
        State.ClearSelection();
        await NotifySelectionChanged();
    }

    protected bool IsAllSelected() => ProcessedItems.Any() && ProcessedItems.All(State.IsSelected);

    protected async Task NotifySelectionChanged()
    {
        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync([.. State.SelectedItems]);
        }
    }

    protected async Task HandleRowClick(TItem item)
    {
        if (SelectionMode != SelectionMode.None)
        {
            await HandleSelectRow(item);
        }

        if (OnRowClick.HasDelegate)
        {
            await OnRowClick.InvokeAsync(item);
        }
    }

    protected async Task HandleRowKeyDown(KeyboardEventArgs e, TItem item)
    {
        PreventRowKeyDown = false;

        if (e.Key is "Enter" or " ")
        {
            PreventRowKeyDown = true;
            await HandleRowClick(item);
        }
    }

    protected async Task ChangePage(int page)
    {
        if (page < 1 || page > TotalPages)
        {
            return;
        }

        State.CurrentPage = page;
        ProcessData();

        if (OnPageChange.HasDelegate)
        {
            await OnPageChange.InvokeAsync(new DataCollectionPageChangeEventArgs
            {
                Page = page,
                PageSize = State.PageSize
            });
        }
    }

    protected async Task HandlePageSizeChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int newSize))
        {
            State.PageSize = newSize;
            State.ResetPagination();
            ProcessData();

            if (OnPageChange.HasDelegate)
            {
                await OnPageChange.InvokeAsync(new DataCollectionPageChangeEventArgs
                {
                    Page = State.CurrentPage,
                    PageSize = newSize
                });
            }
        }
    }

    protected string? GetItemPatternStyle(int index) => !UsePerItemPatternStyles ? null : (ItemPattern?.GetItemInlineStyle(index));

    protected IEnumerable<int> GetVisiblePages()
    {
        const int maxVisible = 5;
        int start = Math.Max(1, State.CurrentPage - (maxVisible / 2));
        int end = Math.Min(TotalPages, start + maxVisible - 1);

        if (end - start + 1 < maxVisible)
        {
            start = Math.Max(1, end - maxVisible + 1);
        }

        return Enumerable.Range(start, end - start + 1);
    }

    protected static string GetAlignClass(ColumnAlign align, string prefix) => align switch
    {
        ColumnAlign.Center => $"{prefix}--center",
        ColumnAlign.Right => $"{prefix}--right",
        _ => string.Empty
    };

    protected static string FormatValue(object? value, string? format)
    {
        return value == null
            ? string.Empty
            : !string.IsNullOrEmpty(format) && value is IFormattable formattable
            ? formattable.ToString(format, null)
            : value.ToString() ?? string.Empty;
    }

    protected string? GetAriaSort(DataColumnRegistration<TItem> col)
    {
        return !col.Sortable || State.SortColumn != col.Header
            ? null
            : State.SortDirection == SortDirection.Ascending ? "ascending" : "descending";
    }

    /// <summary>Sorts the collection by the given column header in the requested direction.</summary>
    public void SortBy(string columnName, SortDirection direction)
    {
        State.SortColumn = columnName;
        State.SortDirection = direction;
        ProcessData();
        StateHasChanged();
    }

    /// <summary>Applies a free-text filter and resets pagination to page 1.</summary>
    public void Filter(string filterText)
    {
        State.FilterText = filterText;
        State.ResetPagination();
        ProcessData();
        StateHasChanged();
    }

    /// <summary>Navigates to the given 1-based page index. Out-of-range values are ignored.</summary>
    public void GoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            State.CurrentPage = page;
            ProcessData();
            StateHasChanged();
        }
    }

    /// <summary>Returns the set of currently selected items.</summary>
    public IReadOnlySet<TItem> GetSelectedItems() => State.SelectedItems;
}