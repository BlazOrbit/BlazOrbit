using BlazOrbit.Abstractions;
using Microsoft.JSInterop;

namespace BlazOrbit.Hotkeys;

internal sealed class HotkeyJsInterop : ModuleJsInteropBase, IHotkeyJsInterop
{
    // Static-asset URL of the bundled module shipped inside the BlazOrbit.Hotkeys NuGet
    // package. Mirrors the convention used by other peripheral packages (e.g. Charts).
    private const string ModulePath = "./_content/BlazOrbit.Hotkeys/js/Hotkey/HotkeyInterop.min.js";

    public HotkeyJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, ModulePath)
    {
    }

    public async ValueTask AttachAsync(string hostId, DotNetObjectReference<HotkeyHostRelay> relay)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync("attach", hostId, relay);
    }

    public async ValueTask DetachAsync(string hostId)
    {
        // Dispose path: 4-tuple only.
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("detach", hostId);
    }

    public async ValueTask RegisterComboAsync(string hostId, string combo, bool preventDefault)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync("registerCombo", hostId, combo, preventDefault);
    }

    public async ValueTask UnregisterComboAsync(string hostId, string combo)
    {
        // Teardown path: 4-tuple only.
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("unregisterCombo", hostId, combo);
    }
}