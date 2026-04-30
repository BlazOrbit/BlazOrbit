// Captures the `beforeinstallprompt` event so the docs site can offer a
// custom "Install app" CTA instead of the browser's default. Lives as an
// external file (rather than inline in index.html) so it works under a
// strict CSP `script-src 'self'` directive.
window.__bobInstall = { deferred: null, installed: false, listeners: [] };

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    window.__bobInstall.deferred = e;
    window.__bobInstall.listeners.forEach((fn) => {
        try { fn(); } catch { /* listener-side error must not block siblings */ }
    });
});

window.addEventListener('appinstalled', () => {
    window.__bobInstall.deferred = null;
    window.__bobInstall.installed = true;
    window.__bobInstall.listeners.forEach((fn) => {
        try { fn(); } catch { /* listener-side error must not block siblings */ }
    });
});
