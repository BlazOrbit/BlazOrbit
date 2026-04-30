using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Layout;

public interface IModalJsInterop
{
    ValueTask LockScrollAsync();

    ValueTask ReleaseFocusAsync(string trapId);

    ValueTask TrapFocusAsync(ElementReference element, string trapId);

    ValueTask UnlockScrollAsync();

    ValueTask WaitForAnimationEndAsync(ElementReference element, int fallbackMs);
}

internal sealed class ModalJsInterop : ModuleJsInteropBase, IModalJsInterop
{
    public ModalJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Modal)
    {
    }

    public async ValueTask LockScrollAsync()
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("lockScroll");
    }

    public async ValueTask ReleaseFocusAsync(string trapId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("releaseFocus", trapId);
    }

    public async ValueTask TrapFocusAsync(ElementReference element, string trapId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("trapFocus", element, trapId);
    }

    public async ValueTask UnlockScrollAsync()
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("unlockScroll");
    }

    public async ValueTask WaitForAnimationEndAsync(ElementReference element, int fallbackMs)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("waitForAnimationEnd", element, fallbackMs);
    }
}