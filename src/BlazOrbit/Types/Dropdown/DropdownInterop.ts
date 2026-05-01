//  ARIA APG Combobox Pattern
//  Tecla Comportamiento
//  Space(en trigger) Abre el menú
//  Enter(en trigger) Abre el menú
//  Arrow Down Abre el menú(si cerrado) o navega a siguiente opción
//  Arrow Up Navega a opción anterior
//  Home Navega a primera opción
//  End Navega a última opción
//  Enter(en menú abierto) Selecciona opción activa y cierra
//  Space(en menú abierto) Selecciona opción activa(NO cierra en multiselect, SÍ cierra en single)
//  Escape Cierra sin seleccionar
//  Tab Cierra el menú y mueve focus al siguiente elemento

interface DropdownCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface DropdownInstance {
    triggerElement: HTMLElement;
    menuElement: HTMLElement | null;
    dotnetRef: DropdownCallbacksRelay;
    componentId: string;
}

interface DropdownPosition {
    triggerTop: number;
    triggerLeft: number;
    triggerWidth: number;
    triggerHeight: number;
    viewportHeight: number;
    viewportWidth: number;
    scrollY: number;
}

const dropdownInstances = new Map<string, DropdownInstance>();

// A single pair of document-level listeners is shared by every active
// dropdown instead of one pair per instance. Installed when the first
// dropdown initializes, removed when the last one disposes. The handlers
// dispatch to every registered instance — each one decides whether the
// event is its own by walking the trigger's `dropdown-container` ancestor.
const RELEVANT_KEYS = new Set(['Escape', 'ArrowDown', 'ArrowUp', 'Enter', 'Tab', 'Home', 'End']);

function handleClickOutside(e: MouseEvent): void {
    const target = e.target as HTMLElement;
    for (const instance of dropdownInstances.values()) {
        const container = instance.triggerElement.closest('[data-bob-component="dropdown-container"]');
        if (container && !container.contains(target)) {
            instance.dotnetRef.invokeMethodAsync('OnClickOutside');
        }
    }
}

function handleKeyDown(e: KeyboardEvent): void {
    for (const instance of dropdownInstances.values()) {
        const container = instance.triggerElement.closest('[data-bob-component="dropdown-container"]');
        if (!container?.contains(document.activeElement)) continue;

        const isOpen = container.getAttribute('data-bob-dropdown-open') === 'true';
        const active = document.activeElement as HTMLElement | null;
        const trigger = instance.triggerElement.querySelector('.bob-dropdown__trigger');
        const isMenuButton = isOpen && active?.tagName === 'BUTTON' && active !== trigger;

        // Enter / Space on menu-internal buttons (Select All, Deselect All, action cells) —
        // let the native button activation fire instead of routing through OnKeyDown.
        if (isMenuButton && (e.key === 'Enter' || e.key === ' ')) {
            return;
        }

        if (e.key === ' ' && isOpen) {
            e.preventDefault();
            instance.dotnetRef.invokeMethodAsync('OnKeyDown', e.key, e.shiftKey, e.ctrlKey);
            return;
        }

        if (e.key.toLowerCase() === 'a' && e.ctrlKey && isOpen) {
            e.preventDefault();
            instance.dotnetRef.invokeMethodAsync('OnKeyDown', e.key.toLowerCase(), e.shiftKey, e.ctrlKey);
            return;
        }

        if (RELEVANT_KEYS.has(e.key)) {
            e.preventDefault();
            instance.dotnetRef.invokeMethodAsync('OnKeyDown', e.key, e.shiftKey, e.ctrlKey);
            return;
        }
    }
}

let listenersInstalled = false;
function ensureListeners(): void {
    if (listenersInstalled || dropdownInstances.size === 0) return;
    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleKeyDown);
    listenersInstalled = true;
}

function maybeRemoveListeners(): void {
    if (!listenersInstalled || dropdownInstances.size > 0) return;
    document.removeEventListener('mousedown', handleClickOutside);
    document.removeEventListener('keydown', handleKeyDown);
    listenersInstalled = false;
}

export function initialize(
    triggerElement: HTMLElement,
    menuElement: HTMLElement | null,
    dotnetRef: DropdownCallbacksRelay,
    componentId: string
): void {
    if (!triggerElement || !componentId) return;

    dispose(componentId);

    dropdownInstances.set(componentId, {
        triggerElement,
        menuElement,
        dotnetRef,
        componentId
    });

    ensureListeners();
}

export function getPosition(componentId: string): DropdownPosition | null {
    const instance = dropdownInstances.get(componentId);
    if (!instance) return null;

    const rect = instance.triggerElement.getBoundingClientRect();

    return {
        triggerTop: rect.top,
        triggerLeft: rect.left,
        triggerWidth: rect.width,
        triggerHeight: rect.height,
        viewportHeight: window.innerHeight,
        viewportWidth: window.innerWidth,
        scrollY: window.scrollY
    };
}

export function focusSearchInput(componentId: string): void {
    const instance = dropdownInstances.get(componentId);
    if (!instance) return;

    const container = instance.triggerElement.closest('[data-bob-component="dropdown-container"]');
    const searchInput = container?.querySelector('.bob-dropdown__search input') as HTMLInputElement | null;

    if (searchInput) {
        searchInput.focus();
        searchInput.select();
    }
}

export function dispose(componentId: string): void {
    if (!dropdownInstances.delete(componentId)) return;
    maybeRemoveListeners();
}