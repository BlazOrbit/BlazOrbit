namespace BlazOrbit.Components.Utils.Draggable;

/// <summary>Pointer position payload for drag events raised by <c>BOBDraggable</c>.</summary>
public readonly struct DragEventArgs
{
    /// <summary>Pointer X position in client (viewport) coordinates.</summary>
    public double ClientX { get; init; }
    /// <summary>Pointer Y position in client (viewport) coordinates.</summary>
    public double ClientY { get; init; }
}