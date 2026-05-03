interface SliderCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

type Orientation = 'horizontal' | 'vertical';

interface SliderInstance {
    track: HTMLElement;
    orientation: Orientation;
    relay: SliderCallbacksRelay;
    pointerMove: (e: PointerEvent) => void;
    pointerUp: (e: PointerEvent) => void;
}

const instances = new Map<string, SliderInstance>();

function computePercentInternal(track: HTMLElement, orientation: Orientation, clientX: number, clientY: number): number {
    const rect = track.getBoundingClientRect();
    let raw: number;
    if (orientation === 'vertical') {
        raw = rect.height === 0 ? 0 : ((rect.bottom - clientY) / rect.height) * 100;
    } else {
        raw = rect.width === 0 ? 0 : ((clientX - rect.left) / rect.width) * 100;
    }
    if (raw < 0) return 0;
    if (raw > 100) return 100;
    return raw;
}

export function computePercent(track: HTMLElement, orientation: Orientation, clientX: number, clientY: number): number {
    return computePercentInternal(track, orientation, clientX, clientY);
}

export function startDrag(
    track: HTMLElement,
    dotNetRef: SliderCallbacksRelay,
    componentId: string,
    orientation: Orientation,
    initialClientX: number,
    initialClientY: number,
): void {
    if (instances.has(componentId)) {
        stopDrag(componentId);
    }

    const handlers: SliderInstance = {
        track,
        orientation,
        relay: dotNetRef,
        pointerMove: (e: PointerEvent) => {
            const percent = computePercentInternal(track, orientation, e.clientX, e.clientY);
            dotNetRef.invokeMethodAsync('OnPointerMove', percent);
        },
        pointerUp: (e: PointerEvent) => {
            const percent = computePercentInternal(track, orientation, e.clientX, e.clientY);
            dotNetRef.invokeMethodAsync('OnPointerUp', percent);
            stopDrag(componentId);
        },
    };

    document.addEventListener('pointermove', handlers.pointerMove);
    document.addEventListener('pointerup', handlers.pointerUp);
    document.addEventListener('pointercancel', handlers.pointerUp);

    instances.set(componentId, handlers);

    // Emit initial percent so click-to-seek and direct-thumb-grab both land at the
    // pointer position before any pointermove fires.
    const initialPercent = computePercentInternal(track, orientation, initialClientX, initialClientY);
    dotNetRef.invokeMethodAsync('OnPointerMove', initialPercent);
}

export function stopDrag(componentId: string): void {
    const handlers = instances.get(componentId);
    if (!handlers) return;

    document.removeEventListener('pointermove', handlers.pointerMove);
    document.removeEventListener('pointerup', handlers.pointerUp);
    document.removeEventListener('pointercancel', handlers.pointerUp);

    instances.delete(componentId);
}
