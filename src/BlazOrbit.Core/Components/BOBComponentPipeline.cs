using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace BlazOrbit.Abstractions;

/// <summary>
/// Shared pipeline composed by every BOB component base class. Owns the
/// <see cref="BOBComponentAttributesBuilder"/>, the JS behavior handle and the DEBUG-only
/// performance instrumentation so that <see cref="BOBComponentBase"/> (which derives from
/// <see cref="ComponentBase"/>) and <see cref="BOBInputComponentBase{TValue}"/> (which must derive
/// from <c>InputBase&lt;TValue&gt;</c> for <c>EditContext</c> participation) share the same code
/// path instead of re-implementing the style+behavior lifecycle twice.
/// </summary>
internal sealed class BOBComponentPipeline
{
    private readonly BOBComponentAttributesBuilder _styleBuilder = new();
    private IJSObjectReference? _behaviorInstance;

#if DEBUG
    private readonly Stopwatch _stopwatch = new();
    private Stopwatch? _initStopwatch;
#endif

    public Dictionary<string, object> ComputedAttributes => _styleBuilder.ComputedAttributes;

    /// <summary>
    /// PERF-08: forwards <see cref="BOBComponentAttributesBuilder.LastBuildSkipped"/> so the
    /// input echo-guard in <see cref="BOBInputComponentBase{TValue}"/> can suppress redundant
    /// render-tree builds when no style-affecting parameter changed.
    /// </summary>
    public bool LastBuildSkipped => _styleBuilder.LastBuildSkipped;

    public void BuildStyles(
        ComponentBase component,
        IReadOnlyDictionary<string, object>? additionalAttributes)
        => _styleBuilder.BuildStyles(component, additionalAttributes);

    public void PatchVolatileAttributes(ComponentBase component)
        => _styleBuilder.PatchVolatileAttributes(component);

    public async Task AttachBehaviorAsync(
        ComponentBase component,
        IBehaviorJsInterop behaviorJs)
    {
        _behaviorInstance = await BOBComponentJsBehaviorBuilder
            .For(component, behaviorJs)
            .BuildAndAttachAsync();
    }

    public async ValueTask DisposeBehaviorAsync()
    {
        if (_behaviorInstance == null)
        {
            return;
        }

        try
        {
            await _behaviorInstance.InvokeVoidAsync("dispose");
            await _behaviorInstance.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Blazor Server circuit disconnected — behavior already gone.
        }
        catch (ObjectDisposedException)
        {
            // Runtime already disposed.
        }
        catch (InvalidOperationException)
        {
            // No active JS runtime (prerender / server shutdown).
        }
        catch (TaskCanceledException)
        {
            // Disposal raced with an in-flight call.
        }
    }

    // DEBUG-only perf hooks. [Conditional("DEBUG")] elides call sites in Release so there is zero
    // runtime cost when not tracking. Stopwatch fields are themselves behind #if DEBUG.

    [Conditional("DEBUG")]
    public void BeginInit()
    {
#if DEBUG
        _initStopwatch = Stopwatch.StartNew();
#endif
    }

    [Conditional("DEBUG")]
    public void EndInit(string componentType, IBOBPerformanceService? performanceService, bool enabled)
    {
#if DEBUG
        if (!enabled)
        {
            return;
        }

        _initStopwatch?.Stop();
        performanceService?.RecordInit(componentType, _initStopwatch?.Elapsed.TotalMilliseconds ?? 0);
#endif
    }

    [Conditional("DEBUG")]
    public void BeginParametersSet()
    {
#if DEBUG
        _stopwatch.Restart();
#endif
    }

    [Conditional("DEBUG")]
    public void EndParametersSet(string componentType, IBOBPerformanceService? performanceService, bool enabled)
    {
#if DEBUG
        if (!enabled)
        {
            return;
        }

        _stopwatch.Stop();
        performanceService?.RecordParametersSet(componentType, _stopwatch.Elapsed.TotalMilliseconds);
#endif
    }

    [Conditional("DEBUG")]
    public void BeginRenderTree()
    {
#if DEBUG
        _stopwatch.Restart();
#endif
    }

    [Conditional("DEBUG")]
    public void EndRenderTree(string componentType, IBOBPerformanceService? performanceService, bool enabled)
    {
#if DEBUG
        if (!enabled)
        {
            return;
        }

        _stopwatch.Stop();
        performanceService?.RecordRenderTreeBuild(componentType, _stopwatch.Elapsed.TotalMilliseconds);
#endif
    }
}
