using BlazOrbit.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Charts.Services.JsInterop;

/// <summary>
/// Default <see cref="IChartJsInterop"/> implementation. Lazily imports
/// <c>_content/BlazOrbit.Charts/js/Types/Chart/ChartInterop.js</c> on the
/// first call and forwards every member to the underlying ES module.
/// <para>
/// The TypeScript source for the bundled module lives at
/// <c>src/BlazOrbit.Charts/wwwroot/js/Types/Chart/ChartInterop.ts</c> and is
/// shipped pre-bundled via the BlazOrbit.Charts.BuildTools chain (added in
/// the bar-chart phase, when JS interop becomes useful).
/// </para>
/// <para>
/// All public methods follow the BlazOrbit teardown contract: each call is
/// wrapped against the four canonical "circuit / runtime gone" exceptions so
/// late invocations during dispose silently no-op instead of bubbling.
/// </para>
/// </summary>
public sealed class ChartJsInterop : ModuleJsInteropBase, IChartJsInterop
{
    private const string ModulePath = "./_content/BlazOrbit.Charts/js/Types/Chart/ChartInterop.min.js";

    /// <summary>
    /// Initializes a new <see cref="ChartJsInterop"/> bound to the supplied
    /// <see cref="IJSRuntime"/>. The JS module is not imported until the
    /// first method call.
    /// </summary>
    /// <param name="jsRuntime">Runtime used to import the chart module.</param>
    public ChartJsInterop(IJSRuntime jsRuntime) : base(jsRuntime, ModulePath)
    {
    }

    /// <inheritdoc />
    public async ValueTask<string> ObserveResizeAsync(ElementReference container, DotNetObjectReference<object> callbackTarget)
    {
        try
        {
            IJSObjectReference module = await ModuleTask.Value;
            return await module.InvokeAsync<string>("observeResize", container, callbackTarget);
        }
        catch (JSDisconnectedException) { return string.Empty; }
        catch (ObjectDisposedException) { return string.Empty; }
        catch (InvalidOperationException) { return string.Empty; }
        catch (TaskCanceledException) { return string.Empty; }
    }

    /// <inheritdoc />
    public async ValueTask UnobserveResizeAsync(string handle)
    {
        if (string.IsNullOrEmpty(handle))
        {
            return;
        }

        try
        {
            IJSObjectReference module = await ModuleTask.Value;
            await module.InvokeVoidAsync("unobserveResize", handle);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
        catch (TaskCanceledException) { }
    }

    /// <inheritdoc />
    public async ValueTask ExportSvgAsPngAsync(ElementReference svg, string fileName, int? width = null, int? height = null)
    {
        try
        {
            IJSObjectReference module = await ModuleTask.Value;
            await module.InvokeVoidAsync("exportSvgAsPng", svg, fileName, width, height);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
        catch (TaskCanceledException) { }
    }
}
