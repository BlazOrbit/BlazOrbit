using Microsoft.JSInterop;

namespace BlazOrbit.Hotkeys;

/// <summary>
/// Bridge object the JS hotkey listener calls back into. Held alive by
/// <c>BOBHotkeyHost</c> via a <see cref="DotNetObjectReference{T}"/>; do not register
/// directly in DI.
/// </summary>
public sealed class HotkeyHostRelay
{
    private readonly IHotkeyService _service;

    /// <summary>Initializes a new <see cref="HotkeyHostRelay"/>.</summary>
    public HotkeyHostRelay(IHotkeyService service) => _service = service;

    /// <summary>Invoked by the JS bridge for every keydown event. Returns whether the original event should be cancelled.</summary>
    [JSInvokable]
    public Task<bool> OnHotkey(string combo) => _service.DispatchAsync(combo);
}