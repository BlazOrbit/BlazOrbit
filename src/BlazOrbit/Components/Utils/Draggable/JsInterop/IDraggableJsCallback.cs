using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// JS-side callback contract used by the draggable interop relay. Public for
/// reflection / DI but plumbing-only — consumers never implement this.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IDraggableJsCallback
{
    Task OnMouseMove(double clientX, double clientY);

    Task OnMouseUp(double clientX, double clientY);
}