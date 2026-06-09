// Tiny utility kept around for the imperative "focus the search input" call. The Ctrl+K
// and "/" shortcut wiring previously lived in this file; the docs site now drives them
// through the BlazOrbit.Hotkeys service, so the keydown handler here is intentionally
// gone.
window.DocSearchShortcuts = {
    focusInput: function (selector) {
        var el = document.querySelector(selector);
        if (el) {
            el.focus();
            if (typeof el.select === 'function') {
                el.select();
            }
        }
    }
};
