using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

internal interface IBehaviorJsInterop
{
    /// <summary>
    /// Attaches the configured behaviors to the underlying root element. Returns
    /// <see langword="null"/> when the JS module fails to load (5-tuple swallow) or
    /// during prerender/circuit-tear-down — callers must null-check before storing.
    /// </summary>
    ValueTask<IJSObjectReference?> AttachBehaviorsAsync(BehaviorConfiguration configuration);
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