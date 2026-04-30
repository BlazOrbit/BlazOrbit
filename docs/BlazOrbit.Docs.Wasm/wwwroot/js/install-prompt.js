const store = window.__bobInstall = window.__bobInstall || { deferred: null, installed: false, listeners: [] };
let dotnetRef = null;

function detectPlatform() {
    const ua = navigator.userAgent || '';
    if (/iPhone|iPad|iPod/i.test(ua)) return 'ios';
    if (/Android/i.test(ua)) return 'android';
    if (/Edg\//i.test(ua)) return 'edge';
    if (/Chrome\//i.test(ua)) return 'chrome';
    if (/Firefox\//i.test(ua)) return 'firefox';
    if (/Safari\//i.test(ua)) return 'safari';
    return 'other';
}

function isStandalone() {
    return (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches)
        || window.navigator.standalone === true;
}

function snapshot() {
    return {
        canPrompt: store.deferred !== null,
        installed: store.installed,
        standalone: isStandalone(),
        platform: detectPlatform()
    };
}

function notify() {
    if (!dotnetRef) return;
    dotnetRef.invokeMethodAsync('OnStateChanged', snapshot()).catch(() => { });
}

store.listeners.push(notify);

export function register(ref) {
    dotnetRef = ref;
    notify();
}

export function unregister() {
    dotnetRef = null;
}

export async function prompt() {
    if (!store.deferred) return 'unavailable';
    const ev = store.deferred;
    store.deferred = null;
    try {
        await ev.prompt();
        const choice = await ev.userChoice;
        notify();
        return choice.outcome || 'dismissed';
    } catch {
        notify();
        return 'error';
    }
}

export function getState() {
    return snapshot();
}

export function getDismissed(key) {
    try { return localStorage.getItem(key) === '1'; } catch { return false; }
}

export function setDismissed(key) {
    try { localStorage.setItem(key, '1'); } catch { }
}
