namespace BlazOrbit.Components;

/// <summary>Placement of a tooltip relative to its anchor element.</summary>
public enum TooltipPlacement
{
    /// <summary>Above, centered on the anchor.</summary>
    Top,

    /// <summary>Above, aligned with the anchor's start edge.</summary>
    TopStart,

    /// <summary>Above, aligned with the anchor's end edge.</summary>
    TopEnd,

    /// <summary>Below, centered on the anchor.</summary>
    Bottom,

    /// <summary>Below, aligned with the anchor's start edge.</summary>
    BottomStart,

    /// <summary>Below, aligned with the anchor's end edge.</summary>
    BottomEnd,

    /// <summary>Left of the anchor, centered vertically.</summary>
    Left,

    /// <summary>Left of the anchor, aligned with its top edge.</summary>
    LeftStart,

    /// <summary>Left of the anchor, aligned with its bottom edge.</summary>
    LeftEnd,

    /// <summary>Right of the anchor, centered vertically.</summary>
    Right,

    /// <summary>Right of the anchor, aligned with its top edge.</summary>
    RightStart,

    /// <summary>Right of the anchor, aligned with its bottom edge.</summary>
    RightEnd
}

/// <summary>How a tooltip becomes visible.</summary>
public enum TooltipTrigger
{
    /// <summary>Show on pointer hover.</summary>
    Hover,

    /// <summary>Show on keyboard focus.</summary>
    Focus,

    /// <summary>Show on either hover or focus.</summary>
    HoverAndFocus,

    /// <summary>Show on click; click outside / Escape to dismiss.</summary>
    Click,

    /// <summary>Always visible.</summary>
    Permanent
}
