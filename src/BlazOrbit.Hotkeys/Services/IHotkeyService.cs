namespace BlazOrbit.Hotkeys;

/// <summary>
/// Registry of keyboard shortcuts. The matching <c>BOBHotkeyHost</c> component installs
/// a <c>document</c> keydown listener via JS interop and forwards the canonical combo to
/// every registered handler.
/// </summary>
/// <remarks>
/// Combo grammar (case-insensitive, modifiers before the key, fixed order):
/// <c>"ctrl+shift+s"</c>, <c>"meta+k"</c>, <c>"?"</c>, <c>"escape"</c>. The bridge skips
/// keystrokes that originate inside <c>&lt;input&gt;</c>, <c>&lt;textarea&gt;</c>,
/// <c>&lt;select&gt;</c> or <c>contentEditable</c> targets — typed text never collides
/// with shortcuts.
/// </remarks>
public interface IHotkeyService
{
    /// <summary>Currently registered hotkeys, useful for a cheat-sheet UI.</summary>
    IReadOnlyList<HotkeyDescriptor> RegisteredHotkeys { get; }

    /// <summary>
    /// Registers <paramref name="handler"/> against <paramref name="combo"/>. The returned
    /// <see cref="IDisposable"/> removes the entry — call it from <c>IDisposable.Dispose</c>
    /// to scope the shortcut to a page / component lifetime.
    /// </summary>
    /// <param name="combo">Canonical combo string. Case-insensitive; whitespace around <c>+</c> is OK.</param>
    /// <param name="description">Short human label used by the cheat-sheet.</param>
    /// <param name="handler">Async callback invoked when the combo fires.</param>
    /// <param name="scope">Scope tag; surfaced via <see cref="RegisteredHotkeys"/>.</param>
    /// <param name="preventDefault">When <see langword="true"/>, the JS bridge calls <c>event.preventDefault()</c> after the handler matches.</param>
    IDisposable Register(
        string combo,
        string description,
        Func<Task> handler,
        HotkeyScope scope = HotkeyScope.Global,
        bool preventDefault = true);

    /// <summary>
    /// Internal hook called by <c>BOBHotkeyHost</c> when JS dispatches a keydown. Returns
    /// <see langword="true"/> when at least one handler matched and requested preventDefault.
    /// </summary>
    Task<bool> DispatchAsync(string combo);
}
