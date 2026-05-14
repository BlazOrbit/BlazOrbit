// BlazOrbit — Carousel interop (hand-written JSDoc-typed ESM).

/**
 * @typedef {Object} CarouselCallbacksRelay
 * @property {(methodName: string, ...args: unknown[]) => Promise<unknown>} invokeMethodAsync
 */

/**
 * @typedef {Object} CarouselInstance
 * @property {(e: TouchEvent) => void} touchStart
 * @property {(e: TouchEvent) => void} touchEnd
 * @property {HTMLElement} element
 */

/** @type {Map<string, CarouselInstance>} */
const instances = new Map();

/**
 * @param {HTMLElement} element
 * @param {CarouselCallbacksRelay} dotNetRef
 * @param {string} componentId
 * @param {number} swipeThresholdPx
 */
export function initialize(element, dotNetRef, componentId, swipeThresholdPx) {
    if (instances.has(componentId)) {
        dispose(componentId);
    }

    let startX = 0;
    let startY = 0;

    /** @type {CarouselInstance} */
    const handlers = {
        element,
        touchStart: (e) => {
            if (e.touches.length !== 1) return;
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        },
        touchEnd: (e) => {
            if (e.changedTouches.length !== 1) return;
            const dx = e.changedTouches[0].clientX - startX;
            const dy = e.changedTouches[0].clientY - startY;
            // Ignore vertical-dominant gestures so the user can scroll the page through the carousel.
            if (Math.abs(dx) < swipeThresholdPx || Math.abs(dx) <= Math.abs(dy)) return;
            if (dx < 0) {
                dotNetRef.invokeMethodAsync('OnSwipeLeft');
            } else {
                dotNetRef.invokeMethodAsync('OnSwipeRight');
            }
        }
    };

    element.addEventListener('touchstart', handlers.touchStart, { passive: true });
    element.addEventListener('touchend', handlers.touchEnd, { passive: true });

    instances.set(componentId, handlers);
}

/** @param {string} componentId */
export function dispose(componentId) {
    const handlers = instances.get(componentId);
    if (!handlers) return;

    handlers.element.removeEventListener('touchstart', handlers.touchStart);
    handlers.element.removeEventListener('touchend', handlers.touchEnd);

    instances.delete(componentId);
}
