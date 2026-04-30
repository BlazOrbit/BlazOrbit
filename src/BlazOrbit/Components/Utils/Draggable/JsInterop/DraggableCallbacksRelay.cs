using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Components;

internal sealed class DraggableCallbacksRelay : IDisposable
{
    private readonly IDraggableJsCallback _callback;

    [DynamicDependency(nameof(OnMouseMove))]
    [DynamicDependency(nameof(OnMouseUp))]
    public DraggableCallbacksRelay(IDraggableJsCallback callback)
    {
        _callback = callback;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<DraggableCallbacksRelay> DotNetReference { get; }

    public void Dispose() => DotNetReference.Dispose();

    [JSInvokable]
    public Task OnMouseMove(double clientX, double clientY)
        => _callback.OnMouseMove(clientX, clientY);

    [JSInvokable]
    public Task OnMouseUp(double clientX, double clientY)
        => _callback.OnMouseUp(clientX, clientY);
}