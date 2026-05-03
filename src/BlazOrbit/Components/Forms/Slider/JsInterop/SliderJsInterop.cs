using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Forms;

internal interface ISliderJsInterop
{
    ValueTask StartDragAsync(
        ElementReference trackElement,
        DotNetObjectReference<SliderCallbacksRelay> dotnetReference,
        string componentId,
        string orientation,
        double initialClientX,
        double initialClientY);

    ValueTask StopDragAsync(string componentId);

    ValueTask<double> ComputePercentAsync(
        ElementReference trackElement,
        string orientation,
        double clientX,
        double clientY);
}

internal sealed class SliderJsInterop : ModuleJsInteropBase, ISliderJsInterop
{
    public SliderJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Slider)
    {
    }

    public async ValueTask StartDragAsync(
        ElementReference trackElement,
        DotNetObjectReference<SliderCallbacksRelay> dotnetReference,
        string componentId,
        string orientation,
        double initialClientX,
        double initialClientY)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync(
            "startDrag",
            trackElement,
            dotnetReference,
            componentId,
            orientation,
            initialClientX,
            initialClientY);
    }

    public async ValueTask StopDragAsync(string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("stopDrag", componentId);
    }

    public async ValueTask<double> ComputePercentAsync(
        ElementReference trackElement,
        string orientation,
        double clientX,
        double clientY)
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<double>(
            "computePercent",
            trackElement,
            orientation,
            clientX,
            clientY);
    }
}
