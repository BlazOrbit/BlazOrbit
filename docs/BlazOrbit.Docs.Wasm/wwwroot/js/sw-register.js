// Registers the docs-site service worker and forces an immediate reload when
// a new version is waiting. This guarantees that users always run the latest
// Blazor WASM shell instead of a stale cached one.
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('service-worker.js')
            .then((registration) => {
                // If a new service worker is installing, watch it so we can
                // activate it as soon as it is ready.
                registration.addEventListener('updatefound', () => {
                    const newWorker = registration.installing;
                    if (!newWorker) return;

                    newWorker.addEventListener('statechange', () => {
                        // 'installed' + an active controller means there is a
                        // *new* worker waiting. Tell it to skip waiting; the
                        // controllerchange listener below will reload the page
                        // once the new worker takes control.
                        if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                            newWorker.postMessage({ type: 'SKIP_WAITING' });
                        }
                    });
                });
            })
            .catch(() => {
                // Failures swallowed: a missing/blocked SW is not a fatal error.
            });

        // Reload as soon as the new service worker becomes the active controller.
        // This is the moment the new cache version and assets become effective.
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            window.location.reload();
        });
    });
}
