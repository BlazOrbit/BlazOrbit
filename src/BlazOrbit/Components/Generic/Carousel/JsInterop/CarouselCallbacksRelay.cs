using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Components;

internal sealed class CarouselCallbacksRelay : IDisposable
{
    private readonly ICarouselJsCallback _callback;

    [DynamicDependency(nameof(OnSwipeLeft))]
    [DynamicDependency(nameof(OnSwipeRight))]
    public CarouselCallbacksRelay(ICarouselJsCallback callback)
    {
        _callback = callback;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<CarouselCallbacksRelay> DotNetReference { get; }

    public void Dispose() => DotNetReference.Dispose();

    [JSInvokable]
    public Task OnSwipeLeft() => _callback.OnSwipeLeftAsync();

    [JSInvokable]
    public Task OnSwipeRight() => _callback.OnSwipeRightAsync();
}
