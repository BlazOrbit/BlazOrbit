window.DocSearchShortcuts = {
    register: function (dotnetRef) {
        var handler = function (e) {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                dotnetRef.invokeMethodAsync('OpenSearch');
            }
            if (e.key === '/' && !['INPUT', 'TEXTAREA', 'SELECT'].includes(e.target.tagName)) {
                e.preventDefault();
                dotnetRef.invokeMethodAsync('OpenSearch');
            }
        };
        document.addEventListener('keydown', handler);
        return {
            dispose: function () {
                document.removeEventListener('keydown', handler);
            }
        };
    },
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
