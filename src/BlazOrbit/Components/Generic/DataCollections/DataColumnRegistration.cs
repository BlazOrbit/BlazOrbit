using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

public sealed class DataColumnRegistration<TItem>
{
    public ColumnAlign Align { get; set; } = ColumnAlign.Left;
    public string? CellClass { get; set; }
    public Func<TItem, TItem, int>? CustomComparer { get; set; }
    public Func<TItem, string, bool>? CustomFilter { get; set; }
    public bool Filterable { get; set; }
    public string? Format { get; set; }
    public string? Header { get; set; }
    public RenderFragment? HeaderTemplate { get; set; }
    public string? HeaderClass { get; set; }
    public bool Sortable { get; set; }
    public RenderFragment<TItem>? Template { get; set; }
    public Func<TItem, object?>? ValueSelector { get; set; }
    public bool Visible { get; set; } = true;
    public string? Width { get; set; }
}