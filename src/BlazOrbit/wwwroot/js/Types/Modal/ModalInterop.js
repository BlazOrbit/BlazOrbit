// BlazOrbit - Modal interop (hand-written JSDoc-typed ESM).

/**
 * @typedef {Object} FocusTrapState
 * @property {string} id
 * @property {HTMLElement | null} previousActiveElement
 * @property {HTMLElement} container
 * @property {HTMLElement | null} firstFocusable
 * @property {HTMLElement | null} lastFocusable
 */

let scrollLockCount = 0;

// Nested modals (e.g. dialog → confirmation modal, or modal → picker) need
// independent focus traps. The stack stores one entry per active trap,
// keyed by an `id` that the C# side owns (per-component Guid). The top
// entry is the innermost trap and receives all Tab handling. Releasing
// looks up by id and removes that specific entry - only restoring focus
// to its previousActiveElement when it was the top, so an outer trap
// being torn down out-of-order (e.g. parent component disposes while an
// inner modal is still open) doesn't steal focus from the inner one.
//
// A single document-level keydown listener reads `focusTrapStack` top
// instead of registering one listener per trap. That avoids the historical
// bug where opening a nested modal would `releaseFocus()` the parent and
// orphan its listener.
/** @type {FocusTrapState[]} */
const focusTrapStack = [];
/** @type {((e: KeyboardEvent) => void) | null} */
let activeTabHandler = null;

// Aligned with focus-trap / ariakit: include rich-editor targets and media elements
// so a modal containing a <div contenteditable> or <video controls> traps Tab correctly.
const FOCUSABLE_SELECTORS = [
    'button:not([disabled])',
    '[href]',
    'input:not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])',
    '[contenteditable=""]',
    '[contenteditable="true"]',
    'audio[controls]',
    'video[controls]',
    'iframe',
    'embed',
    'object'
].join(', ');

export function lockScroll() {
    scrollLockCount++;
    if (scrollLockCount === 1) {
        document.body.style.overflow = 'hidden';
    }
}

export function unlockScroll() {
    scrollLockCount--;
    if (scrollLockCount <= 0) {
        scrollLockCount = 0;
        document.body.style.overflow = '';
    }
}

/**
 * @param {HTMLElement} element
 * @param {string} id
 */
export function trapFocus(element, id) {
    const previousActiveElement = /** @type {HTMLElement} */ (document.activeElement);
    const focusables = element.querySelectorAll(FOCUSABLE_SELECTORS);

    /** @type {HTMLElement | null} */
    let firstFocusable = null;
    /** @type {HTMLElement | null} */
    let lastFocusable = null;

    if (focusables.length === 0) {
        element.setAttribute('tabindex', '-1');
        element.focus();
    } else {
        firstFocusable = /** @type {HTMLElement} */ (focusables[0]);
        lastFocusable = /** @type {HTMLElement} */ (focusables[focusables.length - 1]);
        firstFocusable.focus();
    }

    focusTrapStack.push({
        id,
        previousActiveElement,
        container: element,
        firstFocusable,
        lastFocusable
    });

    if (!activeTabHandler) {
        activeTabHandler = (e) => {
            if (e.key !== 'Tab') return;

            const top = focusTrapStack[focusTrapStack.length - 1];
            if (!top) return;

            const active = /** @type {HTMLElement | null} */ (document.activeElement);
            const { container, firstFocusable: first, lastFocusable: last } = top;

            // If focus escaped the dialog (e.g. host wrapper has tabindex=-1),
            // pull it back inside.
            if (!active || !container.contains(active)) {
                if (first) {
                    e.preventDefault();
                    first.focus();
                }
                return;
            }

            if (!first || !last) return;

            if (e.shiftKey) {
                if (active === first) {
                    e.preventDefault();
                    last.focus();
                }
            } else {
                if (active === last) {
                    e.preventDefault();
                    first.focus();
                }
            }
        };
        document.addEventListener('keydown', activeTabHandler);
    }
}

/**
 * @param {HTMLElement} element
 * @param {number} fallbackMs
 * @returns {Promise<void>}
 */
export function waitForAnimationEnd(element, fallbackMs) {
    if (!element) return Promise.resolve();

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) return Promise.resolve();

    return new Promise((resolve) => {
        let done = false;
        const finish = () => {
            if (done) return;
            done = true;
            element.removeEventListener('animationend', finish);
            element.removeEventListener('transitionend', finish);
            clearTimeout(timeoutId);
            resolve();
        };
        element.addEventListener('animationend', finish, { once: true });
        element.addEventListener('transitionend', finish, { once: true });
        const timeoutId = window.setTimeout(finish, fallbackMs);
    });
}

/** @param {string} id */
export function releaseFocus(id) {
    const idx = focusTrapStack.findIndex(s => s.id === id);
    if (idx === -1) return;

    const wasTop = idx === focusTrapStack.length - 1;
    const [released] = focusTrapStack.splice(idx, 1);

    if (focusTrapStack.length === 0 && activeTabHandler) {
        document.removeEventListener('keydown', activeTabHandler);
        activeTabHandler = null;
    }

    // Only restore focus when releasing the topmost trap. If an outer trap
    // is being torn down while an inner one is still active (e.g. parent
    // component unmounted), the inner trap must keep focus - restoring an
    // older `previousActiveElement` here would steal it.
    if (wasTop && released.previousActiveElement) {
        released.previousActiveElement.focus();
    }
}
