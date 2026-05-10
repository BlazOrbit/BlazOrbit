using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Charts.BuildTools.Generators;

/// <summary>
/// Scaffolds the chart-family TypeScript interop module skeleton at
/// <c>Types/Chart/ChartInterop.ts</c>. The module is the source-of-truth
/// for the JS exports the C# components consume (ResizeObserver wiring,
/// SVG-to-PNG export). Idempotent — runs at build but does not overwrite
/// when the target file already exists in the consuming project.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ChartInteropTypeScriptGenerator : IAssetGenerator
{
    public string FileName => "Types/Chart/ChartInterop.ts";
    public string Name => "Chart Interop TypeScript Skeleton";

    public Task<string> GetContent() => Task.FromResult("""
// ============================================================================
// BlazOrbit.Charts — JS interop entry point.
// Auto-generated skeleton. Safe to edit; rerunning the generator does not
// overwrite the file when it already exists.
// ============================================================================

interface ChartObserverEntry {
    observer: ResizeObserver;
    dotnet: any;
}

const observers = new WeakMap<Element, ChartObserverEntry>();

export function observeResize(element: Element, dotnet: any): void {
    if (!element || observers.has(element)) {
        return;
    }
    const observer = new ResizeObserver(entries => {
        for (const entry of entries) {
            const { width, height } = entry.contentRect;
            try {
                dotnet.invokeMethodAsync("OnResize", width, height);
            } catch {
                // .NET object may have been disposed mid-flight; safe to ignore.
            }
        }
    });
    observer.observe(element);
    observers.set(element, { observer, dotnet });
}

export function unobserveResize(element: Element): void {
    const entry = observers.get(element);
    if (entry) {
        entry.observer.disconnect();
        observers.delete(element);
    }
}

export async function exportSvgAsPng(svgEl: SVGSVGElement, fileName: string): Promise<void> {
    const xml = new XMLSerializer().serializeToString(svgEl);
    const svg64 = btoa(unescape(encodeURIComponent(xml)));
    const dataUrl = `data:image/svg+xml;base64,${svg64}`;
    const img = new Image();
    img.src = dataUrl;
    await img.decode();

    const canvas = document.createElement("canvas");
    canvas.width = svgEl.clientWidth || 600;
    canvas.height = svgEl.clientHeight || 400;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;
    ctx.fillStyle = "#ffffff";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(img, 0, 0);

    const a = document.createElement("a");
    a.download = fileName.endsWith(".png") ? fileName : `${fileName}.png`;
    a.href = canvas.toDataURL("image/png");
    a.click();
}
""");
}
