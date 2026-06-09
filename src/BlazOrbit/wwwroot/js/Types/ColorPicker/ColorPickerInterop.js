// BlazOrbit - ColorPicker interop (hand-written JSDoc-typed ESM).

/**
 * @param {HTMLElement} element
 * @param {number} clientX
 * @param {number} clientY
 * @returns {number[]}
 */
export function getRelativePosition(element, clientX, clientY) {
    const rect = element.getBoundingClientRect();
    const w = rect.width || 1;
    const h = rect.height || 1;
    const x = Math.max(0, Math.min(rect.width, clientX - rect.left)) / w;
    const y = Math.max(0, Math.min(rect.height, clientY - rect.top)) / h;
    return [x, y];
}
