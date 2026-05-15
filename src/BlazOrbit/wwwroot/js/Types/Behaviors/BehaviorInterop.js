// BlazOrbit - Behaviors interop (hand-written JSDoc-typed ESM).
// Source of truth: this file. No transpile step.

/**
 * @typedef {Object} RippleConfiguration
 * @property {string} [color]
 * @property {number} [duration]
 * @property {HTMLElement} rippleContainer
 */

/**
 * @typedef {Object} BehaviorConfiguration
 * @property {RippleConfiguration} [ripple]
 */

class RippleBehavior {
    /** @param {RippleConfiguration} config */
    constructor(config) {
        /** @type {HTMLElement} */
        this.element = config.rippleContainer;
        /** @type {RippleConfiguration} */
        this.config = config;
        /** @type {(e: MouseEvent) => void} */
        this.clickHandler = this.handleClick.bind(this);
        this.element.addEventListener('click', this.clickHandler);
    }

    /** @param {MouseEvent} e */
    handleClick(e) {
        const rect = this.element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = e.clientX - rect.left - size / 2;
        const y = e.clientY - rect.top - size / 2;

        const ripple = document.createElement('span');
        ripple.className = 'bob-ripple';
        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = x + 'px';
        ripple.style.top = y + 'px';

        if (this.config.color) {
            ripple.style.backgroundColor = this.config.color;
        }

        const duration = this.config.duration || 600;
        ripple.style.animationDuration = `${duration}ms`;

        this.element.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, duration);
    }

    dispose() {
        this.element.removeEventListener('click', this.clickHandler);
    }
}

class BehaviorManager {
    /** @param {BehaviorConfiguration} config */
    constructor(config) {
        /** @type {Array<{ dispose: () => void }>} */
        this.behaviors = [];
        if (config.ripple) {
            this.behaviors.push(new RippleBehavior(config.ripple));
        }
    }

    dispose() {
        this.behaviors.forEach(b => b.dispose());
        this.behaviors = [];
    }
}

// Canonical `initialize`/`dispose` shape across all interop modules. The
// instance returned still owns its own `dispose()` because behaviors are
// scoped to the BOB component, not to a global componentId map.
/**
 * @param {BehaviorConfiguration} config
 * @returns {BehaviorManager}
 */
export function initialize(config) {
    return new BehaviorManager(config);
}
