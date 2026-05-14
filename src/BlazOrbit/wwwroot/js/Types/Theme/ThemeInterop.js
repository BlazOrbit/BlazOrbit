// BlazOrbit — Theme interop (hand-written JSDoc-typed ESM).

const THEME_KEY = 'blazorbit-theme';
const DEFAULT_THEME = 'dark';

// Theme IDs land in `document.documentElement[data-bob-theme="..."]`, which becomes a
// selector target in CSS. An attacker with write access to localStorage (XSS in
// the consumer app, extension, same-origin script) could inject a value that
// doubles as a CSS attribute selector (`x"] { ... } [data-secret="y`) — even if
// the immediate impact is limited to CSS, storage-sourced strings must never
// reach the DOM unvalidated.
const THEME_ID_PATTERN = /^[a-zA-Z0-9_-]{1,32}$/;

/**
 * @param {string | null | undefined} value
 * @returns {value is string}
 */
function isSafeThemeId(value) {
    return typeof value === 'string' && THEME_ID_PATTERN.test(value);
}

/** @returns {string} */
function getSystemPreference() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light';
}

/** @param {string} [defaultTheme] */
export function initialize(defaultTheme) {
    // Priority order:
    // 1. localStorage (user's manual selection) — only if it passes sanitation
    // 2. defaultTheme parameter (if provided by the consumer)
    // 3. System preference (prefers-color-scheme)
    // 4. DEFAULT_THEME constant ('dark')
    const savedTheme = localStorage.getItem(THEME_KEY);
    const safeSaved = isSafeThemeId(savedTheme) ? savedTheme : null;
    const safeDefault = isSafeThemeId(defaultTheme) ? defaultTheme : null;
    const theme = safeSaved ?? safeDefault ?? getSystemPreference() ?? DEFAULT_THEME;

    document.documentElement.setAttribute('data-bob-theme', theme);
}

/** @returns {string} */
export function getTheme() {
    return document.documentElement.getAttribute('data-bob-theme') ?? DEFAULT_THEME;
}

/** @param {string} theme */
export function setTheme(theme) {
    if (!isSafeThemeId(theme)) return;
    localStorage.setItem(THEME_KEY, theme);
    document.documentElement.setAttribute('data-bob-theme', theme);
}

/**
 * @param {string[]} themes
 * @returns {string}
 */
export function toggleTheme(themes) {
    const current = getTheme();
    const index = themes.indexOf(current);
    const next = themes[(index + 1) % themes.length];
    setTheme(next);
    return next;
}

// Kept in sync with BOBThemePaletteBase. Canonical source lives in C#; if a property is
// added/removed there, update this list too.
const PALETTE_VARS = [
    '--palette-active-tint',
    '--palette-background',
    '--palette-background-contrast',
    '--palette-border',
    '--palette-error',
    '--palette-error-contrast',
    '--palette-highlight',
    '--palette-hover-tint',
    '--palette-info',
    '--palette-info-contrast',
    '--palette-primary',
    '--palette-primary-contrast',
    '--palette-secondary',
    '--palette-secondary-contrast',
    '--palette-shadow',
    '--palette-success',
    '--palette-success-contrast',
    '--palette-surface',
    '--palette-surface-contrast',
    '--palette-warning',
    '--palette-warning-contrast'
];

/** @returns {Promise<void>} */
function waitForStyles() {
    if (document.readyState === 'complete') {
        return Promise.resolve();
    }
    return new Promise(resolve => {
        window.addEventListener('load', () => resolve(), { once: true });
    });
}

/** @returns {Promise<Record<string, string>>} */
export async function getPalette() {
    await waitForStyles();

    return new Promise(resolve => {
        requestAnimationFrame(() => {
            const style = getComputedStyle(document.documentElement);
            /** @type {Record<string, string>} */
            const result = {};
            for (const v of PALETTE_VARS) {
                result[v] = style.getPropertyValue(v).trim();
            }
            resolve(result);
        });
    });
}
