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
    /// Raised when a new hotkey is registered. <c>BOBHotkeyHost</c> subscribes so it can
    /// push the descriptor to the JS bridge, which is what enables <em>synchronous</em>
    /// <c>preventDefault()</c> on match — async dispatch alone is too late to suppress
    /// the browser's default action (e.g. <c>Ctrl+S</c> Save dialog).
    /// </summary>
    event Action<HotkeyDescriptor>? Registered;

    /// <summary>Raised when a previously-registered hotkey is removed (via the returned <see cref="IDisposable"/>).</summary>
    event Action<HotkeyDescriptor>? Unregistered;

    /// <summary>
    /// Registers <paramref name="handler"/> against <paramref name="combo"/>. The returned
    /// <see cref="IDisposable"/> removes the entry — call it from <c>IDisposable.Dispose</c>
    /// to scope the shortcut to a page / component lifetime.
    /// </summary>
    /// <param name="combo">Canonical combo string. Case-insensitive; whitespace around <c>+</c> is OK.</param>
    /// <param name="description">Short human label used by the cheat-sheet.</param>
    /// <param name="handler">Async callback invoked when the combo fires.</param>
    /// <param name="scope">Scope tag; surfaced via <see cref="RegisteredHotkeys"/>.</param>
    /// <param name="preventDefault">When <see langword="true"/>, the JS bridge calls <c>event.preventDefault()</c> synchronously on match so the browser's default action never fires.</param>
    IDisposable Register(
        string combo,
        string description,
        Func<Task> handler,
        HotkeyScope scope = HotkeyScope.Global,
        bool preventDefault = true);

    /// <summary>
    /// Internal hook called by <c>BOBHotkeyHost</c> when JS dispatches a keydown. Returns
    /// <see langword="true"/> when at least one handler matched and requested preventDefault
    /// — retained as a fallback signal, but the synchronous suppression now lives in JS via
    /// the <see cref="Registered"/> / <see cref="Unregistered"/> events.
    /// </summary>
    Task<bool> DispatchAsync(string combo);
}
