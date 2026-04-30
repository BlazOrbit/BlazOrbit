using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Components.Forms;

internal sealed class DropdownCallbacksRelay : IDisposable
{
    private readonly IDropdownJsCallback _callback;

    [DynamicDependency(nameof(OnClickOutside))]
    [DynamicDependency(nameof(OnKeyDown))]
    [DynamicDependency(nameof(OnRequestPosition))]
    public DropdownCallbacksRelay(IDropdownJsCallback callback)
    {
        _callback = callback;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<DropdownCallbacksRelay> DotNetReference { get; }

    public void Dispose() => DotNetReference.Dispose();

    [JSInvokable]
    public Task OnClickOutside()
        => _callback.OnClickOutside();

    [JSInvokable]
    public Task OnKeyDown(string key, bool shiftKey, bool ctrlKey)
        => _callback.OnKeyDown(key, shiftKey, ctrlKey);

    [JSInvokable]
    public Task<DropdownPosition> OnRequestPosition()
        => _callback.OnRequestPosition();
}