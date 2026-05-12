namespace BlazOrbit.Components;

/// <summary>
/// Single sort step inside a multi-column sort. Lower
/// <see cref="Priority"/> values sort first; the next steps act as
/// tie-breakers (LINQ <c>OrderBy → ThenBy</c>).
/// </summary>
public sealed class SortDescriptor
{
    /// <summary>Header name of the sorted column.</summary>
    public string ColumnName { get; init; } = string.Empty;

    /// <summary>Sort direction. <see cref="SortDirection.None"/> drops the descriptor.</summary>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;

    /// <summary>1-based sort priority (1 = primary sort, 2 = secondary, …).</summary>
    public int Priority { get; init; } = 1;

    /// <summary>Convenience deconstructor.</summary>
    public void Deconstruct(out string column, out SortDirection direction, out int priority)
    {
        column = ColumnName;
        direction = Direction;
        priority = Priority;
    }
}

public enum SortAppendBehavior
{
    /// <summary>Never append when sorting => single column sorting</summary>
    None = 0,

    /// <summary>Always append when sorting</summary>
    Always = 1,

    /// <summary>Append to sorting when holding Shift key</summary>
    ShiftKey = 2,

    /// <summary>Append to sorting when holding Ctrl key</summary>
    CtrlKey = 3,

    /// <summary>Append to sorting when holding Ctrl or Shift key</summary>
    CtrlOrShiftKey = 4
}