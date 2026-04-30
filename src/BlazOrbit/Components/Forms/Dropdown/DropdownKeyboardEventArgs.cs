namespace BlazOrbit.Components.Forms;

/// <summary>
/// Provides keyboard event data for dropdown interactions.
/// </summary>
/// <param name="Key">The key that was pressed.</param>
/// <param name="ShiftKey">Whether the Shift key was held.</param>
/// <param name="CtrlKey">Whether the Control key was held.</param>
public readonly record struct DropdownKeyboardEventArgs(string Key, bool ShiftKey, bool CtrlKey);
