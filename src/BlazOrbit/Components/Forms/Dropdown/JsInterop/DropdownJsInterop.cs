using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Forms;

internal interface IDropdownJsInterop
{
    ValueTask DisposeAsync(string componentId);

    ValueTask FocusSearchInputAsync(string componentId);

    ValueTask<DropdownPosition> GetPositionAsync(string componentId);

    ValueTask InitializeAsync(
                    ElementReference triggerElement,
        ElementReference menuElement,
        DotNetObjectReference<DropdownCallbacksRelay> dotnetReference,
        string componentId);
}

internal sealed class DropdownJsInterop : ModuleJsInteropBase, IDropdownJsInterop
{
    public DropdownJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Dropdown)
    {
    }

    public async ValueTask DisposeAsync(string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("dispose", componentId);
    }

    public async ValueTask FocusSearchInputAsync(string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("focusSearchInput", componentId);
    }

    public async ValueTask<DropdownPosition> GetPositionAsync(string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;

        return await module.InvokeAsync<DropdownPosition>("getPosition", componentId);
    }

    public async ValueTask InitializeAsync(
                    ElementReference triggerElement,
        ElementReference menuElement,
        DotNetObjectReference<DropdownCallbacksRelay> dotnetReference,
        string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "initialize",
            triggerElement,
            menuElement,
            dotnetReference,
            componentId);
    }
}