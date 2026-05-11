namespace BlazOrbit.Hotkeys;

/// <summary>
/// Snapshot of a registered hotkey, exposed via <see cref="IHotkeyService.RegisteredHotkeys"/>
/// so a cheat-sheet UI can render the live list.
/// </summary>
/// <param name="Combo">Canonical combo string (e.g. <c>"ctrl+s"</c>).</param>
/// <param name="Description">Human-friendly label shown in the cheat-sheet.</param>
/// <param name="Scope">Scope of the entry.</param>
public readonly record struct HotkeyDescriptor(string Combo, string Description, HotkeyScope Scope);
