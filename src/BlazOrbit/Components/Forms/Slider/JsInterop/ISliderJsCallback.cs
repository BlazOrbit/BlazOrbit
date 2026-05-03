using System.ComponentModel;

namespace BlazOrbit.Components.Forms;

/// <summary>
/// JS-side callback contract used by the slider interop relay. Public for
/// reflection / DI but plumbing-only — consumers never implement this.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ISliderJsCallback
{
    /// <summary>Invoked on every pointer-move during a drag, with the current 0-100 percent along the track.</summary>
    Task OnPointerMove(double percent);

    /// <summary>Invoked once when the drag ends, with the final 0-100 percent along the track.</summary>
    Task OnPointerUp(double percent);
}
