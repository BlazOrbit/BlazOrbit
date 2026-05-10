using Microsoft.JSInterop;

namespace BlazOrbit.Hotkeys;

/// <summary>JS bridge that wires a single document-level keydown listener.</summary>
internal interface IHotkeyJsInterop
{
    /// <summary>Installs the keydown listener and registers <paramref name="relay"/> as the dispatcher.</summary>
    ValueTask AttachAsync(string hostId, DotNetObjectReference<HotkeyHostRelay> relay);

    /// <summary>Removes the keydown listener.</summary>
    ValueTask DetachAsync(string hostId);
}
