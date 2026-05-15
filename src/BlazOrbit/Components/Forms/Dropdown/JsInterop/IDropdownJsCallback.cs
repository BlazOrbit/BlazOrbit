using System.ComponentModel;

namespace BlazOrbit.Components.Forms;

/// <summary>
/// JS-side callback contract used by the dropdown interop relay. Public for
/// reflection / DI but plumbing-only — consumers never implement this.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IDropdownJsCallback
{
    /// <summary>JS fires this when the user clicks outside the dropdown popup.</summary>
    Task OnClickOutside();

    /// <summary>JS forwards keystrokes that reach the open popup.</summary>
    Task OnKeyDown(string key, bool shiftKey, bool ctrlKey);

    /// <summary>JS asks for the current trigger/viewport geometry so it can place the popup.</summary>
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