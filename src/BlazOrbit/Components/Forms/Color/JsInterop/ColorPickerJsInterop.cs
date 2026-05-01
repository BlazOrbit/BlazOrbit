using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Forms;

/// <summary>JS interop contract for the color picker module.</summary>
public interface IColorPickerJsInterop
{
    /// <summary>Returns the pointer position relative to the given element as <c>[x, y]</c>.</summary>
    ValueTask<double[]> GetRelativePositionAsync(ElementReference element, double clientX, double clientY);
}

internal sealed class ColorPickerJsInterop : ModuleJsInteropBase, IColorPickerJsInterop
{
    public ColorPickerJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.ColorPicker)
    {
    }

    public async ValueTask<double[]> GetRelativePositionAsync(ElementReference element, double clientX, double clientY)
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<double[]>("getRelativePosition", element, clientX, clientY);
    }
}