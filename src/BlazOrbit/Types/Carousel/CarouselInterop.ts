interface CarouselCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface CarouselInstance {
    touchStart: (e: TouchEvent) => void;
    touchEnd: (e: TouchEvent) => void;
    element: HTMLElement;
}

const instances = new Map<string, CarouselInstance>();

export function initialize(
    element: HTMLElement,
    dotNetRef: CarouselCallbacksRelay,
    componentId: string,
    swipeThresholdPx: number
): void {
    if (instances.has(componentId)) {
        dispose(componentId);
    }

    let startX = 0;
    let startY = 0;

    const handlers: CarouselInstance = {
        element,
        touchStart: (e: TouchEvent) => {
            if (e.touches.length !== 1) return;
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        },
        touchEnd: (e: TouchEvent) => {
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

export function dispose(componentId: string): void {
    const handlers = instances.get(componentId);
    if (!handlers) return;

    handlers.element.removeEventListener('touchstart', handlers.touchStart);
    handlers.element.removeEventListener('touchend', handlers.touchEnd);

    instances.delete(componentId);
}
