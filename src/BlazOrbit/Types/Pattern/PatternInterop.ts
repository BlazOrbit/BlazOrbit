interface PatternCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface PatternInstance {
    container: HTMLElement;
    dotnetRef: PatternCallbacksRelay;
    componentId: string;
}

const patternInstances = new Map<string, PatternInstance>();

export function initialize(
    container: HTMLElement,
    dotnetRef: PatternCallbacksRelay,
    componentId: string
): void {
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

async function handleInput(e: Event): Promise<void> {
    const target = e.target as HTMLElement;
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

async function handleClick(e: Event): Promise<void> {
    const target = e.target as HTMLElement;

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

async function handleFocus(e: FocusEvent): Promise<void> {
    const target = e.target as HTMLElement;
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

async function handleBlur(e: FocusEvent): Promise<void> {
    const target = e.target as HTMLElement;
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

async function handlePaste(e: ClipboardEvent): Promise<void> {
    e.preventDefault();

    const target = e.target as HTMLElement;
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

async function handleKeyDown(e: KeyboardEvent): Promise<void> {
    const target = e.target as HTMLElement;
    if (!isEditableSpan(target) && !isToggleSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);

    if (e.key === 'Tab') {
        const spans = Array.from(
            instance.container.querySelectorAll('[contenteditable="true"], [data-bob-toggle="true"]')
        ) as HTMLElement[];

        const currentIdx = spans.findIndex(s => s === target);
        const isFirst = currentIdx === 0;
        const isLast = currentIdx === spans.length - 1;

        if ((isLast && !e.shiftKey) || (isFirst && e.shiftKey)) {
            return;
        }

        e.preventDefault();

        let nextSpan: HTMLElement | null = null;
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

function moveToNextSpan(instance: PatternInstance, currentIndex: number): void {
    const allEditableSpans = Array.from(
        instance.container.querySelectorAll('[contenteditable="true"]')
    ) as HTMLElement[];

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

function moveToPrevEditableSpan(instance: PatternInstance, currentIndex: number): void {
    const allEditableSpans = Array.from(
        instance.container.querySelectorAll('[contenteditable="true"]')
    ) as HTMLElement[];

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

export function updateSpanValue(componentId: string, index: number, value: string): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = instance.container.querySelector(
        `[data-bob-index="${index}"]`
    ) as HTMLElement;

    if (span && span.textContent !== value) {
        const isFocused = document.activeElement === span;

        span.textContent = value;

        if (isFocused && isEditableSpan(span)) {
            selectSpanContentInternal(span);
        }
    }
}

export function selectSpanContent(componentId: string, index: number): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = instance.container.querySelector(
        `[data-bob-index="${index}"]`
    ) as HTMLElement;

    if (span) {
        selectSpanContentInternal(span);
    }
}

export function setCaretToEnd(componentId: string, index: number): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = instance.container.querySelector(
        `[data-bob-index="${index}"]`
    ) as HTMLElement;

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

export function focusSpan(componentId: string, index: number): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = instance.container.querySelector(
        `[data-bob-index="${index}"][contenteditable="true"]`
    ) as HTMLElement;

    if (span) {
        span.focus();
        selectSpanContentInternal(span);
    }
}

export function focusFirstEditable(componentId: string): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const firstSpan = instance.container.querySelector(
        '[contenteditable="true"]'
    ) as HTMLElement;

    if (firstSpan) {
        firstSpan.focus();
        selectSpanContentInternal(firstSpan);
    }
}

export function dispose(componentId: string): void {
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

function isEditableSpan(element: HTMLElement): boolean {
    return element.tagName === 'SPAN' && element.contentEditable === 'true';
}

function isToggleSpan(element: HTMLElement): boolean {
    return element.tagName === 'SPAN' && element.dataset.bobToggle === 'true';
}

function getSpanIndex(span: HTMLElement): number {
    return parseInt(span.dataset.bobIndex || '-1');
}

function getInstanceFromElement(element: HTMLElement): PatternInstance | null {
    const container = element.closest('[data-bob-pattern-id]') as HTMLElement;
    if (!container) return null;

    const componentId = container.dataset.bobPatternId;
    if (!componentId) return null;

    return patternInstances.get(componentId) || null;
}

function selectSpanContentInternal(span: HTMLElement): void {
    const range = document.createRange();
    range.selectNodeContents(span);
    const selection = window.getSelection();
    selection?.removeAllRanges();
    selection?.addRange(range);
}