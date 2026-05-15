using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// JS-side callback contract used by the modal interop relay. Public for
/// reflection / DI but plumbing-only — consumers never implement this.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IModalJsCallback
{
    Task OnEscapePressed();

    Task OnOverlayClick();
}