// BlazOrbit - TextArea interop (hand-written JSDoc-typed ESM).
// Handles auto-resize functionality.

/**
 * @typedef {Object} TextAreaInstance
 * @property {HTMLTextAreaElement} textarea
 * @property {() => void} inputHandler
 */

/** @type {Map<string, TextAreaInstance>} */
const instances = new Map();

/**
 * Initialize auto-resize behavior for a textarea.
 * @param {HTMLTextAreaElement} textarea
 * @param {string} textareaId
 */
export function initialize(textarea, textareaId) {
    if (!textarea || instances.has(textareaId)) {
        return;
    }

    // Initial resize
    adjustHeight(textarea);

    // Create event handler
    const inputHandler = () => adjustHeight(textarea);

    // Store reference for cleanup
    instances.set(textareaId, {
        textarea,
        inputHandler
    });

    // Attach event listener
    textarea.addEventListener('input', inputHandler);
}

/**
 * Adjust textarea height based on content.
 * @param {HTMLTextAreaElement} textarea
 */
function adjustHeight(textarea) {
    // Reset height to auto to get the correct scrollHeight
    textarea.style.height = 'auto';

    // Get minimum height from CSS variable
    const container = textarea.closest('bob-component');
    const minHeightStr = container
        ? getComputedStyle(container).getPropertyValue('--bob-textarea-min-height')
        : '80px';

    const minHeight = parseFloat(minHeightStr) || 80;

    // Set new height based on content
    const newHeight = Math.max(textarea.scrollHeight, minHeight);
    textarea.style.height = `${newHeight}px`;
}

/**
 * Clean up auto-resize for a textarea.
 * @param {string} textareaId
 */
export function dispose(textareaId) {
    const instance = instances.get(textareaId);

    if (!instance) {
        return;
    }

    const { textarea, inputHandler } = instance;

    // Remove event listener
    if (textarea && inputHandler) {
        textarea.removeEventListener('input', inputHandler);
    }

    // Remove from map
    instances.delete(textareaId);
}
