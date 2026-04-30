namespace BlazOrbit.Components;

/// <summary>
/// Defines the filtering mode for data collection components.
/// </summary>
public enum FilterMode
{
    /// <summary>
    /// No filtering is applied.
    /// </summary>
    None,

    /// <summary>
    /// Matches items that contain the filter text.
    /// </summary>
    Contains,

    /// <summary>
    /// Matches items that start with the filter text.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Matches items that end with the filter text.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Matches items that equal the filter text exactly.
    /// </summary>
    Equals,

    /// <summary>
    /// Uses a custom filtering logic.
    /// </summary>
    Custom
}

/// <summary>
/// Defines the selection mode for data collection components.
/// </summary>
public enum SelectionMode
{
    /// <summary>
    /// No selection is allowed.
    /// </summary>
    None,

    /// <summary>
    /// Only a single item can be selected.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple items can be selected.
    /// </summary>
    Multiple
}

/// <summary>
/// Defines the sort direction for data collection columns.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// No sorting is applied.
    /// </summary>
    None,

    /// <summary>
    /// Sorts in ascending order.
    /// </summary>
    Ascending,

    /// <summary>
    /// Sorts in descending order.
    /// </summary>
    Descending
}

/// <summary>
/// Defines the horizontal alignment of a data collection column.
/// </summary>
public enum ColumnAlign
{
    /// <summary>
    /// Aligns content to the left.
    /// </summary>
    Left,

    /// <summary>
    /// Aligns content to the center.
    /// </summary>
    Center,

    /// <summary>
    /// Aligns content to the right.
    /// </summary>
    Right
}
