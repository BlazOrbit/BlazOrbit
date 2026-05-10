/**
 * BlazOrbit.Charts — JS interop module (TypeScript source of truth).
 *
 * Bundled to `wwwroot/js/Types/Chart/ChartInterop.min.js` via esbuild on each
 * build (see the `BundleChartInterop` MSBuild target in BlazOrbit.Charts.csproj).
 * The compiled `.min.js` is committed so builds without Node still work — the
 * MSBuild target only re-runs when esbuild is locally available.
 *
 * API surface intentionally minimal: three exports matching `IChartJsInterop`.
 *
 * On import, the module also auto-injects the `blazorbit-charts.css` <link>
 * into <head> so consumers don't need to wire it manually in index.html /
 * App.razor — the CSS sibling path is computed from `import.meta.url`.
 */

interface DotNetObjectRef {
    invokeMethodAsync<T = unknown>(methodName: string, ...args: unknown[]): Promise<T>;
    dispose(): void;
}

/**
 * Self-installing stylesheet hook. Idempotent: if a <link> already points to
 * `blazorbit-charts.css` (user added it manually, or the module ran twice
 * across hot reloads) we skip. Otherwise we resolve the CSS sibling URL via
 * `import.meta.url` so the static-asset pipeline's fingerprint / base href
 * resolution propagates without hand-coding `_content/BlazOrbit.Charts/...`.
 */
function ensureStylesheet(): void {
    if (typeof document === 'undefined') return;

    const head = document.head ?? document.getElementsByTagName('head')[0];
    if (!head) return;

    // Already-linked check — covers manual links + re-imports.
    const existing = document.querySelectorAll('link[rel="stylesheet"]');
    for (let i = 0; i < existing.length; i++) {
        const href = (existing[i] as HTMLLinkElement).href;
        if (href && href.indexOf('blazorbit-charts.css') !== -1) return;
    }

    let cssHref: string;
    try {
        cssHref = new URL('../../../css/blazorbit-charts.css', import.meta.url).href;
    } catch {
        // Some bundlers strip import.meta.url at minify-time; fall back to
        // the canonical RCL static-asset path.
        cssHref = '_content/BlazOrbit.Charts/css/blazorbit-charts.css';
    }

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = cssHref;
    link.setAttribute('data-bob-charts-css', 'auto');
    head.appendChild(link);
}

ensureStylesheet();

interface ObserverEntry {
    observer: ResizeObserver;
    dotnetRef: DotNetObjectRef;
}

const _observers: Map<string, ObserverEntry> = new Map();

/**
 * Install a `ResizeObserver` on `container` that pings
 * `dotnetRef.OnResize(width, height)` on every dimension change. Returns an
 * opaque handle for `unobserveResize`. Returns `""` when the browser does
 * not support `ResizeObserver` (legacy hosts); the C# side treats that as
 * "no responsive resize, use static dims".
 */
export function observeResize(container: Element | null, dotnetRef: DotNetObjectRef): string {
    if (!container || typeof ResizeObserver === 'undefined') return '';

    const handle: string = ((Math.random() * 1e9) | 0).toString(36) + Date.now().toString(36);

    const observer: ResizeObserver = new ResizeObserver((entries: ResizeObserverEntry[]) => {
        for (const entry of entries) {
            const { width, height } = entry.contentRect;
            // Fire-and-forget: a failed invoke means the circuit / runtime is
            // gone, which the C# 4-tuple catch swallows on its end.
            dotnetRef.invokeMethodAsync('OnResize', width, height).catch(() => { /* ignore */ });
        }
    });

    observer.observe(container);
    _observers.set(handle, { observer, dotnetRef });
    return handle;
}

/**
 * Detach a previously installed observer. Idempotent — a missing handle is
 * a no-op.
 */
export function unobserveResize(handle: string): void {
    const entry = _observers.get(handle);
    if (!entry) return;

    try { entry.observer.disconnect(); } catch { /* already gone */ }
    try { entry.dotnetRef.dispose(); } catch { /* already disposed */ }
    _observers.delete(handle);
}

/**
 * Serialize the supplied `<svg>` element to a PNG blob and trigger a browser
 * download. Runs entirely client-side (no server round-trip). Background is
 * white so dark-theme charts read correctly when pasted into reports / slides.
 */
export async function exportSvgAsPng(
    svg: SVGSVGElement | null,
    fileName: string,
    width?: number | null,
    height?: number | null,
): Promise<void> {
    if (!svg) return;

    // Serialize SVG → string. Inject xmlns if missing (some renderers omit it).
    const serializer = new XMLSerializer();
    let svgString = serializer.serializeToString(svg);
    if (!/\sxmlns=/.test(svgString)) {
        svgString = svgString.replace(/^<svg/, '<svg xmlns="http://www.w3.org/2000/svg"');
    }

    // Resolve raster dimensions: explicit args win, then SVG viewBox, then
    // bounding-rect fallback, then a 600 × 400 default.
    const bbox = svg.getBoundingClientRect();
    const vb = svg.viewBox && svg.viewBox.baseVal;
    const w: number = width || (vb && vb.width) || bbox.width || 600;
    const h: number = height || (vb && vb.height) || bbox.height || 400;

    const svgBlob = new Blob([svgString], { type: 'image/svg+xml;charset=utf-8' });
    const svgUrl = URL.createObjectURL(svgBlob);

    try {
        const img = new Image();
        await new Promise<void>((resolve, reject) => {
            img.onload = () => resolve();
            img.onerror = () => reject(new Error('SVG image load failed'));
            img.src = svgUrl;
        });

        const canvas = document.createElement('canvas');
        canvas.width = w;
        canvas.height = h;
        const ctx = canvas.getContext('2d');
        if (!ctx) return;
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, w, h);
        ctx.drawImage(img, 0, 0, w, h);

        const pngBlob: Blob | null = await new Promise(resolve =>
            canvas.toBlob(b => resolve(b), 'image/png'));
        if (!pngBlob) return;

        const pngUrl = URL.createObjectURL(pngBlob);
        try {
            const a = document.createElement('a');
            a.href = pngUrl;
            a.download = (fileName || 'chart') + '.png';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        } finally {
            URL.revokeObjectURL(pngUrl);
        }
    } finally {
        URL.revokeObjectURL(svgUrl);
    }
}
