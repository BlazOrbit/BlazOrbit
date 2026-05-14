// BlazOrbit — Pattern interop (hand-written JSDoc-typed ESM).

/**
 * @typedef {Object} PatternCallbacksRelay
 * @property {(methodName: string, ...args: unknown[]) => Promise<unknown>} invokeMethodAsync
 */

/**
 * @typedef {Object} PatternInstance
 * @property {HTMLElement} container
 * @property {PatternCallbacksRelay} dotnetRef
 * @property {string} componentId
 */

/** @type {Map<string, PatternInstance>} */
const patternInstances = new Map();

/**
 * @param {HTMLElement} container
 * @param {PatternCallbacksRelay} dotnetRef
 * @param {string} componentId
 */
export function initialize(container, dotnetRef, componentId) {
    if (!container || !componentId) return;

    dispose(componentId);

    patternInstances.set(componentId, {
        container,
        dotnetRef,
        componentId
    });

    container.addEventListener('input', handleInput);
    container.addEventListener('focus', handleFocus, true);
    container.addEventListener('blur', handleBlur, true);
    container.addEventListener('click', handleClick, true);
    container.addEventListener('paste', handlePaste);
    container.addEventListener('keydown', handleKeyDown);
}

/** @param {Event} e */
async function handleInput(e) {
    const target = /** @type {HTMLElement} */ (e.target);
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);
    const value = target.textContent || '';
    const maxLength = parseInt(target.dataset.bobMaxlength || '0');

    try {
        await instance.dotnetRef.invokeMethodAsync('OnSpanInput', index, value);

        if (value.length === maxLength) {
            const isValid = await instance.dotnetRef.invokeMethodAsync('OnSpanComplete', index, value);

            if (isValid) {
                moveToNextSpan(instance, index);
            }
        }
    } catch (error) {
        console.error('Input handling error:', error);
    }
}

/** @param {Event} e */
async function handleClick(e) {
    const target = /** @type {HTMLElement} */ (e.target);

    if (isToggleSpan(target)) {
        const instance = getInstanceFromElement(target);
        if (!instance) return;

        const index = getSpanIndex(target);
        await instance.dotnetRef.invokeMethodAsync('OnToggleClick', index);
        return;
    }

    if (!isEditableSpan(target)) return;
    selectSpanContentInternal(target);
}

/** @param {FocusEvent} e */
async function handleFocus(e) {
    const target = /** @type {HTMLElement} */ (e.target);
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);

    try {
        await instance.dotnetRef.invokeMethodAsync('OnSpanFocus', index);
    } catch (error) {
        console.error('Focus handling error:', error);
    }
}

/** @param {FocusEvent} e */
async function handleBlur(e) {
    const target = /** @type {HTMLElement} */ (e.target);
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);

    try {
        await instance.dotnetRef.invokeMethodAsync('OnSpanBlur', index);
    } catch (error) {
        console.error('Blur handling error:', error);
    }
}

/** @param {ClipboardEvent} e */
async function handlePaste(e) {
    e.preventDefault();

    const target = /** @type {HTMLElement} */ (e.target);
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance || !e.clipboardData) return;

    const text = e.clipboardData.getData('text');

    try {
        await instance.dotnetRef.invokeMethodAsync('OnPaste', text);
    } catch (error) {
        console.error('Paste handling error:', error);
    }
}

/** @param {KeyboardEvent} e */
async function handleKeyDown(e) {
    const target = /** @type {HTMLElement} */ (e.target);
    if (!isEditableSpan(target) && !isToggleSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);

    if (e.key === 'Tab') {
        const spans = /** @type {HTMLElement[]} */ (Array.from(
            instance.container.querySelectorAll('[contenteditable="true"], [data-bob-toggle="true"]')
        ));

        const currentIdx = spans.findIndex(s => s === target);
        const isFirst = currentIdx === 0;
        const isLast = currentIdx === spans.length - 1;

        if ((isLast && !e.shiftKey) || (isFirst && e.shiftKey)) {
            return;
        }

        e.preventDefault();

        /** @type {HTMLElement | null} */
        let nextSpan = null;
        if (e.shiftKey && currentIdx > 0) {
            nextSpan = spans[currentIdx - 1];
        } else if (!e.shiftKey && currentIdx < spans.length - 1) {
            nextSpan = spans[currentIdx + 1];
        }

        if (nextSpan) {
            nextSpan.focus();
            if (isEditableSpan(nextSpan)) {
                selectSpanContentInternal(nextSpan);
            }
        }
    } else if (e.key === 'Backspace' && target.textContent === '' && isEditableSpan(target)) {
        e.preventDefault();
        moveToPrevEditableSpan(instance, index);
    } else if ((e.key === ' ' || e.key === 'Enter') && isToggleSpan(target)) {
        e.preventDefault();
        await instance.dotnetRef.invokeMethodAsync('OnToggleClick', index);
    }
}

/**
 * @param {PatternInstance} instance
 * @param {number} currentIndex
 */
