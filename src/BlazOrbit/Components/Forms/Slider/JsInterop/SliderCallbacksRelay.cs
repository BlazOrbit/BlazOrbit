using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Components.Forms;

internal sealed class SliderCallbacksRelay : IDisposable
{
    private readonly ISliderJsCallback _callback;

    [DynamicDependency(nameof(OnPointerMove))]
    [DynamicDependency(nameof(OnPointerUp))]
    public SliderCallbacksRelay(ISliderJsCallback callback)
    {
        _callback = callback;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<SliderCallbacksRelay> DotNetReference { get; }

    public void Dispose() => DotNetReference.Dispose();

    [JSInvokable]
    public Task OnPointerMove(double percent)
        => _callback.OnPointerMove(percent);

    [JSInvokable]
    public Task OnPointerUp(double percent)
        => _callback.OnPointerUp(percent);
}
