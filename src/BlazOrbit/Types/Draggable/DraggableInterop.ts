interface DragCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface DragInstance {
    mouseMove: (e: MouseEvent) => void;
    mouseUp: (e: MouseEvent) => void;
}

const instances = new Map<string, DragInstance>();

// JS-08: canonical `initialize` / `dispose` shape shared with the other
// interop modules.
// JS-10: drag is single-instance by design. Starting a new drag while another
// is active replaces the previous one (the previous instance's mousemove/mouseup
// handlers are removed first) so we never accumulate document-level listeners.
export function initialize(
    element: HTMLElement,
    dotNetRef: DragCallbacksRelay,
    componentId: string
): void {
    if (instances.has(componentId)) return;

    // Replace any other concurrent drag to keep the listener count at zero or one.
    if (instances.size > 0) {
        for (const id of [...instances.keys()]) dispose(id);
    }

    const handlers: DragInstance = {
        mouseMove: (e: MouseEvent) => {
            dotNetRef.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY);
        },
        mouseUp: (e: MouseEvent) => {
            dotNetRef.invokeMethodAsync('OnMouseUp', e.clientX, e.clientY);
            dispose(componentId);
        }
    };

    document.addEventListener('mousemove', handlers.mouseMove);
    document.addEventListener('mouseup', handlers.mouseUp);

    instances.set(componentId, handlers);
}

export function dispose(componentId: string): void {
    const handlers = instances.get(componentId);
    if (!handlers) return;

    document.removeEventListener('mousemove', handlers.mouseMove);
    document.removeEventListener('mouseup', handlers.mouseUp);

    instances.delete(componentId);
}