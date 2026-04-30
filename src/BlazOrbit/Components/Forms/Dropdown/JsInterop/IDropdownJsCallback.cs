using System.ComponentModel;

namespace BlazOrbit.Components.Forms;

/// <summary>
/// JS-side callback contract used by the dropdown interop relay. Public for
/// reflection / DI but plumbing-only — consumers never implement this.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IDropdownJsCallback
{
    Task OnClickOutside();

    Task OnKeyDown(string key, bool shiftKey, bool ctrlKey);

    Task<DropdownPosition> OnRequestPosition();
}

/// <summary>JS-interop payload describing the dropdown trigger / viewport rectangle.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public record struct DropdownPosition(
    double TriggerTop,
    double TriggerLeft,
    double TriggerWidth,
    double TriggerHeight,
    double ViewportHeight,
    double ViewportWidth,
    double ScrollY);