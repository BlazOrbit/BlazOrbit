// BlazOrbit.Hotkeys - interop module (hand-written JSDoc-typed ESM).
//
// Global keyboard listener bridge. The host component on the .NET side calls
// `attach` once per circuit/runtime; subsequent register/unregister of individual
// combos pushes through `registerCombo` / `unregisterCombo` so the JS handler can
// synchronously call `event.preventDefault()` on match - async dispatch alone is
// too late to suppress the browser's default action (e.g. Ctrl+S Save dialog).

/**
 * @typedef {Object} HotkeyRelay
 * @property {(methodName: string, ...args: unknown[]) => Promise<unknown>} invokeMethodAsync
 */

/**
 * @typedef {Object} HotkeyInstance
 * @property {HotkeyRelay} relay
 * @property {(e: KeyboardEvent) => void} handler
 * @property {Map<string, boolean>} combos combo (canonical, lowercase) -> preventDefault flag.
 */

/** @type {Map<string, HotkeyInstance>} */
const instances = new Map();

/**
 * @param {EventTarget | null} target
 * @returns {boolean}
 */
function isTypingTarget(target) {
    if (!target) return false;
    const el = /** @type {HTMLElement} */ (target);
    if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.tagName === 'SELECT') return true;
    if (el.isContentEditable) return true;
    return false;
}

/**
 * Stable canonical form: "ctrl+shift+k". Order is fixed so the .NET side can do
 * straight string equality without normalising on every keystroke.
 * @param {KeyboardEvent} e
 * @returns {string}
 */
function serializeCombo(e) {
    /** @type {string[]} */
    const parts = [];
    if (e.ctrlKey) parts.push('ctrl');
    if (e.metaKey) parts.push('meta');
    if (e.altKey) parts.push('alt');
    if (e.shiftKey) parts.push('shift');
    parts.push(e.key.toLowerCase());
    return parts.join('+');
}

/**
 * @param {string} hostId
 * @param {HotkeyRelay} relay
 */
export function attach(hostId, relay) {
    if (instances.has(hostId)) {
        detach(hostId);
    }

    /** @type {HotkeyInstance} */
    const inst = {
        relay,
        combos: new Map(),
        handler: () => { /* placeholder, replaced below */ }
    };

    inst.handler = (e) => {
        if (isTypingTarget(e.target)) return;
        const combo = serializeCombo(e);
        const preventDefault = inst.combos.get(combo);
        if (preventDefault === true) {
            // Suppress the browser default synchronously - by the time the .NET roundtrip
            // resolves the browser has already executed it.
            e.preventDefault();
        }
        // Always dispatch - the .NET side decides whether any handler matches. Registered
        // combos pass through here even if the JS registry hasn't caught up yet (race
        // window during initial mount).
        inst.relay.invokeMethodAsync('OnHotkey', combo).catch(() => {
            // Swallow circuit-tear-down races; the dispatch is best-effort.
        });
    };

    instances.set(hostId, inst);
    document.addEventListener('keydown', inst.handler);
}

/** @param {string} hostId */
export function detach(hostId) {
    const inst = instances.get(hostId);
    if (!inst) return;
    document.removeEventListener('keydown', inst.handler);
    instances.delete(hostId);
}

/**
 * @param {string} hostId
 * @param {string} combo
 * @param {boolean} preventDefault
 */
export function registerCombo(hostId, combo, preventDefault) {
    const inst = instances.get(hostId);
    if (!inst) return;
    inst.combos.set(combo.toLowerCase(), preventDefault);
}

/**
 * @param {string} hostId
 * @param {string} combo
 */
export function unregisterCombo(hostId, combo) {
    const inst = instances.get(hostId);
    if (!inst) return;
    inst.combos.delete(combo.toLowerCase());
}
