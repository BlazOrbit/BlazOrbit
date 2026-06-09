// BlazOrbit - LocalStorage interop (hand-written JSDoc-typed ESM).

/**
 * @param {string} key
 * @returns {string | null}
 */
export function get(key) {
    return localStorage.getItem(key);
}

/**
 * @param {string} key
 * @param {string} value
 */
export function set(key, value) {
    localStorage.setItem(key, value);
}
