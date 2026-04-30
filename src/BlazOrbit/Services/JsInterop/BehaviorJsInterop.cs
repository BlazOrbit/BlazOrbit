using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

internal sealed class BehaviorJsInterop : ModuleJsInteropBase, IBehaviorJsInterop
{
    public BehaviorJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.BehaviorsJs)
    {
    }

    public async ValueTask<IJSObjectReference> AttachBehaviorsAsync(BehaviorConfiguration configuration)
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<IJSObjectReference>(
            "initialize", configuration);
    }
}