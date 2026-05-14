namespace BlazOrbit.Components.Layout;

/// <summary>Layout direction of a <c>BOBSplitter</c> rail.</summary>
public enum BOBSplitterOrientation
{
    /// <summary>Panes laid out left-to-right, grippers split the row vertically.</summary>
    Horizontal = 0,

    /// <summary>Panes stacked top-to-bottom, grippers split the column horizontally.</summary>
    Vertical = 1
}