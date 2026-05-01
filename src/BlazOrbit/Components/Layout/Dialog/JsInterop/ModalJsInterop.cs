using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Layout;

/// <summary>JS interop contract for the modal/dialog/drawer module.</summary>
public interface IModalJsInterop
{
    /// <summary>Locks document scroll while a modal is open.</summary>
    ValueTask LockScrollAsync();

    /// <summary>Releases the focus trap previously installed via <see cref="TrapFocusAsync"/>.</summary>
    ValueTask ReleaseFocusAsync(string trapId);

    /// <summary>Installs a focus trap inside <paramref name="element"/>, identified by <paramref name="trapId"/>.</summary>
    ValueTask TrapFocusAsync(ElementReference element, string trapId);

    /// <summary>Releases the document-scroll lock installed by <see cref="LockScrollAsync"/>.</summary>
    ValueTask UnlockScrollAsync();

    /// <summary>Resolves once the element's animation ends, or after <paramref name="fallbackMs"/> as a safety net.</summary>
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