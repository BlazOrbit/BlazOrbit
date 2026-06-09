namespace BlazOrbit.Components;

/// <summary>
/// Per-column filter entry stored in <c>DataCollectionState&lt;TItem&gt;.ColumnFilters</c>.
/// Pairs the user's filter text with the comparison operator and the column's data mode
/// so the grid pipeline can apply the right comparison without re-deriving it from the
/// column registration.
/// </summary>
/// <param name="Text">Raw filter text typed by the user.</param>
/// <param name="Operator">Comparison operator. <see cref="ColumnFilterOperator.Contains"/> by default for text columns.</param>
/// <param name="Mode">Data category hint (text / numeric / date). Controls which operators are allowed.</param>
public readonly record struct ColumnFilterEntry(
    string Text,
    ColumnFilterOperator Operator = ColumnFilterOperator.Contains,
    ColumnFilterMode Mode = ColumnFilterMode.Text);