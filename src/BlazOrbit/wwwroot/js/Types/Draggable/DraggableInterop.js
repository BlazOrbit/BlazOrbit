// BlazOrbit — Draggable interop (hand-written JSDoc-typed ESM).

/**
 * @typedef {Object} DragCallbacksRelay
 * @property {(methodName: string, ...args: unknown[]) => Promise<unknown>} invokeMethodAsync
 */

/**
 * @typedef {Object} DragInstance
 * @property {(e: MouseEvent) => void} mouseMove
 * @property {(e: MouseEvent) => void} mouseUp
 */

/** @type {Map<string, DragInstance>} */
const instances = new Map();

// Canonical `initialize` / `dispose` shape shared with the other interop
// modules. Drag is single-instance by design. Starting a new drag while
// another is active replaces the previous one (the previous instance's
// mousemove/mouseup handlers are removed first) so we never accumulate
// document-level listeners.
/**
 * @param {HTMLElement} element
 * @param {DragCallbacksRelay} dotNetRef
 * @param {string} componentId
 */
export function initialize(element, dotNetRef, componentId) {
    if (instances.has(componentId)) return;

    // Replace any other concurrent drag to keep the listener count at zero or one.
    if (instances.size > 0) {
        for (const id of [...instances.keys()]) dispose(id);
    }

    /** @type {DragInstance} */
    const handlers = {
        mouseMove: (e) => {
            dotNetRef.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY);
        },
        mouseUp: (e) => {
            dotNetRef.invokeMethodAsync('OnMouseUp', e.clientX, e.clientY);
            dispose(componentId);
        }
    };

    document.addEventListener('mousemove', handlers.mouseMove);
    document.addEventListener('mouseup', handlers.mouseUp);

    instances.set(componentId, handlers);
}

/** @param {string} componentId */
export function dispose(componentId) {
    const handlers = instances.get(componentId);
    if (!handlers) return;

    document.removeEventListener('mousemove', handlers.mouseMove);
    document.removeEventListener('mouseup', handlers.mouseUp);

    instances.delete(componentId);
}
