// BOBTextArea TypeScript Module
// Handles auto-resize functionality

interface TextAreaInstance {
    textarea: HTMLTextAreaElement;
    inputHandler: () => void;
}

const instances = new Map<string, TextAreaInstance>();

/**
 * Initialize auto-resize behavior for a textarea.
 */
export function initialize(
    textarea: HTMLTextAreaElement,
    textareaId: string
): void {
    if (!textarea || instances.has(textareaId)) {
        return;
    }

    // Initial resize
    adjustHeight(textarea);

    // Create event handler
    const inputHandler = (): void => adjustHeight(textarea);

    // Store reference for cleanup
    instances.set(textareaId, {
        textarea,
        inputHandler
    });

    // Attach event listener
    textarea.addEventListener('input', inputHandler);
}

/**
 * Adjust textarea height based on content
 */
function adjustHeight(textarea: HTMLTextAreaElement): void {
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
 */
export function dispose(textareaId: string): void {
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