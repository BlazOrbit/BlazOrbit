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
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("attach", hostId, relay);
    }

    public async ValueTask DetachAsync(string hostId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("detach", hostId);
    }
}
