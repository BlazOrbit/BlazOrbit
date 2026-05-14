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

    /// <summary>
    /// Setter used by inline-edit mode to write the user's input back to the row item.
    /// Derived from the column's <c>Property</c> expression when the body is a member
    /// access against a writable property. <see langword="null"/> when the column has
    /// no settable property (e.g. computed columns or read-only projections) — the grid
    /// renders such columns as non-editable even if <see cref="Editable"/> is on.
    /// </summary>
    public Action<TItem, object?>? ValueSetter { get; set; }

    /// <summary>
    /// When <see langword="true"/> and the parent grid is in an inline-edit mode, the
    /// column participates in editing — cells render an input on activation instead of
    /// the static template. Requires <see cref="ValueSetter"/> to be non-null (auto-derived
    /// from a writable <c>Property</c>) or a custom <see cref="EditTemplate"/>.
    /// </summary>
    public bool Editable { get; set; }

    /// <summary>
    /// Optional custom editor fragment rendered while a cell is in edit mode. Receives the
    /// row item. When <see langword="null"/>, the grid picks a default input primitive
    /// based on the value type (string → text, numeric → number, date → date, bool →
    /// checkbox). Override for cases the auto-resolver can't handle (enum dropdowns,
    /// lookup pickers, etc.).
    /// </summary>
    public RenderFragment<TItem>? EditTemplate { get; set; }

    /// <summary>
    /// Optional validator invoked on every cell commit when the column is editable.
    /// Receives the row item and the proposed value (already coerced to the property type
    /// via the column's setter pipeline). Returns the error message to surface in the
    /// cell's tooltip when validation fails, or <see langword="null"/> to allow the
    /// commit. The grid prevents the commit and keeps the editor open on failure.
    /// </summary>
    public Func<TItem, object?, string?>? Validator { get; set; }

    /// <summary>When <see langword="false"/>, the column is registered but not rendered.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>Optional explicit column width (any valid CSS length).</summary>
    public string? Width { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the grid renders a drag handle at the trailing edge
    /// of this column's header so the user can resize the column with pointer events.
    /// Requires <c>BOBDataGrid.Resizable</c> to also be on.
    /// </summary>
    public bool Resizable { get; set; }

    /// <summary>
    /// Lower bound (in pixels) the resize drag will clamp to. Defaults to <c>48</c> so a
    /// user can't shrink a column below the typical icon + padding minimum.
    /// </summary>
    public double MinWidth { get; set; } = 48d;

    /// <summary>
    /// Upper bound (in pixels) the resize drag will clamp to. <c>0</c> (default) means
    /// no upper limit — the column grows freely.
    /// </summary>
    public double MaxWidth { get; set; }

    /// <summary>
    /// Pin side for the column. Defaults to <see cref="ColumnFreeze.None"/>. Frozen
    /// columns require an explicit <see cref="Width"/> in pixels so the grid can
    /// compute cumulative <c>left</c> / <c>right</c> offsets for adjacent frozen peers.
    /// </summary>
    public ColumnFreeze Freeze { get; set; } = ColumnFreeze.None;

    /// <summary>
    /// Filter category for the column. Drives which input type (<c>type=text</c> /
    /// <c>type=number</c> / <c>type=date</c>) and which operator dropdown the grid
    /// renders when <c>ShowColumnFilters=true</c>. Defaults to
    /// <see cref="ColumnFilterMode.Text"/>.
    /// </summary>
    public ColumnFilterMode FilterMode { get; set; } = ColumnFilterMode.Text;

    /// <summary>
    /// Aggregate function rendered in the footer cell of this column. The
    /// aggregate runs against the full filtered set (not just the visible
    /// page) so the total reflects the result of the user's filters.
    /// </summary>
    public AggregateFunction Aggregate { get; set; } = AggregateFunction.None;

    /// <summary>
    /// Optional <see cref="IFormattable"/> format applied to the aggregate
    /// value. Falls back to <see cref="Format"/> when not set so consumers
    /// don't have to repeat their currency / percentage format strings.
    /// </summary>
    public string? AggregateFormat { get; set; }

    /// <summary>
    /// Prefix label shown next to the aggregate value (e.g. <c>"Total: "</c>,
    /// <c>"Avg: "</c>). When <see langword="null"/> the column renders the
    /// aggregate value alone.
    /// </summary>
    public string? AggregateLabel { get; set; }

    /// <summary>
    /// Custom aggregate delegate invoked when <see cref="Aggregate"/> is
    /// <see cref="AggregateFunction.Custom"/>. Receives the filtered items
    /// and returns the value to render (formatted via
    /// <see cref="AggregateFormat"/> when applicable).
    /// </summary>
    public Func<IEnumerable<TItem>, object?>? CustomAggregate { get; set; }
}