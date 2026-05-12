using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazOrbit.Components;

/// <summary>
/// Maintains the state for data collection components including pagination, filtering, sorting, and selection.
/// </summary>
/// <typeparam name="TItem">The type of the items in the collection.</typeparam>
public sealed class DataCollectionState<TItem>
{
    private readonly HashSet<TItem> _selectedItems = [];

    // Per-column filter entries. Keyed by column header (case-insensitive). The value
    // carries the user-typed text plus the operator + data mode so the grid pipeline
    // applies the right comparison without re-deriving it from the column registration.
    private readonly Dictionary<string, ColumnFilterEntry> _columnFilters = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The current page number.
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// The current filter text.
    /// </summary>
    public string FilterText { get; set; } = string.Empty;

    /// <summary>
    /// Active per-column filter entries keyed by column header. Combined with
    /// <see cref="FilterText"/> using AND semantics — a row must match the global filter
    /// AND every column-scoped filter to remain visible. Mutate via
    /// <see cref="SetColumnFilter(string, string)"/> /
    /// <see cref="SetColumnFilter(string, ColumnFilterEntry)"/> /
    /// <see cref="ClearColumnFilter"/> / <see cref="ClearAllColumnFilters"/>.
    /// </summary>
    public IReadOnlyDictionary<string, ColumnFilterEntry> ColumnFilters => _columnFilters;

