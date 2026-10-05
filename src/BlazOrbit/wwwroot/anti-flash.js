// Prevent theme flash on page load. Runs before the page body parses so the
// `data-bob-theme` attribute on <html> is set before any styled element renders.
//
// Sanitize the stored theme id: an attacker-controlled localStorage value
// must not land inside the `data-bob-theme="..."` attribute selector surface.
// Allow-list: 1-32 ASCII letters/digits/_/-.
//
// Shipped as a StaticWebAsset under `_content/BlazOrbit/anti-flash.js`
// so consumer apps with strict CSPs can allow it via `script-src 'self'`
// without needing inline-script exceptions or per-request nonces.
//
// Resolution order mirrors `initialize()` in ThemeInterop.js so the theme does not
// change once Blazor starts: stored selection -> `data-default-theme` on this script
// tag (BOBInitializer.DefaultTheme) -> prefers-color-scheme -> 'dark'.
(function () {
    var re = /^[a-zA-Z0-9_-]{1,32}$/;
    var safe = function (v) { return typeof v === 'string' && re.test(v) ? v : null; };

    var saved = null;
    try { saved = localStorage.getItem('blazorbit-theme'); } catch (e) { /* storage blocked */ }

    var script = document.currentScript;
    var fallback = script ? script.getAttribute('data-default-theme') : null;

    var system = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light';

    var t = safe(saved) || safe(fallback) || system || 'dark';
    document.documentElement.setAttribute('data-bob-theme', t);
})();
