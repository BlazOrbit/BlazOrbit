using System.Globalization;

namespace BlazOrbit.Components;

/// <summary>
/// Shared filter / sort / paginate routines used by the in-memory data path of
/// <see cref="BOBDataCollectionBase{TItem, TComponent, TVariant}"/> and the
/// <see cref="InMemoryDataSource{TItem}"/> wrapper. Kept as a private helper so the same
/// semantics drive both code paths — the legacy <c>Items</c> parameter and the new
/// <c>DataSource</c> abstraction agree on filter operators, sort tie-breakers and
/// pagination math.
/// </summary>
internal static class DataCollectionPipeline
{
    /// <summary>
    /// Applies the global filter + per-column filters against <paramref name="items"/>.
    /// Mirrors the legacy behaviour: when <paramref name="customGlobalFilter"/> is set
    /// it drives the global match; otherwise every column with a non-null
    /// <c>ValueSelector</c> contributes a substring check. Per-column entries are
    /// AND-combined with the global match and with each other.
    /// </summary>
    internal static IEnumerable<TItem> ApplyFilter<TItem>(
        IEnumerable<TItem> items,
        string? globalFilter,
        IReadOnlyDictionary<string, ColumnFilterEntry> columnFilters,
        IReadOnlyList<DataColumnRegistration<TItem>> registeredColumns,
        Func<TItem, string, bool>? customGlobalFilter)
    {
        IEnumerable<TItem> result = items;

        // Global filter — operates across every filterable column. Preserved as the legacy
        // path so consumers using the toolbar search box keep working unchanged.
        if (!string.IsNullOrWhiteSpace(globalFilter))
        {
            if (customGlobalFilter is not null)
            {
                string filterValue = globalFilter;
                result = result.Where(item => customGlobalFilter(item, filterValue));
            }
            else
            {
                string searchText = globalFilter.ToLowerInvariant();
                List<DataColumnRegistration<TItem>> filterableColumns = registeredColumns
                    .Where(c => c.Filterable && c.ValueSelector != null)
                    .ToList();

                if (filterableColumns.Count == 0)
                {
                    filterableColumns = registeredColumns
                        .Where(c => c.ValueSelector != null)
                        .ToList();
                }

                result = result.Where(item =>
                    filterableColumns.Any(col =>
                    {
                        object? value = col.ValueSelector?.Invoke(item);
                        return value?.ToString()?.ToLowerInvariant().Contains(searchText) ?? false;
                    }));
            }
        }

        // Per-column filters — AND-combined with the global filter and with each other so
        // typing "al" into the Name column then "30" into Age narrows the visible rows.
        if (columnFilters.Count > 0)
        {
            foreach (KeyValuePair<string, ColumnFilterEntry> entry in columnFilters)
            {
                if (string.IsNullOrWhiteSpace(entry.Value.Text))
                {
                    continue;
                }

                DataColumnRegistration<TItem>? column = registeredColumns
                    .FirstOrDefault(c => string.Equals(c.Header, entry.Key, StringComparison.OrdinalIgnoreCase));
                if (column?.ValueSelector is null)
                {
                    continue;
                }

                ColumnFilterEntry filter = entry.Value;
                if (column.CustomFilter != null)
                {
                    // Custom predicates ignore the operator dropdown — consumers wrote
                    // bespoke matching logic that has its own semantics.
                    Func<TItem, string, bool> custom = column.CustomFilter;
                    result = result.Where(item => custom(item, filter.Text));
                    continue;
                }

                Func<TItem, object?> selector = column.ValueSelector;
                result = filter.Mode switch
                {
                    ColumnFilterMode.Numeric => result.Where(item => MatchesNumeric(selector(item), filter)),
                    ColumnFilterMode.Date => result.Where(item => MatchesDate(selector(item), filter)),
                    _ => result.Where(item => MatchesText(selector(item), filter))
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Multi-column sort honouring descriptor priority. Custom comparers on the column
    /// registration take precedence over the value selector; subsequent descriptors chain
    /// via <c>CreateOrderedEnumerable</c> so <c>OrderBy → ThenBy</c> semantics survive.
    /// </summary>
    internal static IEnumerable<TItem> ApplySort<TItem>(
        IEnumerable<TItem> items,
        IReadOnlyList<SortDescriptor> descriptors,
        IReadOnlyList<DataColumnRegistration<TItem>> registeredColumns)
    {
        if (descriptors.Count == 0)
        {
            return items;
        }

        IOrderedEnumerable<TItem>? ordered = null;
        foreach (SortDescriptor descriptor in descriptors)
        {
            DataColumnRegistration<TItem>? column = registeredColumns
                .FirstOrDefault(c => c.Header == descriptor.ColumnName);
            if (column?.ValueSelector is null)
            {
                continue;
            }

            bool ascending = descriptor.Direction == SortDirection.Ascending;

            // Custom comparer takes precedence — it operates on items, not the value
            // selector, so we wrap it as IComparer<TItem> and chain via
            // CreateOrderedEnumerable for ThenBy fall-through.
            if (column.CustomComparer is { } cmp)
            {
                Comparison<TItem> comparison = ascending
                    ? new Comparison<TItem>(cmp)
                    : (x, y) => cmp(y, x);
                IComparer<TItem> comparer = Comparer<TItem>.Create(comparison);
                ordered = ordered is null
                    ? items.OrderBy(static x => x, comparer)
                    : ordered.CreateOrderedEnumerable(static x => x, comparer, false);
                continue;
            }

            Func<TItem, object?> selector = column.ValueSelector;
            ordered = ordered is null
                ? ascending ? items.OrderBy(selector) : items.OrderByDescending(selector)
                : ascending
                    ? ordered.ThenBy(selector)
                    : ordered.ThenByDescending(selector);
        }

        return ordered ?? items;
    }

    /// <summary>
    /// Skips <c>(page - 1) * pageSize</c> entries and takes <c>pageSize</c>. The caller
    /// owns the decision to paginate at all — passing a <see langword="null"/>
    /// <c>pageSize</c> upstream means the full filtered/sorted set flows through.
    /// </summary>
    internal static IEnumerable<TItem> ApplyPagination<TItem>(
        IEnumerable<TItem> items,
        int currentPage,
        int pageSize)
    {
        int skip = (currentPage - 1) * pageSize;
        return items.Skip(skip).Take(pageSize);
    }

    private static bool MatchesText(object? value, ColumnFilterEntry filter)
    {
        string raw = value?.ToString() ?? string.Empty;
        string left = raw.ToLowerInvariant();
        string right = filter.Text.ToLowerInvariant();
        return filter.Operator switch
        {
            ColumnFilterOperator.StartsWith => left.StartsWith(right, StringComparison.Ordinal),
            ColumnFilterOperator.EndsWith => left.EndsWith(right, StringComparison.Ordinal),
            ColumnFilterOperator.Equals => left == right,
            ColumnFilterOperator.NotEquals => left != right,
            // Contains is the safe fallback for operators that don't make sense on text
            // (e.g. GreaterThan) — consumers picking the wrong combo see a plain substring
            // match instead of an empty result.
            _ => left.Contains(right, StringComparison.Ordinal)
        };
    }

    private static bool MatchesNumeric(object? value, ColumnFilterEntry filter)
    {
        if (!TryToDouble(value, out double left))
        {
            return false;
        }

        if (!double.TryParse(filter.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double right))
        {
            return false;
        }

        return filter.Operator switch
        {
            ColumnFilterOperator.Equals => left == right,
            ColumnFilterOperator.NotEquals => left != right,
            ColumnFilterOperator.GreaterThan => left > right,
            ColumnFilterOperator.LessThan => left < right,
            ColumnFilterOperator.GreaterOrEqual => left >= right,
            ColumnFilterOperator.LessOrEqual => left <= right,
            _ => left == right
        };
    }

    private static bool MatchesDate(object? value, ColumnFilterEntry filter)
    {
        if (!TryToDateTime(value, out DateTime left))
        {
            return false;
        }

        if (!DateTime.TryParse(filter.Text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime right))
        {
            return false;
        }

        // Date comparisons drop the time component so an entry typed as "2026-05-10" matches
        // rows on that calendar day regardless of the wall-clock time stored.
        DateTime l = left.Date;
        DateTime r = right.Date;
        return filter.Operator switch
        {
            ColumnFilterOperator.Equals => l == r,
            ColumnFilterOperator.NotEquals => l != r,
            ColumnFilterOperator.GreaterThan => l > r,
            ColumnFilterOperator.LessThan => l < r,
            ColumnFilterOperator.GreaterOrEqual => l >= r,
            ColumnFilterOperator.LessOrEqual => l <= r,
            _ => l == r
        };
    }

    private static bool TryToDouble(object? value, out double result)
    {
        result = 0;
        if (value is null)
        {
            return false;
        }

        try
        {
            result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (FormatException) { return false; }
        catch (InvalidCastException) { return false; }
        catch (OverflowException) { return false; }
    }

    private static bool TryToDateTime(object? value, out DateTime result)
    {
        switch (value)
        {
            case DateTime dt:
                result = dt;
                return true;
            case DateTimeOffset dto:
                result = dto.LocalDateTime;
                return true;
            case DateOnly d:
                result = d.ToDateTime(TimeOnly.MinValue);
                return true;
            case null:
                result = default;
                return false;
            default:
                return DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result);
        }
    }
}
