// Prevent theme flash on page load. Runs before the page body parses so the
// `data-theme` attribute on <html> is set before any styled element renders.
//
// Sanitize the stored theme id: an attacker-controlled localStorage value
// must not land inside the `data-theme="..."` attribute selector surface
// (see SEC-04 / ThemeInterop.ts). Allow-list: 1-32 ASCII letters/digits/_/-.
//
// Shipped as a StaticWebAsset under `_content/BlazOrbit/anti-flash.js`
// so consumer apps with strict CSPs can allow it via `script-src 'self'`
// without needing inline-script exceptions or per-request nonces. See SEC-01.
(function () {
    var re = /^[a-zA-Z0-9_-]{1,32}$/;
    var t = localStorage.getItem('blazorbit-theme');
    if (!t || !re.test(t)) t = 'dark';
    document.documentElement.setAttribute('data-theme', t);
})();
