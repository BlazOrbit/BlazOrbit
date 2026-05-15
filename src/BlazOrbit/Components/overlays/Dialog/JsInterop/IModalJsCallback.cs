using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// JS-side callback contract used by the modal interop relay. Public for
/// reflection / DI but plumbing-only — consumers never implement this.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IModalJsCallback
{
    /// <summary>JS fires this when the user presses Escape while the modal is open.</summary>
    Task OnEscapePressed();

    /// <summary>JS fires this when the user clicks the modal overlay (outside the dialog box).</summary>
    Task OnOverlayClick();
}