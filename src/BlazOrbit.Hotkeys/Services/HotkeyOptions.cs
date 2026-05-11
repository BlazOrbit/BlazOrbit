namespace BlazOrbit.Hotkeys;

/// <summary>
/// Builder accumulator passed to
/// <c>AddBlazOrbitHotkeys(opts =&gt; opts.Register(...))</c>. Captures app-wide hotkeys
/// declared at composition time so they fire as soon as <c>BOBHotkeyHost</c> renders.
/// </summary>
/// <remarks>
/// Handlers receive the request <see cref="IServiceProvider"/> so they can resolve scoped
/// dependencies (e.g. <c>NavigationManager</c>, <c>IModalService</c>) without capturing
/// state at registration time.
/// </remarks>
public sealed class HotkeyOptions
{
    private readonly List<HotkeyRegistration> _registrations = [];

    internal IReadOnlyList<HotkeyRegistration> Registrations => _registrations;

    /// <summary>Registers a hotkey that fires globally across the app.</summary>
    /// <param name="combo">Canonical combo string (case-insensitive). E.g. <c>"ctrl+k"</c>, <c>"?"</c>.</param>
    /// <param name="description">Short human-friendly label used by cheat-sheet UIs.</param>
    /// <param name="handler">Async callback receiving the request <see cref="IServiceProvider"/>.</param>
    /// <param name="scope">Surfaced via <see cref="HotkeyDescriptor.Scope"/>; defaults to <see cref="HotkeyScope.Global"/>.</param>
    /// <param name="preventDefault">When <see langword="true"/> (default), the JS bridge calls <c>event.preventDefault()</c> after the handler matches.</param>
    public HotkeyOptions Register(
        string combo,
        string description,
        Func<IServiceProvider, Task> handler,
        HotkeyScope scope = HotkeyScope.Global,
        bool preventDefault = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(combo);
        ArgumentNullException.ThrowIfNull(handler);
        _registrations.Add(new HotkeyRegistration(combo, description ?? string.Empty, handler, scope, preventDefault));
        return this;
    }

}

internal sealed record HotkeyRegistration(
    string Combo,
    string Description,
    Func<IServiceProvider, Task> Handler,
    HotkeyScope Scope,
    bool PreventDefault);
