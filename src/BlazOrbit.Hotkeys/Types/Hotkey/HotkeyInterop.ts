// Global keyboard listener bridge. The host component on the .NET side calls
// `attach` once per circuit/runtime; subsequent `Register/Unregister` happens
// purely in C# without re-entering JS. We funnel every keydown back to .NET via
// the relay and let the service decide which combo, if any, matches.

interface HotkeyRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface HotkeyInstance {
    relay: HotkeyRelay;
    handler: (e: KeyboardEvent) => void;
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

    const handler = (e: KeyboardEvent) => {
        if (isTypingTarget(e.target)) return;
        const combo = serializeCombo(e);
        // Fire-and-forget; the .NET side decides whether to invoke any handler.
        // We do not preventDefault here — the C# side requests prevention via the
        // PreventDefault return value when a handler matched.
        relay.invokeMethodAsync('OnHotkey', combo).then((preventDefault: unknown) => {
            if (preventDefault === true) {
                e.preventDefault();
            }
        });
    };

    instances.set(hostId, { relay, handler });
    document.addEventListener('keydown', handler);
}

export function detach(hostId: string): void {
    const inst = instances.get(hostId);
    if (!inst) return;
    document.removeEventListener('keydown', inst.handler);
    instances.delete(hostId);
}
