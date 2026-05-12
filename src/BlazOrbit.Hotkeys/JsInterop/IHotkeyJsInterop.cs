using Microsoft.JSInterop;

namespace BlazOrbit.Hotkeys;

/// <summary>JS bridge that wires a single document-level keydown listener.</summary>
internal interface IHotkeyJsInterop
{
    /// <summary>Installs the keydown listener and registers <paramref name="relay"/> as the dispatcher.</summary>
    ValueTask AttachAsync(string hostId, DotNetObjectReference<HotkeyHostRelay> relay);

    /// <summary>Removes the keydown listener.</summary>
    ValueTask DetachAsync(string hostId);

    /// <summary>
    /// Pushes a combo to the JS-side registry so the keydown handler can call
    /// <c>event.preventDefault()</c> synchronously on match. Without this, the .NET
    /// roundtrip resolves after the browser has already executed the default action
    /// (e.g. <c>Ctrl+S</c> Save dialog).
    /// </summary>
    ValueTask RegisterComboAsync(string hostId, string combo, bool preventDefault);

    /// <summary>Removes a combo from the JS-side registry.</summary>
    ValueTask UnregisterComboAsync(string hostId, string combo);
}
