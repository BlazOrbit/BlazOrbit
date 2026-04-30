using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

internal interface IBehaviorJsInterop
{
    ValueTask<IJSObjectReference> AttachBehaviorsAsync(BehaviorConfiguration configuration);
}

internal sealed class BehaviorConfiguration
{
    public bool HasAnyBehavior => Ripple != null;
    public RippleConfiguration? Ripple { get; set; }
}

internal sealed class RippleConfiguration
{
    public string? Color { get; set; }
    public int? Duration { get; set; }

    public ElementReference RippleContainer { get; set; }
}