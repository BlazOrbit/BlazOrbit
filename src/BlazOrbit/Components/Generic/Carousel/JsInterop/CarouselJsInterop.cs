using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

internal interface ICarouselJsInterop
{
    ValueTask AttachAsync(
        ElementReference element,
        DotNetObjectReference<CarouselCallbacksRelay> dotnetReference,
        string componentId,
        int swipeThresholdPx);

    ValueTask DetachAsync(string componentId);
}

internal sealed class CarouselJsInterop : ModuleJsInteropBase, ICarouselJsInterop
{
    public CarouselJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Carousel)
    {
    }

    public async ValueTask AttachAsync(
        ElementReference element,
        DotNetObjectReference<CarouselCallbacksRelay> dotnetReference,
        string componentId,
        int swipeThresholdPx)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("initialize", element, dotnetReference, componentId, swipeThresholdPx);
    }

    public async ValueTask DetachAsync(string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("dispose", componentId);
    }
}
