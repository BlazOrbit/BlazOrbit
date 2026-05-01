using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

/// <summary>Describes how a single column is rendered and behaves inside a data collection.</summary>
public sealed class DataColumnRegistration<TItem>
{
    /// <summary>Horizontal alignment for the column's header and cells.</summary>
    public ColumnAlign Align { get; set; } = ColumnAlign.Left;
    /// <summary>Extra CSS class applied to every cell in the column.</summary>
    public string? CellClass { get; set; }
    /// <summary>Optional comparer used when sorting by this column.</summary>
    public Func<TItem, TItem, int>? CustomComparer { get; set; }
    /// <summary>Optional predicate used to evaluate filter matches for the column.</summary>
    public Func<TItem, string, bool>? CustomFilter { get; set; }
    /// <summary>When <see langword="true"/>, the column participates in free-text filtering.</summary>
    public bool Filterable { get; set; }
    /// <summary>Optional format string applied to <see cref="IFormattable"/> values.</summary>
    public string? Format { get; set; }
    /// <summary>Header label rendered when no <see cref="HeaderTemplate"/> is provided.</summary>
    public string? Header { get; set; }
    /// <summary>Custom render fragment used in place of <see cref="Header"/>.</summary>
    public RenderFragment? HeaderTemplate { get; set; }
    /// <summary>Extra CSS class applied to the header cell.</summary>
    public string? HeaderClass { get; set; }
    /// <summary>When <see langword="true"/>, the column header acts as a sort affordance.</summary>
    public bool Sortable { get; set; }
    /// <summary>Cell render template receiving the row item.</summary>
    public RenderFragment<TItem>? Template { get; set; }
    /// <summary>Selector used to project the cell value from the row item.</summary>
    public Func<TItem, object?>? ValueSelector { get; set; }
    /// <summary>When <see langword="false"/>, the column is registered but not rendered.</summary>
    public bool Visible { get; set; } = true;
    /// <summary>Optional explicit column width (any valid CSS length).</summary>
    public string? Width { get; set; }
}