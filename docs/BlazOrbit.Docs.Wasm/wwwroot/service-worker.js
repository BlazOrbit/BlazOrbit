// Network-first service worker for the BlazOrbit docs site.
//
// Bump CACHE_VERSION on every shell change. The `activate` handler deletes
// any cache that does not match — old shells are evicted on the next visit.
// CACHE_VERSION is replaced in publish time.
const CACHE_VERSION = 'blazorbit-docs-v1';

// App shell precached on install so the site is reachable offline after the
// first visit. Keep the list short — Blazor's `_framework/*` payloads are
// large and varied per build, so they are filled in lazily by the fetch
// handler below.
const PRECACHE_URLS = [
    './',
    './index.html',
    './icon.svg',
    './icon-48.png',
    './icon-144.png',
    './icon-192.png',
    './icon-512.png',
    './og-image.png',
    './manifest.webmanifest',
    './_content/BlazOrbit/css/blazorbit.css'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_VERSION).then((cache) =>
            // `addAll` is atomic — if any URL fails, the install is rejected,
            // so map each URL through `Request` with a `no-cache` mode to
            // bypass any stale browser cache during the precache fetch.
            cache.addAll(PRECACHE_URLS.map((url) => new Request(url, { cache: 'reload' })))
        ).then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((keys) => Promise.all(
                keys.filter((key) => key !== CACHE_VERSION).map((key) => caches.delete(key))
            ))
            .then(() => self.clients.claim())
    );
});

self.addEventListener('message', (event) => {
    // Only accept messages from our own origin — untrusted origins could
    // force an unexpected service-worker update.
    if (event.origin !== self.location.origin) return;

    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

// Network-first with cache fallback. On a successful network response, also
// write it back to the cache so the next offline visit has it available.
//
// Only GET requests are cached. POST / PUT / DELETE pass through unchanged.
// Cross-origin requests (analytics, fonts) also pass through — caching them
// would mask CORS errors and cookie behaviour.
self.addEventListener('fetch', (event) => {
    const req = event.request;

    if (req.method !== 'GET') return;

    const url = new URL(req.url);
    if (url.origin !== self.location.origin) return;

    event.respondWith(
        fetch(req)
            .then((response) => {
                // Only cache "complete" responses — opaque/redirect/error
                // responses can poison the cache.
                if (response && response.status === 200 && response.type === 'basic') {
                    const clone = response.clone();
                    caches.open(CACHE_VERSION).then((cache) => cache.put(req, clone));
                }
                return response;
            })
            .catch(() =>
                caches.match(req).then((cached) => cached || caches.match('./index.html').then((shell) => shell || Response.error()))
            )
    );
});
