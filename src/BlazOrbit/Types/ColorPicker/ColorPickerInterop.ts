export function getRelativePosition(
    element: HTMLElement,
    clientX: number,
    clientY: number
): number[] {
    const rect = element.getBoundingClientRect();
    const w = rect.width || 1;
    const h = rect.height || 1;
    const x = Math.max(0, Math.min(rect.width, clientX - rect.left)) / w;
    const y = Math.max(0, Math.min(rect.height, clientY - rect.top)) / h;
    return [x, y];
}