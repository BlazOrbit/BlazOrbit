using BlazOrbit.Abstractions;
using BlazOrbit.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Localization;

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
        IPureBuiltComponent,
        IHasDensity,
        IHasSize,
        IHasShadow,
        IHasBorder,
        IHasBackgroundColor,
        IDataCollectionFamilyComponent,
        IDisposable
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

    // Row count returned by the most recent <c>DataSource.LoadAsync</c> call. Drives the
    // pagination footer + live-region announcement when running through the new
    // <see cref="DataSource"/> path; null when the component is using the legacy
    // <see cref="Items"/> pipeline (in which case <see cref="FilteredItems"/>.Count
    // remains authoritative).
    private int? _remoteTotalCount;

    // Cancels the previous in-flight load whenever a newer request supersedes it. Each
    // <see cref="ProcessDataAsync"/> call creates a fresh CTS, cancels the old one and
    // disposes it once the new token is in scope.
    private CancellationTokenSource? _loadCts;

    /// <summary>Column definitions rendered inside the data collection.</summary>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <summary>
    /// Data items to display. Used by the legacy in-memory pipeline; ignored when
    /// <see cref="DataSource"/> is non-<see langword="null"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Strategy that materialises rows. When set, drives the visible page via
    /// <see cref="IDataCollectionDataSource{TItem}.LoadAsync"/> — the host component
    /// translates its sort / filter / page state into a <see cref="DataRequest"/> and
    /// renders the returned <see cref="DataResult{TItem}.Items"/>. Takes precedence
    /// over <see cref="Items"/> when both are supplied.
    /// <para>
    /// Default <see langword="null"/> preserves the legacy <see cref="Items"/> path.
    /// Use <see cref="InMemoryDataSource{TItem}"/> for in-process collections you want
    /// to route through the new contract (e.g. for testing parity), and
    /// <see cref="RemoteDataSource{TItem}"/> for server-side paging.
    /// </para>
    /// </summary>
    [Parameter]
    public IDataCollectionDataSource<TItem>? DataSource { get; set; }

    /// <summary>Vertical density (gap) of rows.</summary>
    [Parameter]
    public BOBDensity Density { get; set; } = BOBDensity.Standard;

    /// <summary>Visual size of the data collection.</summary>
    [Parameter]
    public BOBSize Size { get; set; } = BOBSize.Medium;

    /// <summary>When <see langword="true" /> (default), rows highlight on hover.</summary>
    [Parameter]
    public bool Hoverable { get; set; } = true;

    /// <summary>Shadow style applied to the container.</summary>
    [Parameter]
    public ShadowStyle? Shadow { get; set; }

    /// <summary>Row selection mode.</summary>
    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

    /// <summary>Currently selected items. Use with two-way binding.</summary>
    [Parameter]
    public HashSet<TItem>? SelectedItems { get; set; }

    /// <summary>Raised when the selection changes.</summary>
    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>When <see langword="true" />, column headers are clickable for sorting.</summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>Name of the column to sort by on initial render.</summary>
    [Parameter]
    public string? DefaultSortColumn { get; set; }

    /// <summary>Initial sort direction. Defaults to <see cref="SortDirection.Ascending" />.</summary>
    [Parameter]
    public SortDirection DefaultSortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>Sort Append Behavior for multi-column sorting. Defaults to <see cref="SortAppendBehavior.Always" />. 
    /// Values are <see cref="SortAppendBehavior.None" />, <see cref="SortAppendBehavior.Always" />, <see cref="SortAppendBehavior.CtrlKey" />, <see cref="SortAppendBehavior.ShiftKey" />, 
    /// <see cref="SortAppendBehavior.CtrlOrShiftKey" />.</summary>
    [Parameter]
    public SortAppendBehavior SortAppendBehavior { get; set; } = SortAppendBehavior.Always;

    /// <summary>When <see langword="true" />, a search box filters the displayed items.</summary>
    [Parameter]
    public bool Filterable { get; set; }

    /// <summary>Placeholder text for the filter search box.</summary>
    [Parameter]
    public string FilterPlaceholder { get; set; } = "Search...";

    /// <summary>Localized placeholder when the caller does not override <see cref="FilterPlaceholder"/>.</summary>
    protected string ResolvedFilterPlaceholder =>
        FilterPlaceholder == "Search..." ? Localizer["Search..."] : FilterPlaceholder;

    /// <summary>Custom predicate used to filter items. Receives the item and the search text.</summary>
    [Parameter]
    public Func<TItem, string, bool>? CustomFilter { get; set; }

    /// <summary>
    /// When <see langword="true" />, render a per-column text filter input below the
    /// header label for every <see cref="BOBDataColumn{TItem}"/> with
    /// <c>Filterable=true</c>. Per-column filters AND-combine with the toolbar
    /// <see cref="Filterable"/> search box. Default <see langword="false"/> for
    /// backwards-compat.
    /// </summary>
    [Parameter]
    public bool ShowColumnFilters { get; set; }

    /// <summary>
    /// When <see langword="true"/>, headers receive <c>tabindex="0"</c> + an
    /// <c>Alt+ArrowLeft</c> / <c>Alt+ArrowRight</c> keyboard shortcut that shifts the
    /// focused column toward the start or end of the row. Default <see langword="false"/>
    /// for backwards-compat. Drag-based reorder is a separate slice (I1.S4 follow-up).
    /// </summary>
    [Parameter]
    public bool Reorderable { get; set; }

    /// <summary>
    /// When <see langword="true"/>, columns whose <c>Resizable</c> parameter is on render
    /// a drag handle on their trailing edge so the user can resize them via pointer
    /// events. The drag clamps to each column's <c>MinWidth</c> / <c>MaxWidth</c> and
    /// writes the new width to <see cref="DataCollectionState{TItem}.ColumnWidths"/>;
    /// the value participates in the persistence round-trip.
    /// </summary>
    [Parameter]
    public bool Resizable { get; set; }

    /// <summary>
    /// Optional storage key for the registered <see cref="IDataCollectionStatePersistence"/>.
    /// When set, the persistable slice of <see cref="State"/> (filter / sort / page /
    /// column order / column filters) is loaded on first render and saved after every
    /// state mutation. Default <see langword="null"/> = no persistence (the registered
    /// service is only consulted when this is non-empty).
    /// </summary>
    [Parameter]
    public string? PersistenceKey { get; set; }

    /// <summary>
    /// Persistence strategy resolved from DI. Default registration is
    /// <see cref="NullStatePersistence"/>; consumers opt in to <c>localStorage</c>
    /// persistence by registering <see cref="LocalStorageStatePersistence"/> on top.
    /// </summary>
    [Inject]
    internal IDataCollectionStatePersistence StatePersistence { get; set; } = default!;

    /// <summary>Localized strings for data-collection UI text (filter placeholders, aria-labels, live-region messages).</summary>
    [Inject]
    protected IStringLocalizer<BOBDataCollectionResources> Localizer { get; set; } = default!;

    private bool _persistenceLoaded;

    /// <summary>Number of items per page. When <see langword="null" />, pagination is disabled.</summary>
    [Parameter]
    public int? PageSize { get; set; }

    /// <summary>Available page sizes shown in the page-size selector.</summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = [10, 20, 50, 100];

    /// <summary>When <see langword="true" />, a dropdown lets the user change the page size.</summary>
    [Parameter]
    public bool ShowPageSizeSelector { get; set; }

    /// <summary>When <see langword="true" />, only visible rows are rendered (improves performance for large lists).</summary>
    [Parameter]
    public bool EnableVirtualization { get; set; }

    /// <summary>Fixed height of the container. Required for virtualization and fixed header.</summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>Custom template rendered when there are no items to display.</summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>Custom template rendered while <see cref="Loading" /> is <see langword="true" />.</summary>
    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    /// <summary>When <see langword="true" />, the loading template is shown.</summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>Border style applied to the container.</summary>
    [Parameter]
    public BorderStyle? Border { get; set; }

    /// <summary>Background color of the container. Accepts any valid CSS color value, <see cref="PaletteColor"/> or <see cref="BOBColor"/>.</summary>
    [Parameter]
    public string? BackgroundColor { get; set; }

    /// <summary>Alternating row style pattern (e.g. zebra striping).</summary>
    [Parameter]
    public RowStylePattern? ItemPattern { get; set; }

    /// <summary>
    /// Per-row action buttons rendered in a sticky-right action column
    /// (grid) or as an icon strip on each card. Clicks on action buttons
    /// stop propagation, so the row click only fires for non-action
    /// targets.
    /// </summary>
    [Parameter]
    public IReadOnlyList<DataCollectionRowAction<TItem>>? RowActions { get; set; }

    /// <summary>
    /// Bulk actions rendered in the toolbar when at least one row is
    /// selected. Pair with <see cref="SelectionMode"/> = Multiple to
    /// expose mass-edit / delete / export workflows.
    /// </summary>
    [Parameter]
    public IReadOnlyList<DataCollectionBulkAction<TItem>>? BulkActions { get; set; }

    /// <summary>
    /// Visual mode used while <see cref="Loading"/> is on. Default
    /// <see cref="LoadingMode.Spinner"/> — switch to
    /// <see cref="LoadingMode.Skeleton"/> for animated row placeholders.
    /// </summary>
    [Parameter]
    public LoadingMode LoadingMode { get; set; } = LoadingMode.Spinner;

    /// <summary>
    /// Optional error template rendered in place of the data when
    /// <see cref="Error"/> is non-empty. Useful for surfacing remote-load
    /// failures without leaving an empty grid.
    /// </summary>
    [Parameter]
    public RenderFragment? ErrorContent { get; set; }

    /// <summary>
    /// Error message. When non-empty (and <see cref="Loading"/> is off)
    /// the data area renders <see cref="ErrorContent"/> or a default
    /// error state instead of the rows.
    /// </summary>
    [Parameter]
    public string? Error { get; set; }

    /// <summary>
    /// Optional CTA template rendered alongside the empty state — pair
    /// with <see cref="EmptyContent"/> for "Create first record" buttons
    /// without rewriting the entire empty layout.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyActionTemplate { get; set; }

    /// <summary>
    /// Optional master-detail template. When non-null, every row gets a
    /// chevron toggle that expands a sub-row containing the rendered
    /// fragment. Useful for showing related data (order lines, audit
    /// trail) without navigation.
    /// <para>
    /// With <see cref="EnableVirtualization"/> the variable row height
    /// breaks scroll-position math; pin a fixed <c>ItemSize</c> on
    /// <c>&lt;Virtualize&gt;</c> when expansion is on.
    /// </para>
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? RowDetailTemplate { get; set; }

    /// <summary>Per-instance set of expanded items. Persists across re-renders.</summary>
    private protected readonly HashSet<TItem> ExpandedItems = [];

    /// <summary>True iff <see cref="RowDetailTemplate"/> was supplied.</summary>
    private protected bool HasRowDetail => RowDetailTemplate is not null;

    /// <summary>Returns whether the given item is currently expanded.</summary>
    private protected bool IsExpanded(TItem item) => ExpandedItems.Contains(item);

    /// <summary>Toggle a row's expanded state and re-render.</summary>
    private protected void ToggleExpanded(TItem item)
    {
        if (!ExpandedItems.Add(item))
        {
            ExpandedItems.Remove(item);
        }
    }

    /// <summary>Raised when a row is clicked and <see cref="SelectionMode" /> is not active.</summary>
    [Parameter]
    public EventCallback<TItem> OnRowClick { get; set; }

    /// <summary>Raised when the sort column or direction changes.</summary>
    [Parameter]
    public EventCallback<DataCollectionSortEventArgs> OnSort { get; set; }

    /// <summary>Raised when the filter text changes.</summary>
    [Parameter]
    public EventCallback<DataCollectionFilterEventArgs> OnFilter { get; set; }

    /// <summary>Raised when the current page changes.</summary>
    [Parameter]
    public EventCallback<DataCollectionPageChangeEventArgs> OnPageChange { get; set; }

    protected bool IsInteractiveRow => OnRowClick.HasDelegate || SelectionMode != SelectionMode.None;

    protected bool UsePerItemPatternStyles
        => ItemPattern != null &&
           (!ItemPattern.IsCssExpressible || (EnableVirtualization && !string.IsNullOrEmpty(Height)));

    public virtual void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
    {
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

    public virtual void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
    {
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
            State.SortDirection = DefaultSortDirection;
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

        // Load persisted state on the first render after columns are wired so the rehydrated
        // ColumnOrder / ColumnFilters resolve against an established registry.
        if (firstRender && ColumnsBuilt && !_persistenceLoaded && !string.IsNullOrEmpty(PersistenceKey))
        {
            _persistenceLoaded = true;
            BOBAsyncHelper.SafeFireAndForget(LoadPersistedStateAsync);
        }
    }

    private async Task LoadPersistedStateAsync()
    {
        string? json = await StatePersistence.LoadAsync(PersistenceKey!);
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        State.LoadFromJson(json);
        ProcessData();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Fires the persistence pipeline. Wired into every state mutation handler — filter /
    /// sort / pagination / reorder / column filter — so the next reload restores the
    /// user's view. No-op when <see cref="PersistenceKey"/> is unset.
    /// </summary>
    protected void PersistState()
    {
        if (string.IsNullOrEmpty(PersistenceKey))
        {
            return;
        }

        string payload = State.ToJson();
        BOBAsyncHelper.SafeFireAndForget(() => StatePersistence.SaveAsync(PersistenceKey, payload).AsTask());
    }

    protected void ProcessData()
    {
        if (!ColumnsBuilt)
        {
            FilteredItems = [];
            ProcessedItems = [];
            VisibleColumns = [];
            LiveRegionMessage = null;
            return;
        }

        // Rebuild the visible-column slice from the registry in both paths so the header
        // row is correct before the data slice (if any) arrives.
        VisibleColumns = RegisteredColumns.Where(c => c.Visible).ToList();

        // Apply the user-driven column reorder when present. Columns not mentioned in
        // State.ColumnOrder slot in afterwards in their registration order so newly added
        // columns become visible without a forced reorder.
        if (State.ColumnOrder.Count > 0)
        {
            Dictionary<string, int> rank = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < State.ColumnOrder.Count; i++)
            {
                rank[State.ColumnOrder[i]] = i;
            }

            VisibleColumns = VisibleColumns
                .OrderBy(c => rank.TryGetValue(c.Header ?? string.Empty, out int r) ? r : int.MaxValue)
                .ToList();
        }

        // Async path: a DataSource takes precedence over Items. The work is fire-and-forget
        // because the entry points (HandleSort, HandleFilter, …) are sync — the helper
        // swallows the canonical teardown exceptions and re-renders on completion.
        if (DataSource is not null)
        {
            // ItemsProvider mode: the Virtualize component owns slicing. Filter/sort
            // mutations refresh through its API instead of round-tripping LoadAsync here.
            if (UsesItemsProvider)
            {
                BOBAsyncHelper.SafeFireAndForget(RefreshVirtualizedAsync);
                return;
            }

            BOBAsyncHelper.SafeFireAndForget(ProcessDataAsync);
            return;
        }

        // Legacy sync path — driven by the Items parameter.
        if (Items == null)
        {
            FilteredItems = [];
            ProcessedItems = [];
            _remoteTotalCount = null;
            LiveRegionMessage = null;
            return;
        }

        _remoteTotalCount = null;
        FilteredItems = ApplyFilter(Items).ToList();
        IEnumerable<TItem> sorted = ApplySort(FilteredItems);
        CalculatePaginationInfo();
        ProcessedItems = ApplyPagination(sorted).ToList();
        UpdateLiveRegionMessage();

        // Persist any state mutation now that the result is materialised. Gated on
        // _persistenceLoaded so the initial render — which fires ProcessData before the
        // saved payload has had a chance to land — doesn't overwrite the stored entry
        // with an empty default.
        if (_persistenceLoaded && !string.IsNullOrEmpty(PersistenceKey))
        {
            PersistState();
        }
    }

    /// <summary>
    /// <see langword="true"/> when the host should hand row materialisation to a
    /// <see cref="Virtualize{TItem}"/> ItemsProvider (lazy infinite scroll) instead of
    /// the explicit pagination footer. Triggered by <see cref="DataSource"/> +
    /// <see cref="EnableVirtualization"/> + a non-empty <see cref="Height"/> — the height
    /// is required for Virtualize to compute the scroll viewport.
    /// </summary>
    protected bool UsesItemsProvider =>
        DataSource is not null && EnableVirtualization && !string.IsNullOrEmpty(Height);

    /// <summary>
    /// ItemsProvider callback wired to <see cref="Virtualize{TItem}.ItemsProvider"/>
    /// when <see cref="UsesItemsProvider"/> is on. Translates the viewport-driven
    /// request (<paramref name="request"/>) into a <see cref="DataRequest"/> and forwards
    /// to <see cref="DataSource"/>. The pagination footer is hidden in this mode —
    /// Virtualize owns slicing.
    /// </summary>
    /// <param name="request">Range + cancellation token supplied by Virtualize.</param>
    /// <returns>Materialised slice plus total filtered row count for scroll-height math.</returns>
    protected async ValueTask<ItemsProviderResult<TItem>> ProvideVirtualizedItemsAsync(ItemsProviderRequest request)
    {
        if (DataSource is null)
        {
            return new ItemsProviderResult<TItem>([], 0);
        }

        if (DataSource is InMemoryDataSource<TItem> inMemory)
        {
            inMemory.Columns = RegisteredColumns;
            inMemory.CustomGlobalFilter = CustomFilter;
        }

        DataRequest req = new(
            request.StartIndex,
            request.Count,
            State.FilterText,
            [.. State.SortDescriptors],
            State.ColumnFilters.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase));

        DataResult<TItem> result;
        try
        {
            result = await DataSource.LoadAsync(req, request.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            return new ItemsProviderResult<TItem>([], 0);
        }

        // Mirror the result into FilteredItems / ProcessedItems so aggregates + live region
        // see the same slice the user is scrolling through (page-window only — server-side
        // aggregation remains a documented limitation).
        FilteredItems = [.. result.Items];
        ProcessedItems = [.. result.Items];
        _remoteTotalCount = result.TotalCount;
        UpdateLiveRegionMessage();

        return new ItemsProviderResult<TItem>(result.Items, result.TotalCount);
    }

    /// <summary>
    /// Reference to the Virtualize component when running in ItemsProvider mode — derived
    /// components wire it via <c>@ref</c> so the base can call
    /// <c>RefreshDataAsync()</c> after filter / sort / column-filter mutations.
    /// </summary>
    protected Virtualize<TItem>? VirtualizeRef { get; set; }

    /// <summary>
    /// Tells Virtualize to drop its cache and re-request the visible window. Called from
    /// <see cref="ProcessData"/> whenever the user changes filters or sorts while in
    /// ItemsProvider mode — without this, Virtualize would keep rendering the stale
    /// slice from before the mutation.
    /// </summary>
    private async Task RefreshVirtualizedAsync()
    {
        if (VirtualizeRef is null)
        {
            return;
        }

        try
        {
            await VirtualizeRef.RefreshDataAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException) { /* circuit teardown */ }
        catch (ObjectDisposedException) { /* component disposed mid-refresh */ }
    }

    /// <summary>
    /// Async pipeline driven by the <see cref="DataSource"/> parameter. Each invocation
    /// renews the cancellation token so the latest call wins when the user types fast or
    /// rapidly clicks through pages; superseded calls observe their token and return
    /// without touching the rendered state.
    /// </summary>
    private async Task ProcessDataAsync()
    {
        if (DataSource is null)
        {
            return;
        }

        // Cancel the previous in-flight load; the latest call wins.
        CancellationTokenSource? previous = _loadCts;
        CancellationTokenSource current = new();
        _loadCts = current;
        if (previous is not null)
        {
            try { previous.Cancel(); }
            catch (ObjectDisposedException) { /* race with Dispose */ }
            previous.Dispose();
        }

        // InMemoryDataSource needs the column registry + custom global filter to apply
        // the local pipeline correctly; wire them once per call. RemoteDataSource owns its
        // pipeline server-side and ignores these hooks.
        if (DataSource is InMemoryDataSource<TItem> inMemory)
        {
            inMemory.Columns = RegisteredColumns;
            inMemory.CustomGlobalFilter = CustomFilter;
        }

        // Translate the pagination UI state (1-based Page + PageSize) into the
        // StartIndex/Count pair the DataSource contract expects. Sources that prefer
        // page-based semantics can still read DataRequest.Page directly.
        int startIndex = PageSize.HasValue ? (State.CurrentPage - 1) * State.PageSize : 0;
        int? count = PageSize.HasValue ? State.PageSize : null;

        DataRequest request = new(
            startIndex,
            count,
            State.FilterText,
            [.. State.SortDescriptors],
            State.ColumnFilters.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase));

        DataResult<TItem> result;
        try
        {
            result = await DataSource.LoadAsync(request, current.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (current.IsCancellationRequested)
        {
            return;
        }

        // Remote sources return the visible page + total count. Surface both:
        // FilteredItems holds the page slice (aggregates that depend on the full filtered
        // set therefore reflect the page only — server-side aggregation is a documented
        // follow-up for the next slice), TotalCount drives pagination math.
        FilteredItems = [.. result.Items];
        ProcessedItems = [.. result.Items];
        _remoteTotalCount = result.TotalCount;
        CalculatePaginationInfo();
        UpdateLiveRegionMessage();

        if (_persistenceLoaded && !string.IsNullOrEmpty(PersistenceKey))
        {
            PersistState();
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Cancels and disposes the pending data-source load. Derived components that need
    /// their own teardown should override <see cref="Dispose(bool)"/> and chain
    /// <c>base.Dispose(disposing)</c>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposal hook for derived components. The base implementation cancels and disposes
    /// the in-flight <see cref="DataSource"/> load token.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>; <see langword="false"/> from a finaliser.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        try
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
        }
        catch (ObjectDisposedException) { /* idempotent */ }
        _loadCts = null;
    }

    protected void UpdateLiveRegionMessage()
    {
        // Remote mode: trust the server's total over the page slice — FilteredItems only
        // holds the visible page, so its Count would announce one page instead of the full
        // filtered set.
        int filteredCount = _remoteTotalCount ?? FilteredItems.Count;
        if (filteredCount == 0)
        {
            LiveRegionMessage = Localizer["No data available."];
            return;
        }

        List<string> parts = [];

        if (!string.IsNullOrEmpty(State.SortColumn))
        {
            string direction = State.SortDirection == SortDirection.Ascending
                ? Localizer["ascending"]
                : Localizer["descending"];
            parts.Add(string.Format(Localizer["Sorted by {0} ({1})"], State.SortColumn, direction));
        }

        int totalCount = _remoteTotalCount ?? Items?.Count() ?? 0;
        string rowWord = filteredCount == 1
            ? Localizer["row"]
            : Localizer["rows"];

        if (!string.IsNullOrWhiteSpace(State.FilterText))
        {
            parts.Add(string.Format(Localizer["{0} {1} visible of {2} total"], filteredCount, rowWord, totalCount));
        }
        else
        {
            parts.Add($"{filteredCount} {rowWord}");
        }

        if (PageSize.HasValue && TotalPages > 0)
        {
            parts.Add(string.Format(Localizer["Page {0} of {1}"], State.CurrentPage, TotalPages));
        }

        LiveRegionMessage = string.Join(". ", parts) + ".";
    }

    /// <summary>Returns the localized lower-case label for an aggregate function (e.g. "sum", "average").</summary>
    protected string GetAggregateLabel(AggregateFunction function) => function switch
    {
        AggregateFunction.Count => Localizer["count"],
        AggregateFunction.Sum => Localizer["sum"],
        AggregateFunction.Average => Localizer["average"],
        AggregateFunction.Min => Localizer["min"],
        AggregateFunction.Max => Localizer["max"],
        _ => function.ToString().ToLowerInvariant()
    };

    /// <summary>
    /// Applies global + per-column filters using the shared pipeline. Delegates to
    /// <see cref="DataCollectionPipeline"/> so the legacy <c>Items</c> path and the
    /// <see cref="InMemoryDataSource{TItem}"/> wrapper agree on filter semantics.
    /// </summary>
    protected IEnumerable<TItem> ApplyFilter(IEnumerable<TItem> items) =>
        DataCollectionPipeline.ApplyFilter(items, State.FilterText, State.ColumnFilters, RegisteredColumns, CustomFilter);

    /// <summary>
    /// Multi-column sort using the shared pipeline. Honours per-column custom comparers
    /// then falls back to <c>ValueSelector</c>. Returns the input unchanged when no
    /// descriptors are active.
    /// </summary>
    protected IEnumerable<TItem> ApplySort(IEnumerable<TItem> items) =>
        DataCollectionPipeline.ApplySort(items, State.SortDescriptors, RegisteredColumns);

    /// <summary>
    /// Returns the slice for the current page using the shared pipeline. No-op when
    /// pagination is disabled (<see cref="PageSize"/> is <see langword="null"/>).
    /// </summary>
    protected IEnumerable<TItem> ApplyPagination(IEnumerable<TItem> items) =>
        PageSize.HasValue
            ? DataCollectionPipeline.ApplyPagination(items, State.CurrentPage, State.PageSize)
            : items;

    protected void CalculatePaginationInfo()
    {
        // Remote mode uses the server-reported total — FilteredItems only holds the page
        // slice so its Count under-reports the universe. Legacy mode keeps the historical
        // FilteredItems.Count semantics.
        int total = _remoteTotalCount ?? FilteredItems.Count;

        if (!PageSize.HasValue || total == 0)
        {
            TotalPages = 0;
            PaginationStart = 0;
            PaginationEnd = 0;
            return;
        }

        TotalPages = (int)Math.Ceiling(total / (double)State.PageSize);

        if (State.CurrentPage > TotalPages)
        {
            State.CurrentPage = TotalPages > 0 ? TotalPages : 1;
        }

        PaginationStart = ((State.CurrentPage - 1) * State.PageSize) + 1;
        PaginationEnd = Math.Min(State.CurrentPage * State.PageSize, total);
    }

    protected Task HandleSort(DataColumnRegistration<TItem> column) => HandleSort(column, false);

    /// <summary>
    /// Toggle the sort state for the given column. <paramref name="append"/>
    /// = <see langword="true"/> stacks the sort instead of replacing it
    /// (Shift+Click on the header). The notification fires with the new
    /// primary sort.
    /// </summary>
    protected async Task HandleSort(DataColumnRegistration<TItem> column, bool append)
    {
        if (!column.Sortable || string.IsNullOrEmpty(column.Header))
        {
            return;
        }

        State.ToggleSort(column.Header, append);
        State.ResetPagination();
        ProcessData();

        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = State.SortColumn ?? string.Empty, Direction = State.SortDirection
            });
        }
    }

    /// <summary>
    /// Wrapper invoked from sort buttons that exposes the click args so
    /// <c>shiftKey</c> can drive multi-column sort. Falls back to the
    /// single-column toggle when Shift is not pressed.
    /// </summary>
    protected Task HandleSortClick(DataColumnRegistration<TItem> column, MouseEventArgs e)
    {
        bool append = SortAppendBehavior switch
        {
            SortAppendBehavior.None => false,
            SortAppendBehavior.Always => true,
            SortAppendBehavior.CtrlKey => e.CtrlKey,
            SortAppendBehavior.ShiftKey => e.ShiftKey,
            SortAppendBehavior.CtrlOrShiftKey => e.CtrlKey || e.ShiftKey,
            _ => false
        };
        return HandleSort(column, append);
    }

    /// <summary>Returns the priority (1-based) of the column in the multi-sort list, or null if not sorted.</summary>
    private protected int? GetSortPriority(DataColumnRegistration<TItem> column)
    {
        if (string.IsNullOrEmpty(column.Header))
        {
            return null;
        }

        if (State.SortDescriptors.Count <= 1)
        {
            return null;
        }

        for (int i = 0; i < State.SortDescriptors.Count; i++)
        {
            if (State.SortDescriptors[i].ColumnName == column.Header)
            {
                return i + 1;
            }
        }

        return null;
    }

    /// <summary>Returns the active sort direction for a column, or <see cref="SortDirection.None"/> if not sorted.</summary>
    private protected SortDirection GetSortDirection(DataColumnRegistration<TItem> column)
    {
        if (string.IsNullOrEmpty(column.Header))
        {
            return SortDirection.None;
        }

        foreach (SortDescriptor d in State.SortDescriptors)
        {
            if (d.ColumnName == column.Header)
            {
                return d.Direction;
            }
        }

        return SortDirection.None;
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
                ColumnName = State.SortColumn, Direction = State.SortDirection
            });
        }
    }

    protected async Task ToggleSortDirectionClicked()
    {
        State.SortDirection = State.SortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;
        ProcessData();

        if (OnSort.HasDelegate && !string.IsNullOrEmpty(State.SortColumn))
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = State.SortColumn, Direction = State.SortDirection
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
            await OnFilter.InvokeAsync(new DataCollectionFilterEventArgs { FilterText = State.FilterText });
        }
    }

    protected async Task HandleFilterInputChange(string? value) =>
        await HandleFilterChange(new ChangeEventArgs { Value = value });

    /// <summary>
    /// Handler wired to the per-column filter input rendered in the data grid header
    /// when <see cref="ShowColumnFilters"/> is on. Updates <see cref="DataCollectionState{TItem}.ColumnFilters"/>,
    /// resets pagination, and re-runs the filter pipeline.
    /// </summary>
    protected async Task HandleColumnFilterInput(string columnName, ChangeEventArgs e)
    {
        string text = e.Value?.ToString() ?? string.Empty;
        // Seed the entry's Mode + Operator default from the column registration on first
        // mutation. Subsequent edits keep the user's chosen operator (handled by
        // SetColumnFilter(text) which preserves the operator/mode pair).
        if (!State.ColumnFilters.ContainsKey(columnName))
        {
            DataColumnRegistration<TItem>? column = RegisteredColumns
                .FirstOrDefault(c => string.Equals(c.Header, columnName, StringComparison.OrdinalIgnoreCase));
            ColumnFilterMode mode = column?.FilterMode ?? ColumnFilterMode.Text;
            ColumnFilterOperator op = mode switch
            {
                ColumnFilterMode.Numeric => ColumnFilterOperator.Equals,
                ColumnFilterMode.Date => ColumnFilterOperator.Equals,
                _ => ColumnFilterOperator.Contains
            };
            if (!string.IsNullOrWhiteSpace(text))
            {
                State.SetColumnFilter(columnName, new ColumnFilterEntry(text, op, mode));
            }
        }
        else
        {
            State.SetColumnFilter(columnName, text);
        }

        State.ResetPagination();
        ProcessData();

        if (OnFilter.HasDelegate)
        {
            // Surface the global FilterText for backwards-compat; consumers wanting the
            // per-column slice can read State.ColumnFilters directly.
            await OnFilter.InvokeAsync(new DataCollectionFilterEventArgs { FilterText = State.FilterText });
        }
    }

    /// <summary>
    /// Handler wired to the per-column operator <c>&lt;select&gt;</c> rendered in the
    /// header next to the filter input. Updates the operator on the existing entry and
    /// re-runs the pipeline; no-op when the column has no active text yet.
    /// </summary>
    protected void HandleColumnFilterOperator(string columnName, ChangeEventArgs e)
    {
        if (e.Value is not string raw)
        {
            return;
        }

        if (!Enum.TryParse(raw, true, out ColumnFilterOperator op))
        {
            return;
        }

        State.SetColumnFilterOperator(columnName, op);
        State.ResetPagination();
        ProcessData();
    }

    /// <summary>
    /// String-valued adapter wired to the internal select primitive's
    /// <see cref="EventCallback{T}"/> contract — converts the bare value into a
    /// <see cref="ChangeEventArgs"/> and delegates to
    /// <see cref="HandleColumnFilterOperator(string, ChangeEventArgs)"/>.
    /// </summary>
    protected void HandleColumnFilterOperatorString(string columnName, string? value)
        => HandleColumnFilterOperator(columnName, new ChangeEventArgs { Value = value });

    /// <summary>
    /// String-valued adapter wired to the internal text / date input primitives'
    /// <see cref="EventCallback{T}"/> contract — converts the bare value into a
    /// <see cref="ChangeEventArgs"/> and delegates to
    /// <see cref="HandleColumnFilterInput(string, ChangeEventArgs)"/>.
    /// </summary>
    protected async Task HandleColumnFilterInputText(string columnName, string? value)
        => await HandleColumnFilterInput(columnName, new ChangeEventArgs { Value = value });

    /// <summary>
    /// Returns the operator labels relevant to the column's <see cref="ColumnFilterMode"/>.
    /// Drives the operator <c>&lt;select&gt;</c> options rendered next to the filter input.
    /// </summary>
    protected static IEnumerable<ColumnFilterOperator> GetOperatorsFor(ColumnFilterMode mode) => mode switch
    {
        ColumnFilterMode.Numeric =>
        [
            ColumnFilterOperator.Equals,
            ColumnFilterOperator.NotEquals,
            ColumnFilterOperator.GreaterThan,
            ColumnFilterOperator.LessThan,
            ColumnFilterOperator.GreaterOrEqual,
            ColumnFilterOperator.LessOrEqual
        ],
        ColumnFilterMode.Date =>
        [
            ColumnFilterOperator.Equals,
            ColumnFilterOperator.NotEquals,
            ColumnFilterOperator.GreaterThan,
            ColumnFilterOperator.LessThan
        ],
        _ =>
        [
            ColumnFilterOperator.Contains,
            ColumnFilterOperator.StartsWith,
            ColumnFilterOperator.EndsWith,
            ColumnFilterOperator.Equals,
            ColumnFilterOperator.NotEquals
        ]
    };

    // Razor reads this at render time, so the flag here suppresses the BROWSER default
    // for the *next* matching keystroke (mirrors the BOBInputCheckbox `_preventKeyDefault`
    // pattern). First Alt+Arrow may still navigate browser history; subsequent presses
    // are suppressed once the flag has been committed.
    /// <summary>Backs the conditional <c>@onkeydown:preventDefault</c> binding on header cells.</summary>
    protected bool _preventHeaderKeyDefault;

    /// <summary>
    /// Header keydown handler invoked when <see cref="Reorderable"/> is on.
    /// <c>Alt+ArrowLeft</c> shifts the column one position toward the start, <c>Alt+ArrowRight</c>
    /// toward the end. Other keys fall through so existing keyboard handling (sort cycling
    /// via <c>Enter</c>) keeps working.
    /// </summary>
    protected void HandleHeaderKeyDown(string columnName, KeyboardEventArgs e)
    {
        // Flag set unconditionally for every Alt+Arrow combo we recognise — suppresses
        // the browser default (back/forward navigation on most platforms) on repeat.
        bool isReorderCombo = Reorderable
                              && e.AltKey
                              && e.Key is "ArrowLeft" or "Left" or "ArrowRight" or "Right";
        _preventHeaderKeyDefault = isReorderCombo;

        if (!isReorderCombo)
        {
            return;
        }

        int delta = e.Key switch
        {
            "ArrowLeft" or "Left" => -1,
            "ArrowRight" or "Right" => +1,
            _ => 0
        };
        if (delta == 0)
        {
            return;
        }

        IEnumerable<string> referenceOrder = VisibleColumns
            .Where(c => !string.IsNullOrEmpty(c.Header))
            .Select(c => c.Header!);
        State.MoveColumn(columnName, delta, referenceOrder);
        ProcessData();
    }

    // Header drag-to-reorder state. HTML5 drag/drop on the <th> itself; the user picks up
    // a header and drops it onto a peer to take that peer's slot. Keyboard reorder
    // (Alt+Arrow) still works in parallel — both write to State.ColumnOrder via MoveColumn.
    private string? _draggingColumnHeader;

    /// <summary>Header currently being dragged for reorder, or <see langword="null"/> when idle.</summary>
    protected string? DraggingColumnHeader => _draggingColumnHeader;

    /// <summary>
    /// HTML5 dragstart handler for a reorder-enabled column header. Captures the source
    /// column so the matching drop handler knows which header to move. No-op when
    /// <see cref="Reorderable"/> is off so the gesture is opt-in.
    /// </summary>
    protected void HandleHeaderDragStart(string columnName, DragEventArgs e)
    {
        if (!Reorderable || string.IsNullOrEmpty(columnName))
        {
            return;
        }

        _draggingColumnHeader = columnName;
        // The data payload is symbolic — actual movement happens through `_draggingColumnHeader`
        // on the same component, but populating dataTransfer still keeps the browser's
        // native drag affordance (cursor, ghost) responsive across hosts.
        e.DataTransfer.EffectAllowed = "move";
    }

    /// <summary>
    /// HTML5 dragover handler. Returning <c>preventDefault</c> via the matching
    /// <c>@ondragover:preventDefault</c> attribute is what makes a target eligible for
    /// drop; this handler is a no-op data hook so the grid can highlight the target.
    /// </summary>
    protected void HandleHeaderDragOver(string columnName, DragEventArgs e)
    {
        if (!Reorderable)
        {
            return;
        }
        // Reserved for future visual highlight — left as a stub so consumers can subclass
        // and decorate without touching the razor markup.
    }

    /// <summary>
    /// Drop handler that translates the drag pair (source / target headers) into a
    /// <see cref="DataCollectionState{TItem}.MoveColumn"/> call by computing the index
    /// delta between the two columns in the current visible order.
    /// </summary>
    protected void HandleHeaderDrop(string targetHeader, DragEventArgs e)
    {
        if (!Reorderable || string.IsNullOrEmpty(_draggingColumnHeader) || string.IsNullOrEmpty(targetHeader))
        {
            _draggingColumnHeader = null;
            return;
        }

        if (string.Equals(_draggingColumnHeader, targetHeader, StringComparison.OrdinalIgnoreCase))
        {
            _draggingColumnHeader = null;
            return;
        }

        List<string> order = VisibleColumns
            .Where(c => !string.IsNullOrEmpty(c.Header))
            .Select(c => c.Header!)
            .ToList();
        int from = order.FindIndex(h => string.Equals(h, _draggingColumnHeader, StringComparison.OrdinalIgnoreCase));
        int to = order.FindIndex(h => string.Equals(h, targetHeader, StringComparison.OrdinalIgnoreCase));
        if (from < 0 || to < 0)
        {
            _draggingColumnHeader = null;
            return;
        }

        State.MoveColumn(_draggingColumnHeader!, to - from, order);
        _draggingColumnHeader = null;
        ProcessData();
        PersistState();
    }

    /// <summary>Dragend handler clears the in-flight drag when the user releases outside any header.</summary>
    protected void HandleHeaderDragEnd(DragEventArgs e) => _draggingColumnHeader = null;

    // Column resize drag state. Pure-Blazor pattern (no JS interop) — pointerdown on the
    // trailing-edge resizer captures the column + start X, then a full-grid overlay div
    // listens for pointermove / pointerup so the cursor can leave the resizer mid-drag
    // without losing the gesture.
    private string? _resizingColumn;
    private double _resizeStartX;
    private double _resizeStartWidth;
    private double _resizeMin = 48d;
    private double _resizeMax;

    /// <summary>
    /// <see langword="true"/> while the user holds the resize handle on a column. Drives
    /// rendering of the full-grid drag overlay that captures pointermove / pointerup.
    /// </summary>
    protected bool IsResizing => _resizingColumn is not null;

    /// <summary>
    /// Pointerdown handler for the per-column resize handle. Seeds the drag state with
    /// the column's current effective width (state override > parameter > default 120).
    /// </summary>
    protected void StartColumnResize(DataColumnRegistration<TItem> col, PointerEventArgs e)
    {
        if (!Resizable || !col.Resizable || string.IsNullOrEmpty(col.Header))
        {
            return;
        }

        double current;
        if (State.ColumnWidths.TryGetValue(col.Header!, out double live))
        {
            current = live;
        }
        else if (TryParseWidthPx(col.Width, out double parsed))
        {
            current = parsed;
        }
        else
        {
            // Fallback used when the column has no explicit Width parameter — the resizer
            // still works, the user just starts from a sensible default.
            current = 120d;
        }

        _resizingColumn = col.Header;
        _resizeStartX = e.ClientX;
        _resizeStartWidth = current;
        _resizeMin = col.MinWidth > 0 ? col.MinWidth : 48d;
        _resizeMax = col.MaxWidth;
        StateHasChanged();
    }

    /// <summary>
    /// Pointermove handler bound to the drag overlay; updates the column width on every
    /// frame the user moves the cursor while the resize handle is held.
    /// </summary>
    protected void OnResizePointerMove(PointerEventArgs e)
    {
        if (_resizingColumn is null)
        {
            return;
        }

        double delta = e.ClientX - _resizeStartX;
        double target = _resizeStartWidth + delta;
        target = Math.Max(target, _resizeMin);
        if (_resizeMax > 0)
        {
            target = Math.Min(target, _resizeMax);
        }

        State.SetColumnWidth(_resizingColumn, target);
        StateHasChanged();
    }

    /// <summary>
    /// Pointerup / pointercancel handler that ends the drag. Triggers a persistence save
    /// so the resized width survives across reloads when <see cref="PersistenceKey"/>
    /// is configured.
    /// </summary>
    protected void EndColumnResize(PointerEventArgs e)
    {
        if (_resizingColumn is null)
        {
            return;
        }

        _resizingColumn = null;
        PersistState();
        StateHasChanged();
    }

    private static bool TryParseWidthPx(string? width, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(width))
        {
            return false;
        }

        string trimmed = width.Trim();
        if (trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^2];
        }

        return double.TryParse(trimmed, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out result);
    }

    protected async Task ClearFilterClicked() => ClearFilter();

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

    protected async Task ClearSelectionClicked() => await ClearSelection();

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
                Page = page, PageSize = State.PageSize
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
                    Page = State.CurrentPage, PageSize = newSize
                });
            }
        }
    }

    protected string? GetItemPatternStyle(int index) =>
        !UsePerItemPatternStyles ? null : ItemPattern?.GetItemInlineStyle(index);

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

    /// <summary>
    /// Compute the rendered footer text for an aggregated column. Numeric
    /// aggregates (Sum / Avg / Min / Max) coerce values via
    /// <see cref="Convert.ToDouble(object, IFormatProvider)"/> so the
    /// caller doesn't have to type-narrow per column. <see langword="null"/>
    /// items are skipped.
    /// </summary>
    /// <param name="column">Column whose <see cref="DataColumnRegistration{TItem}.Aggregate"/> drives the calculation.</param>
    /// <param name="items">Source items — typically <see cref="FilteredItems"/> so totals reflect the user's filters.</param>
    /// <returns>Formatted aggregate text or <see langword="null"/> when the column has no aggregate set.</returns>
    private protected string? ComputeAggregate(DataColumnRegistration<TItem> column, IReadOnlyCollection<TItem> items)
    {
        if (column.Aggregate == AggregateFunction.None)
        {
            return null;
        }

        object? raw = column.Aggregate switch
        {
            AggregateFunction.Custom when column.CustomAggregate is not null => column.CustomAggregate(items),
            AggregateFunction.Count => items.Count,
            _ when column.ValueSelector is null => null,
            AggregateFunction.Sum => SafeNumeric(items, column.ValueSelector!).Sum(),
            AggregateFunction.Average => SafeNumeric(items, column.ValueSelector!).DefaultIfEmpty(0).Average(),
            AggregateFunction.Min => column.CustomComparer is { } cmp
                ? items.Where(i => column.ValueSelector!(i) is not null)
                    .OrderBy(x => x, Comparer<TItem>.Create(new Comparison<TItem>(cmp))).Select(column.ValueSelector!)
                    .FirstOrDefault()
                : items.Select(column.ValueSelector!).Where(v => v is not null).OrderBy(v => v).FirstOrDefault(),
            AggregateFunction.Max => column.CustomComparer is { } cmp2
                ? items.Where(i => column.ValueSelector!(i) is not null)
                    .OrderByDescending(x => x, Comparer<TItem>.Create(new Comparison<TItem>(cmp2)))
                    .Select(column.ValueSelector!).FirstOrDefault()
                : items.Select(column.ValueSelector!).Where(v => v is not null).OrderByDescending(v => v)
                    .FirstOrDefault(),
            _ => null
        };

        string formatted = FormatValue(raw, column.AggregateFormat ?? column.Format);
        return string.IsNullOrEmpty(column.AggregateLabel)
            ? formatted
            : column.AggregateLabel + formatted;
    }

    /// <summary>True iff any visible column has a non-<see cref="AggregateFunction.None"/> aggregate set.</summary>
    private protected bool HasAggregates => VisibleColumns.Any(c => c.Aggregate != AggregateFunction.None);

    /// <summary>True when <see cref="RowActions"/> contains at least one entry.</summary>
    private protected bool HasRowActions => RowActions is { Count: > 0 };

    /// <summary>Yields the row actions visible for the given item. Visibility predicates default to <see langword="true"/>.</summary>
    private protected IEnumerable<DataCollectionRowAction<TItem>> VisibleRowActionsFor(TItem item)
    {
        if (RowActions is null)
        {
            yield break;
        }

        foreach (DataCollectionRowAction<TItem> action in RowActions)
        {
            if (action.Visible is null || action.Visible(item))
            {
                yield return action;
            }
        }
    }

    /// <summary>True when <see cref="BulkActions"/> contains at least one entry visible for the current selection.</summary>
    private protected bool HasVisibleBulkActions
    {
        get
        {
            if (BulkActions is not { Count: > 0 } || State.SelectedItems.Count == 0)
            {
                return false;
            }

            foreach (DataCollectionBulkAction<TItem> action in BulkActions)
            {
                if (action.Visible is null || action.Visible(State.SelectedItems.ToList()))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>Yields the bulk actions visible for the current selection.</summary>
    private protected IEnumerable<DataCollectionBulkAction<TItem>> VisibleBulkActions()
    {
        if (BulkActions is null || State.SelectedItems.Count == 0)
        {
            yield break;
        }

        IReadOnlyCollection<TItem> snapshot = State.SelectedItems.ToList();
        foreach (DataCollectionBulkAction<TItem> action in BulkActions)
        {
            if (action.Visible is null || action.Visible(snapshot))
            {
                yield return action;
            }
        }
    }

    /// <summary>Invoke a row-action handler. Stops propagation upstream via the markup's <c>@onclick:stopPropagation</c>.</summary>
    private protected async Task InvokeRowActionAsync(DataCollectionRowAction<TItem> action, TItem item)
    {
        if (action.OnClick is null)
        {
            return;
        }

        if (action.Enabled is not null && !action.Enabled(item))
        {
            return;
        }

        await action.OnClick(item);
    }

    /// <summary>Invoke a bulk-action handler with a snapshot of the current selection.</summary>
    private protected async Task InvokeBulkActionAsync(DataCollectionBulkAction<TItem> action)
    {
        if (action.OnClick is null)
        {
            return;
        }

        IReadOnlyCollection<TItem> snapshot = State.SelectedItems.ToList();
        if (action.Enabled is not null && !action.Enabled(snapshot))
        {
            return;
        }

        await action.OnClick(snapshot);
    }

    private static IEnumerable<double> SafeNumeric(IEnumerable<TItem> items, Func<TItem, object?> selector)
    {
        foreach (TItem item in items)
        {
            object? value = selector(item);
            if (value is null)
            {
                continue;
            }

            if (value is IConvertible)
            {
                double d;
                try { d = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture); }
                catch { continue; }

                yield return d;
            }
        }
    }

    protected string? GetAriaSort(DataColumnRegistration<TItem> col)
    {
        if (!col.Sortable)
        {
            return null;
        }

        SortDirection dir = GetSortDirection(col);
        return dir switch
        {
            SortDirection.Ascending => "ascending",
            SortDirection.Descending => "descending",
            _ => null
        };
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