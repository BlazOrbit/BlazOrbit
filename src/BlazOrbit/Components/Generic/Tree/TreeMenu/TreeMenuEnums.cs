namespace BlazOrbit.Components;

/// <summary>Layout orientation for a <c>BOBTreeMenu</c>.</summary>
public enum TreeMenuOrientation
{
    /// <summary>Stacked top-to-bottom.</summary>
    Vertical,
    /// <summary>Laid out left-to-right.</summary>
    Horizontal
}

/// <summary>How a submenu opens.</summary>
public enum TreeMenuTrigger
{
    /// <summary>Submenu opens on click.</summary>
    Click,
    /// <summary>Submenu opens on hover.</summary>
    Hover
}

/// <summary>How children are revealed when a parent expands.</summary>
public enum TreeMenuExpandMode
{
    /// <summary>Children are pushed into the document flow underneath the parent.</summary>
    Inline,
    /// <summary>Children float over surrounding content as a flyout.</summary>
    Flyout
}