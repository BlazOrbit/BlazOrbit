namespace BlazOrbit.Components.Layout;

/// <summary>Cross-axis alignment for grid items.</summary>
public enum GridAlignItems
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

/// <summary>Per-item override for cross-axis alignment.</summary>
public enum GridAlignSelf
{
    /// <summary>Inherits from the parent's <see cref="GridAlignItems"/> value.</summary>
    Auto,
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

/// <summary>Maps to the grid container's flex-direction equivalent.</summary>
public enum GridDirection
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

/// <summary>Main-axis distribution for grid items.</summary>
public enum GridJustifyContent
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

/// <summary>Wrapping behavior for grid rows.</summary>
public enum GridWrap
{
    /// <summary>Wrap onto multiple lines.</summary>
    Wrap,
    /// <summary>Single line — items shrink to fit.</summary>
    NoWrap,
    /// <summary>Wrap onto multiple lines in reverse order.</summary>
    WrapReverse
}
