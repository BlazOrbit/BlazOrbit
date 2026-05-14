// BlazOrbit — Slider interop (hand-written JSDoc-typed ESM).

/**
 * @typedef {Object} SliderCallbacksRelay
 * @property {(methodName: string, ...args: unknown[]) => Promise<unknown>} invokeMethodAsync
 */

/** @typedef {'horizontal' | 'vertical'} Orientation */

/**
 * @typedef {Object} SliderInstance
 * @property {HTMLElement} track
 * @property {Orientation} orientation
 * @property {SliderCallbacksRelay} relay
 * @property {(e: PointerEvent) => void} pointerMove
 * @property {(e: PointerEvent) => void} pointerUp
 */

/** @type {Map<string, SliderInstance>} */
const instances = new Map();

/**
 * @param {HTMLElement} track
 * @param {Orientation} orientation
 * @param {number} clientX
 * @param {number} clientY
 * @returns {number}
 */
function computePercentInternal(track, orientation, clientX, clientY) {
    const rect = track.getBoundingClientRect();
    let raw;
    if (orientation === 'vertical') {
        raw = rect.height === 0 ? 0 : ((rect.bottom - clientY) / rect.height) * 100;
    } else {
        raw = rect.width === 0 ? 0 : ((clientX - rect.left) / rect.width) * 100;
    }
    if (raw < 0) return 0;
    if (raw > 100) return 100;
    return raw;
}

/**
 * @param {HTMLElement} track
 * @param {Orientation} orientation
 * @param {number} clientX
 * @param {number} clientY
 * @returns {number}
 */
export function computePercent(track, orientation, clientX, clientY) {
    return computePercentInternal(track, orientation, clientX, clientY);
}

/**
 * @param {HTMLElement} track
 * @param {SliderCallbacksRelay} dotNetRef
 * @param {string} componentId
 * @param {Orientation} orientation
 * @param {number} initialClientX
 * @param {number} initialClientY
 */
export function startDrag(track, dotNetRef, componentId, orientation, initialClientX, initialClientY) {
    if (instances.has(componentId)) {
        stopDrag(componentId);
    }

    /** @type {SliderInstance} */
    const handlers = {
        track,
        orientation,
        relay: dotNetRef,
        pointerMove: (e) => {
            const percent = computePercentInternal(track, orientation, e.clientX, e.clientY);
            dotNetRef.invokeMethodAsync('OnPointerMove', percent);
        },
        pointerUp: (e) => {
            const percent = computePercentInternal(track, orientation, e.clientX, e.clientY);
            dotNetRef.invokeMethodAsync('OnPointerUp', percent);
            stopDrag(componentId);
        }
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

/** @param {string} componentId */
export function stopDrag(componentId) {
    const handlers = instances.get(componentId);
    if (!handlers) return;

    document.removeEventListener('pointermove', handlers.pointerMove);
    document.removeEventListener('pointerup', handlers.pointerUp);
    document.removeEventListener('pointercancel', handlers.pointerUp);

    instances.delete(componentId);
}
