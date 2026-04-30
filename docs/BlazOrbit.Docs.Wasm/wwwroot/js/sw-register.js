// Registers the docs-site service worker after the page loads. External
// rather than inline so it works under a strict CSP `script-src 'self'`
// directive. Failures swallowed: a missing/blocked SW is not a fatal error.
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('service-worker.js').catch(() => { });
    });
}
