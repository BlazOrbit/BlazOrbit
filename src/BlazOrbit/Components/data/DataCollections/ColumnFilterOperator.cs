namespace BlazOrbit.Components;

/// <summary>
/// Comparison operator applied by a per-column filter. The filter text is interpreted
/// according to the column's data category (text vs numeric vs date) — operators that
/// don't apply to a category collapse to <see cref="Contains"/> for safety.
/// </summary>
public enum ColumnFilterOperator
{
    /// <summary>Substring match (default for text columns).</summary>
    Contains = 0,

    /// <summary>Prefix match (text only).</summary>
    StartsWith = 1,

    /// <summary>Suffix match (text only).</summary>
    EndsWith = 2,

    /// <summary>Exact equality.</summary>
    Equals = 3,

    /// <summary>Inequality.</summary>
    NotEquals = 4,

    /// <summary>Strictly greater than (numeric / date).</summary>
    GreaterThan = 5,

    /// <summary>Strictly less than (numeric / date).</summary>
    LessThan = 6,

    /// <summary>Greater than or equal (numeric / date).</summary>
    GreaterOrEqual = 7,

    /// <summary>Less than or equal (numeric / date).</summary>
    LessOrEqual = 8
}

/// <summary>
/// Hint for the per-column filter UI: drives which operator dropdown / input type the
/// grid renders for the column. The default <see cref="Text"/> matches the legacy
/// substring-only filter shipped in I1.S1.
/// </summary>
public enum ColumnFilterMode
{
    /// <summary>Text input + Contains/StartsWith/EndsWith/Equals/NotEquals operators.</summary>
    Text = 0,

    /// <summary>Numeric input + ==/!=/&gt;/&lt;/&gt;=/&lt;= operators.</summary>
    Numeric = 1,

    /// <summary>Date input + Before/After/Equals (mapped to LessThan/GreaterThan/Equals).</summary>
    Date = 2
}