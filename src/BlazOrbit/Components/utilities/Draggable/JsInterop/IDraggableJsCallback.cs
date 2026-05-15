using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// JS-side callback contract used by the draggable interop relay. Public for
/// reflection / DI but plumbing-only — consumers never implement this.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IDraggableJsCallback
{
    /// <summary>JS forwards each pointer move while the draggable is captured.</summary>
    Task OnMouseMove(double clientX, double clientY);

    /// <summary>JS fires once when the user releases the pointer to end the drag.</summary>
    Task OnMouseUp(double clientX, double clientY);
}