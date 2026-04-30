export function getRelativePosition(
    element: HTMLElement,
    clientX: number,
    clientY: number
): number[] {
    const rect = element.getBoundingClientRect();
    const x = Math.max(0, Math.min(rect.width, clientX - rect.left));
    const y = Math.max(0, Math.min(rect.height, clientY - rect.top));
    return [x, y];
}

export function setHandlerPosition(
    handler: HTMLElement,
    x: number,
    y: number
): void {
    if (!handler) return;
    handler.style.left = `${x}px`;
    handler.style.top = `${y}px`;
}