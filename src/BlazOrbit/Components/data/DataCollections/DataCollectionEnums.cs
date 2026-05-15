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

/// <summary>
/// Pinning side for a frozen <c>BOBDataColumn{TItem}</c>. Frozen columns sit on the
/// scroll edge via <c>position: sticky</c> so they stay visible while the rest of the
/// grid scrolls horizontally inside its container.
/// </summary>
public enum ColumnFreeze
{
    /// <summary>Column scrolls with the rest of the grid (default).</summary>
    None = 0,

    /// <summary>Column pins to the inline-start edge (left in LTR).</summary>
    Start = 1,

    /// <summary>Column pins to the inline-end edge (right in LTR).</summary>
    End = 2
}

/// <summary>
/// Aggregation function rendered for a column in the data-collection
/// footer. Aggregates run against the post-filter, pre-pagination set so
/// totals reflect the full filtered result rather than just the visible
/// page.
/// </summary>
public enum AggregateFunction
{
    /// <summary>No aggregate is rendered for this column.</summary>
    None = 0,

    /// <summary>Sum of every value in the column. Numeric columns only.</summary>
    Sum,

    /// <summary>Arithmetic mean. Numeric columns only.</summary>
    Average,

    /// <summary>Number of items in the filtered set.</summary>
    Count,

    /// <summary>Minimum comparable value.</summary>
    Min,

    /// <summary>Maximum comparable value.</summary>
    Max,

    /// <summary>Use the column's custom aggregate delegate.</summary>
    Custom
}

/// <summary>
/// Visual mode used while <see cref="BlazOrbit.Components.BOBDataCollectionBase{TItem, TComponent, TVariant}.Loading"/>
/// is on. <see cref="LoadingMode.Skeleton"/> renders animated row
/// placeholders for a smoother UX than the default spinner — pair with
/// remote loading where the row count is known up front.
/// </summary>
public enum LoadingMode
{
    /// <summary>Inline spinner / loader content.</summary>
    Spinner = 0,

    /// <summary>Animated rows / cards with placeholder lines.</summary>
    Skeleton
}