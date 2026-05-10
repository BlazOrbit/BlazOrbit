using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Charts.Services.JsInterop;

/// <summary>
/// Minimal JS interop surface required by BOBCharts. Concrete implementations
/// live in the same package and lazy-load
/// <c>_content/BlazOrbit.Charts/js/Types/Chart/ChartInterop.js</c>.
/// <para>
/// The chart components themselves never call <see cref="IJSRuntime"/>
/// directly; everything goes through this interface so JS-less hosts (static
/// SSR, prerender) can plug in a no-op implementation.
/// </para>
/// </summary>
public interface IChartJsInterop : IAsyncDisposable
{
    /// <summary>
    /// Installs a <c>ResizeObserver</c> on the supplied container element.
    /// Each observed resize re-invokes <c>OnResize(width, height)</c> on the
    /// caller's <see cref="DotNetObjectReference{T}"/>. Returns an opaque
    /// handle that must be passed to <see cref="UnobserveResizeAsync"/> to
    /// detach the observer.
    /// </summary>
    /// <param name="container">Element reference of the chart's host.</param>
    /// <param name="callbackTarget">.NET object exposing <c>OnResize</c>.</param>
    /// <returns>Handle that identifies this observer registration.</returns>
    ValueTask<string> ObserveResizeAsync(ElementReference container, DotNetObjectReference<object> callbackTarget);

    /// <summary>
    /// Removes a previously installed <c>ResizeObserver</c>. Idempotent: a
    /// missing handle is a no-op.
    /// </summary>
    /// <param name="handle">Handle previously returned by <see cref="ObserveResizeAsync"/>.</param>
    ValueTask UnobserveResizeAsync(string handle);

    /// <summary>
    /// Serializes the supplied <c>&lt;svg&gt;</c> element to a PNG image and
    /// triggers a browser download. Runs entirely client-side; the server
    /// never sees the image bytes.
    /// </summary>
    /// <param name="svg">Element reference of the chart's <c>&lt;svg&gt;</c> root.</param>
    /// <param name="fileName">Suggested file name (without extension).</param>
    /// <param name="width">Optional explicit pixel width for the rasterized image.</param>
    /// <param name="height">Optional explicit pixel height for the rasterized image.</param>
    ValueTask ExportSvgAsPngAsync(ElementReference svg, string fileName, int? width = null, int? height = null);
}
