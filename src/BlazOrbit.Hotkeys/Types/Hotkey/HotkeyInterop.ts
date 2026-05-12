// Global keyboard listener bridge. The host component on the .NET side calls
// `attach` once per circuit/runtime; subsequent register/unregister of individual
// combos pushes through `registerCombo` / `unregisterCombo` so the JS handler can
// synchronously call `event.preventDefault()` on match — async dispatch alone is
// too late to suppress the browser's default action (e.g. Ctrl+S Save dialog).

interface HotkeyRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface HotkeyInstance {
    relay: HotkeyRelay;
    handler: (e: KeyboardEvent) => void;
    // combo (canonical, lowercase) -> preventDefault flag. The set drives the sync
    // preventDefault decision; the .NET dispatch still runs async to invoke handlers.
    combos: Map<string, boolean>;
}

const instances = new Map<string, HotkeyInstance>();

function isTypingTarget(target: EventTarget | null): boolean {
    if (!target) return false;
    const el = target as HTMLElement;
    if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.tagName === 'SELECT') return true;
    if (el.isContentEditable) return true;
    return false;
}

function serializeCombo(e: KeyboardEvent): string {
    // Stable canonical form: "ctrl+shift+k". Order is fixed so the .NET side can do
    // straight string equality without normalising on every keystroke.
    const parts: string[] = [];
    if (e.ctrlKey) parts.push('ctrl');
    if (e.metaKey) parts.push('meta');
    if (e.altKey) parts.push('alt');
    if (e.shiftKey) parts.push('shift');
    parts.push(e.key.toLowerCase());
    return parts.join('+');
}

export function attach(hostId: string, relay: HotkeyRelay): void {
    if (instances.has(hostId)) {
        detach(hostId);
    }

    const inst: HotkeyInstance = {
        relay,
        combos: new Map<string, boolean>(),
        handler: (() => { /* placeholder, replaced below */ }) as (e: KeyboardEvent) => void,
    };

    inst.handler = (e: KeyboardEvent) => {
        if (isTypingTarget(e.target)) return;
        const combo = serializeCombo(e);
        const preventDefault = inst.combos.get(combo);
        if (preventDefault === true) {
            // Suppress the browser default synchronously — by the time the .NET roundtrip
            // resolves the browser has already executed it.
            e.preventDefault();
        }
        // Always dispatch — the .NET side decides whether any handler matches. Registered
        // combos pass through here even if the JS registry hasn't caught up yet (race
        // window during initial mount).
        inst.relay.invokeMethodAsync('OnHotkey', combo).catch(() => {
            // Swallow circuit-tear-down races; the dispatch is best-effort.
        });
    };

    instances.set(hostId, inst);
    document.addEventListener('keydown', inst.handler);
}

export function detach(hostId: string): void {
    const inst = instances.get(hostId);
    if (!inst) return;
    document.removeEventListener('keydown', inst.handler);
    instances.delete(hostId);
}

export function registerCombo(hostId: string, combo: string, preventDefault: boolean): void {
    const inst = instances.get(hostId);
    if (!inst) return;
    inst.combos.set(combo.toLowerCase(), preventDefault);
}

export function unregisterCombo(hostId: string, combo: string): void {
    const inst = instances.get(hostId);
    if (!inst) return;
    inst.combos.delete(combo.toLowerCase());
}
