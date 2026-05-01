using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

/// <summary>JS interop contract for clipboard operations.</summary>
public interface IClipboardJsInterop
{
    /// <summary>Copies <paramref name="text"/> to the system clipboard.</summary>
    ValueTask CopyTextAsync(string text);
}

internal sealed class ClipboardJsInterop
    : ModuleJsInteropBase, IClipboardJsInterop
{
    public ClipboardJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Clipboard)
    {
    }

    public async ValueTask CopyTextAsync(string text)
    {
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("copyText", text);
    }
}