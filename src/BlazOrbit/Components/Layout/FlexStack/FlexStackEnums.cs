namespace BlazOrbit.Components.Layout;

/// <summary>Maps to CSS <c>flex-direction</c>.</summary>
public enum FlexStackDirection
{
    /// <summary>Inline axis, start-to-end.</summary>
    Row,
    /// <summary>Inline axis, end-to-start.</summary>
    RowReverse,
    /// <summary>Block axis, start-to-end.</summary>
    Column,
    /// <summary>Block axis, end-to-start.</summary>
    ColumnReverse
}

/// <summary>Maps to CSS <c>flex-wrap</c>.</summary>
public enum FlexStackWrap
{
    /// <summary>Wraps onto multiple lines.</summary>
    Wrap,
    /// <summary>Single line, items shrink to fit.</summary>
    NoWrap,
    /// <summary>Wraps onto multiple lines in reverse order.</summary>
    WrapReverse
}

/// <summary>Maps to CSS <c>justify-content</c> on the main axis.</summary>
public enum FlexStackJustifyContent
{
    /// <summary>Pack to the start of the main axis.</summary>
    Start,
    /// <summary>Pack to the end of the main axis.</summary>
    End,
    /// <summary>Center on the main axis.</summary>
    Center,
    /// <summary>Distribute with equal space between items.</summary>
    SpaceBetween,
    /// <summary>Distribute with equal space around each item.</summary>
    SpaceAround,
    /// <summary>Distribute with equal space between and around items.</summary>
    SpaceEvenly
}

/// <summary>Maps to CSS <c>align-items</c> on the cross axis.</summary>
public enum FlexStackAlignItems
{
    /// <summary>Align to the start of the cross axis.</summary>
    Start,
    /// <summary>Align to the end of the cross axis.</summary>
    End,
    /// <summary>Center on the cross axis.</summary>
    Center,
    /// <summary>Stretch to fill the cross axis.</summary>
    Stretch,
    /// <summary>Align baselines.</summary>
    Baseline
}

/// <summary>Maps to CSS <c>align-content</c> for multi-line flex containers.</summary>
public enum FlexStackAlignContent
{
    /// <summary>Pack lines to the start of the cross axis.</summary>
    Start,
    /// <summary>Pack lines to the end of the cross axis.</summary>
    End,
    /// <summary>Center lines on the cross axis.</summary>
    Center,
    /// <summary>Stretch lines to fill the cross axis.</summary>
    Stretch,
    /// <summary>Distribute lines with equal space between.</summary>
    SpaceBetween,
    /// <summary>Distribute lines with equal space around.</summary>
    SpaceAround,
    /// <summary>Distribute lines with equal space between and around.</summary>
    SpaceEvenly
}