    /// <summary>
    /// Sets the filter text for a specific column with default <see cref="ColumnFilterOperator.Contains"/>
    /// + <see cref="ColumnFilterMode.Text"/> semantics. Whitespace / empty clears the
    /// entry. Column lookup is case-insensitive against the column header.
    /// </summary>
    public void SetColumnFilter(string columnName, string filter)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(filter))
        {
            _columnFilters.Remove(columnName);
            return;
        }

        // Preserve a previously chosen operator / mode when one is already configured —
        // typing into the input shouldn't reset the dropdown choice.
        ColumnFilterOperator op = _columnFilters.TryGetValue(columnName, out ColumnFilterEntry existing)
            ? existing.Operator
            : ColumnFilterOperator.Contains;
        ColumnFilterMode mode = _columnFilters.TryGetValue(columnName, out existing)
            ? existing.Mode
            : ColumnFilterMode.Text;
        _columnFilters[columnName] = new ColumnFilterEntry(filter, op, mode);
    }

    /// <summary>Sets the full filter entry (text + operator + mode) for a column.</summary>
    public void SetColumnFilter(string columnName, ColumnFilterEntry entry)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(entry.Text))
        {
            _columnFilters.Remove(columnName);
            return;
        }

        _columnFilters[columnName] = entry;
    }

    /// <summary>
    /// Updates only the operator for an existing column-filter entry. No-op when the
    /// column has no active filter — typing the text comes first.
    /// </summary>
    public void SetColumnFilterOperator(string columnName, ColumnFilterOperator op)
    {
        if (!_columnFilters.TryGetValue(columnName, out ColumnFilterEntry existing))
        {
            return;
        }

        _columnFilters[columnName] = existing with { Operator = op };
    }

    /// <summary>Removes the per-column filter for the given column header.</summary>
    public void ClearColumnFilter(string columnName) => _columnFilters.Remove(columnName);

    /// <summary>Drops every per-column filter while preserving <see cref="FilterText"/>.</summary>
    public void ClearAllColumnFilters() => _columnFilters.Clear();

    // Custom column order. When empty the grid renders the registry's natural order; once
    // populated (typically via the keyboard-driven Alt+Arrow handler in BOBDataGrid) it
    // becomes the source of truth for visible-column ordering.
    private readonly List<string> _columnOrder = [];

    /// <summary>
    /// Active column ordering by header name. Empty means "use the registration order".
    /// Mutate via <see cref="MoveColumn"/> / <see cref="SetColumnOrder"/> /
    /// <see cref="ClearColumnOrder"/>.
    /// </summary>
    public IReadOnlyList<string> ColumnOrder => _columnOrder;

    /// <summary>
    /// Replaces the column order with the supplied sequence. Pass an empty enumerable to
    /// revert to the registration order.
    /// </summary>
    public void SetColumnOrder(IEnumerable<string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        _columnOrder.Clear();
        foreach (string h in headers)
        {
            if (!string.IsNullOrEmpty(h))
            {
                _columnOrder.Add(h);
            }
        }
    }

    /// <summary>Drops the custom column order and reverts to the registry's natural order.</summary>
    public void ClearColumnOrder() => _columnOrder.Clear();

    // Runtime column widths produced by the resize drag handle in BOBDataGrid. When a
    // column has an entry here the grid uses the dictionary value (in pixels) instead of
    // the static `Width` parameter, so the user's drag survives across re-renders and is
    // covered by the persistence round-trip.
    private readonly Dictionary<string, double> _columnWidths = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Live column widths in pixels, keyed by header name. Empty by default — populated
    /// when the user drags the resize handle in <c>BOBDataGrid</c>. Mutate via
    /// <see cref="SetColumnWidth"/> / <see cref="ClearColumnWidth"/> /
    /// <see cref="ClearAllColumnWidths"/>.
    /// </summary>
    public IReadOnlyDictionary<string, double> ColumnWidths => _columnWidths;

    /// <summary>Sets a runtime width (in pixels) for the named column.</summary>
    public void SetColumnWidth(string columnName, double widthPx)
    {
        ArgumentException.ThrowIfNullOrEmpty(columnName);
        if (widthPx <= 0)
        {
            _columnWidths.Remove(columnName);
            return;
        }

        _columnWidths[columnName] = widthPx;
    }

    /// <summary>Drops the runtime width override for a single column.</summary>
    public void ClearColumnWidth(string columnName) => _columnWidths.Remove(columnName);

    /// <summary>Drops every runtime width override.</summary>
    public void ClearAllColumnWidths() => _columnWidths.Clear();

    /// <summary>
    /// Shifts a column by <paramref name="delta"/> positions in the visible order. The
    /// state lazily seeds itself from <paramref name="referenceOrder"/> on the first move
    /// so consumers don't have to call <see cref="SetColumnOrder"/> first.
    /// </summary>
    /// <param name="columnName">Header of the column being moved (case-insensitive).</param>
    /// <param name="delta">Positive moves toward the end, negative toward the start. Out-of-bounds is silently clamped.</param>
    /// <param name="referenceOrder">Source-of-truth ordering used when <see cref="ColumnOrder"/> is empty.</param>
    public void MoveColumn(string columnName, int delta, IEnumerable<string> referenceOrder)
    {
        ArgumentException.ThrowIfNullOrEmpty(columnName);
        ArgumentNullException.ThrowIfNull(referenceOrder);

        if (_columnOrder.Count == 0)
        {
            foreach (string h in referenceOrder)
            {
                if (!string.IsNullOrEmpty(h))
                {
                    _columnOrder.Add(h);
                }
            }
        }

        int index = _columnOrder.FindIndex(h => string.Equals(h, columnName, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return;
        }

        int target = Math.Clamp(index + delta, 0, _columnOrder.Count - 1);
        if (target == index)
        {
            return;
        }

        string entry = _columnOrder[index];
        _columnOrder.RemoveAt(index);
        _columnOrder.Insert(target, entry);
    }

    /// <summary>
    /// The number of items displayed per page.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// The set of currently selected items.
    /// </summary>
    public IReadOnlySet<TItem> SelectedItems => _selectedItems;

    private readonly List<SortDescriptor> _sortDescriptors = [];

    /// <summary>
    /// The name of the primary sort column (first descriptor in
    /// <see cref="SortDescriptors"/>). Kept as a writeable shortcut for
    /// backward compatibility with existing single-sort code.
    /// </summary>
    public string? SortColumn
    {
        get => _sortDescriptors.Count > 0 ? _sortDescriptors[0].ColumnName : null;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                _sortDescriptors.Clear();
            }
            else
            {
                if (_sortDescriptors.Count > 0 && _sortDescriptors[0].ColumnName == value)
                {
                    return;
                }

                _sortDescriptors.Clear();
                _sortDescriptors.Add(new SortDescriptor
                {
                    ColumnName = value, Direction = SortDirection.Ascending, Priority = 1
                });
            }
        }
    }

    /// <summary>
    /// The current direction of the primary sort. Maps to the first
    /// descriptor in <see cref="SortDescriptors"/>; setting it updates
    /// that descriptor in place.
    /// </summary>
    public SortDirection SortDirection
    {
        get => _sortDescriptors.Count > 0 ? _sortDescriptors[0].Direction : SortDirection.None;
        set
        {
            if (_sortDescriptors.Count > 0)
            {
                _sortDescriptors[0].Direction = value;
            }
        }
    }

    /// <summary>
    /// Active sort descriptors in priority order (primary first). When
    /// empty, no sort is applied. Use <see cref="ToggleSort(string, bool)"/>
    /// from the component layer to manipulate this list — direct mutation
    /// is allowed for advanced scenarios but the renderer expects the
    /// indices to stay 1-based contiguous.
    /// </summary>
    public IReadOnlyList<SortDescriptor> SortDescriptors => _sortDescriptors;

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
    /// Toggle the sort state for a column. Single-sort semantics: cycles
    /// the column through Asc → Desc → None and replaces any existing
    /// sort. Use <see cref="ToggleSort(string, bool)"/> with
    /// <c>append=true</c> to add a tie-breaker without dropping prior
    /// sort levels (multi-column sort).
    /// </summary>
    public void ToggleSort(string columnName) => ToggleSort(columnName, false);

    /// <summary>
    /// Toggle the sort state for a column. When <paramref name="append"/>
    /// is <see langword="true"/>, the column joins (or cycles within)
    /// the existing descriptor list as the next priority instead of
    /// replacing it — wires Shift+Click for multi-column sort.
    /// </summary>
    public void ToggleSort(string columnName, bool append)
    {
        SortDescriptor? existing = _sortDescriptors.FirstOrDefault(d => d.ColumnName == columnName);

        if (!append)
        {
            if (existing is not null && _sortDescriptors.Count == 1)
            {
                // Single-column cycle: Asc → Desc → drop.
                SortDirection next = existing.Direction switch
                {
                    SortDirection.None => SortDirection.Ascending,
                    SortDirection.Ascending => SortDirection.Descending,
                    SortDirection.Descending => SortDirection.None,
                    _ => SortDirection.Ascending
                };
                if (next == SortDirection.None)
                {
                    _sortDescriptors.Clear();
                    return;
                }

                existing.Direction = next;
                return;
            }

            // Replace any current sort with a fresh ascending entry.
            _sortDescriptors.Clear();
            _sortDescriptors.Add(new SortDescriptor
            {
                ColumnName = columnName, Direction = SortDirection.Ascending, Priority = 1
            });
            return;
        }

        // Append / cycle within the multi-sort list.
        if (existing is not null)
        {
            SortDirection next = existing.Direction switch
            {
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.Ascending
            };
            if (next == SortDirection.None)
            {
                _sortDescriptors.Remove(existing);
                ReassignPriorities();
                return;
            }

            existing.Direction = next;
            return;
        }

        _sortDescriptors.Add(new SortDescriptor
        {
            ColumnName = columnName, Direction = SortDirection.Ascending, Priority = _sortDescriptors.Count + 1
        });
    }

    /// <summary>Drops every active sort descriptor.</summary>
    public void ClearSort() => _sortDescriptors.Clear();

    private void ReassignPriorities()
    {
        for (int i = 0; i < _sortDescriptors.Count; i++)
        {
            // SortDescriptor.Priority has init-only setter; rebuild the entry.
            _sortDescriptors[i] = new SortDescriptor
            {
                ColumnName = _sortDescriptors[i].ColumnName,
                Direction = _sortDescriptors[i].Direction,
                Priority = i + 1
            };
        }
    }

    /// <summary>
    /// Serialises the persistable slice of state (filter / sort / page / column order
    /// / column filters) to a JSON payload suitable for round-tripping through
    /// <see cref="IDataCollectionStatePersistence"/>. Selection is intentionally excluded
    /// — selected items reference live data that may not be present after a reload.
    /// </summary>
    public string ToJson()
    {
        StatePayload payload = new()
        {
            FilterText = FilterText,
            CurrentPage = CurrentPage,
            PageSize = PageSize,
            ColumnFilters = _columnFilters.ToDictionary(
                kv => kv.Key,
                kv => new ColumnFilterPayload
                {
                    Text = kv.Value.Text, Operator = kv.Value.Operator, Mode = kv.Value.Mode
                }),
            ColumnOrder = [.._columnOrder],
            ColumnWidths = new Dictionary<string, double>(_columnWidths),
            SortDescriptors = _sortDescriptors
                .Select(d =>
                    new SortPayload { ColumnName = d.ColumnName, Direction = d.Direction, Priority = d.Priority })
                .ToList()
        };
        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Restores state from a JSON payload produced by <see cref="ToJson"/>. Malformed
    /// input is silently dropped so a corrupted localStorage entry does not block the
    /// grid from rendering.
    /// </summary>
    public void LoadFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        StatePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<StatePayload>(json);
        }
        catch (JsonException)
        {
            return;
        }

        if (payload is null)
        {
            return;
        }

        FilterText = payload.FilterText ?? string.Empty;
        CurrentPage = payload.CurrentPage > 0 ? payload.CurrentPage : 1;
        if (payload.PageSize > 0)
        {
            PageSize = payload.PageSize;
        }

        _columnFilters.Clear();
        if (payload.ColumnFilters is not null)
        {
            foreach (KeyValuePair<string, ColumnFilterPayload> entry in payload.ColumnFilters)
            {
                if (string.IsNullOrEmpty(entry.Key) || entry.Value is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.Value.Text))
                {
                    continue;
                }

                _columnFilters[entry.Key] = new ColumnFilterEntry(
                    entry.Value.Text,
                    entry.Value.Operator,
                    entry.Value.Mode);
            }
        }

        _columnOrder.Clear();
        if (payload.ColumnOrder is not null)
        {
            foreach (string h in payload.ColumnOrder)
            {
                if (!string.IsNullOrEmpty(h))
                {
                    _columnOrder.Add(h);
                }
            }
        }

        _columnWidths.Clear();
        if (payload.ColumnWidths is not null)
        {
            foreach (KeyValuePair<string, double> entry in payload.ColumnWidths)
            {
                if (string.IsNullOrEmpty(entry.Key) || entry.Value <= 0)
                {
                    continue;
                }

                _columnWidths[entry.Key] = entry.Value;
            }
        }

        _sortDescriptors.Clear();
        if (payload.SortDescriptors is not null)
        {
            foreach (SortPayload entry in payload.SortDescriptors)
            {
                if (string.IsNullOrEmpty(entry.ColumnName))
                {
                    continue;
                }

                _sortDescriptors.Add(new SortDescriptor
                {
                    ColumnName = entry.ColumnName, Direction = entry.Direction, Priority = entry.Priority
                });
            }
        }
    }

    private sealed class StatePayload
    {
        [JsonPropertyName("filterText")] public string? FilterText { get; set; }
        [JsonPropertyName("currentPage")] public int CurrentPage { get; set; }
        [JsonPropertyName("pageSize")] public int PageSize { get; set; }
        [JsonPropertyName("columnFilters")] public Dictionary<string, ColumnFilterPayload>? ColumnFilters { get; set; }
        [JsonPropertyName("columnOrder")] public List<string>? ColumnOrder { get; set; }
        [JsonPropertyName("columnWidths")] public Dictionary<string, double>? ColumnWidths { get; set; }
        [JsonPropertyName("sortDescriptors")] public List<SortPayload>? SortDescriptors { get; set; }
    }

    private sealed class ColumnFilterPayload
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
        [JsonPropertyName("operator")] public ColumnFilterOperator Operator { get; set; }
        [JsonPropertyName("mode")] public ColumnFilterMode Mode { get; set; }
    }

    private sealed class SortPayload
    {
        [JsonPropertyName("column")] public string? ColumnName { get; set; }
        [JsonPropertyName("direction")] public SortDirection Direction { get; set; }
        [JsonPropertyName("priority")] public int Priority { get; set; }
    }
}