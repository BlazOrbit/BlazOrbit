using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

internal interface IDraggableJsInterop
{
    ValueTask StartDragAsync(
        ElementReference element,
        DotNetObjectReference<DraggableCallbacksRelay> dotnetReference,
        string componentId);

    ValueTask StopDragAsync(string componentId);
}

internal sealed class DraggableJsInterop : ModuleJsInteropBase, IDraggableJsInterop
{
    public DraggableJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Draggable)
    {
    }

    public async ValueTask StartDragAsync(
        ElementReference element,
        DotNetObjectReference<DraggableCallbacksRelay> dotnetReference,
        string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("initialize", element, dotnetReference, componentId);
    }

    public async ValueTask StopDragAsync(string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("dispose", componentId);
    }
}