function moveToNextSpan(instance, currentIndex) {
    const allEditableSpans = /** @type {HTMLElement[]} */ (Array.from(
        instance.container.querySelectorAll('[contenteditable="true"]')
    ));

    const nextSpan = allEditableSpans.find(span => {
        const spanIndex = getSpanIndex(span);
        return spanIndex > currentIndex;
    });

    if (nextSpan) {
        setTimeout(() => {
            nextSpan.focus();
            selectSpanContentInternal(nextSpan);
        }, 0);
    }
}

/**
 * @param {PatternInstance} instance
 * @param {number} currentIndex
 */
function moveToPrevEditableSpan(instance, currentIndex) {
    const allEditableSpans = /** @type {HTMLElement[]} */ (Array.from(
        instance.container.querySelectorAll('[contenteditable="true"]')
    ));

    const prevSpan = [...allEditableSpans].reverse().find(span => {
        const spanIndex = getSpanIndex(span);
        return spanIndex < currentIndex;
    });

    if (prevSpan) {
        prevSpan.focus();
        const range = document.createRange();
        const selection = window.getSelection();
        range.selectNodeContents(prevSpan);
        range.collapse(false);
        selection?.removeAllRanges();
        selection?.addRange(range);
    }
}

/**
 * @param {string} componentId
 * @param {number} index
 * @param {string} value
 */
export function updateSpanValue(componentId, index, value) {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = /** @type {HTMLElement} */ (instance.container.querySelector(
        `[data-bob-index="${index}"]`
    ));

    if (span && span.textContent !== value) {
        const isFocused = document.activeElement === span;

        span.textContent = value;

        if (isFocused && isEditableSpan(span)) {
            selectSpanContentInternal(span);
        }
    }
}

/**
 * @param {string} componentId
 * @param {number} index
 */
export function selectSpanContent(componentId, index) {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = /** @type {HTMLElement} */ (instance.container.querySelector(
        `[data-bob-index="${index}"]`
    ));

    if (span) {
        selectSpanContentInternal(span);
    }
}

/**
 * @param {string} componentId
 * @param {number} index
 */
export function setCaretToEnd(componentId, index) {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = /** @type {HTMLElement} */ (instance.container.querySelector(
        `[data-bob-index="${index}"]`
    ));

    if (span && document.activeElement === span) {
        const range = document.createRange();
        const selection = window.getSelection();

        if (span.firstChild) {
            range.setStart(span.firstChild, span.textContent?.length || 0);
            range.collapse(true);
        } else {
            range.selectNodeContents(span);
            range.collapse(false);
        }

        selection?.removeAllRanges();
        selection?.addRange(range);
    }
}

/**
 * @param {string} componentId
 * @param {number} index
 */
export function focusSpan(componentId, index) {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = /** @type {HTMLElement} */ (instance.container.querySelector(
        `[data-bob-index="${index}"][contenteditable="true"]`
    ));

    if (span) {
        span.focus();
        selectSpanContentInternal(span);
    }
}

/** @param {string} componentId */
export function focusFirstEditable(componentId) {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const firstSpan = /** @type {HTMLElement} */ (instance.container.querySelector(
        '[contenteditable="true"]'
    ));

    if (firstSpan) {
        firstSpan.focus();
        selectSpanContentInternal(firstSpan);
    }
}

/** @param {string} componentId */
export function dispose(componentId) {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    instance.container.removeEventListener('input', handleInput);
    instance.container.removeEventListener('focus', handleFocus, true);
    instance.container.removeEventListener('blur', handleBlur, true);
    instance.container.removeEventListener('click', handleClick, true);
    instance.container.removeEventListener('paste', handlePaste);
    instance.container.removeEventListener('keydown', handleKeyDown);

    patternInstances.delete(componentId);
}

/**
 * @param {HTMLElement} element
 * @returns {boolean}
 */
function isEditableSpan(element) {
    return element.tagName === 'SPAN' && element.contentEditable === 'true';
}

/**
 * @param {HTMLElement} element
 * @returns {boolean}
 */
function isToggleSpan(element) {
    return element.tagName === 'SPAN' && element.dataset.bobToggle === 'true';
}

/**
 * @param {HTMLElement} span
 * @returns {number}
 */
function getSpanIndex(span) {
    return parseInt(span.dataset.bobIndex || '-1');
}

/**
 * @param {HTMLElement} element
 * @returns {PatternInstance | null}
 */
function getInstanceFromElement(element) {
    const container = /** @type {HTMLElement} */ (element.closest('[data-bob-pattern-id]'));
    if (!container) return null;

    const componentId = container.dataset.bobPatternId;
    if (!componentId) return null;

    return patternInstances.get(componentId) || null;
}

/** @param {HTMLElement} span */
function selectSpanContentInternal(span) {
    const range = document.createRange();
    range.selectNodeContents(span);
    const selection = window.getSelection();
    selection?.removeAllRanges();
    selection?.addRange(range);
}
