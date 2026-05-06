# ADR-0004: Two-Tier CSS Architecture (Global Generated Bundle + Scoped Component CSS)

**Status**: Accepted  
**Date**: 2026-03-03  
**Deciders**: BlazOrbit

---

## Context

Blazor component libraries can ship CSS in two ways:
1. **Global CSS** — a single `.css` file imported by the consumer. Good for resets, themes, tokens, and shared patterns. Bad for encapsulation; selectors can collide with consumer styles.
2. **Scoped CSS** — Blazor CSS isolation compiles `.razor.css` into `[b-xxx]`-scoped selectors. Good for component-specific layout. Bad for theming because tokens cannot be overridden from the outside without `:root` variables.

We needed both: a shared token layer that consumers can theme, plus per-component layout that cannot leak.

## Decision

**Ship two CSS layers:**

1. **Global bundle** (generated at build time) — lives in `wwwroot/css/blazorbit.css`. Contains:
   - CSS reset and typography
   - Theme tokens (`--palette-*`) and base variables (`--bob-size-multiplier`, `--bob-density-multiplier`)
   - Base component styles and family shared styles (`_input-family.css`, `_picker-family.css`, `_data-collection-family.css`)
   - Transition classes and animation keyframes
   - `data-bob-size` / `data-bob-density` multiplier mapping

   Produced by `[AssetGenerator]` classes in `src/BlazOrbit.BuildTools/Generators/` written to `src/BlazOrbit/CssBundle/`, then bundled by Vite.

2. **Scoped component CSS** (hand-written `.razor.css`) — one file per `.razor`. Contains:
   - Component-specific layout
   - Private-variable declarations (`--_<component>-*`)
   - Child-element BEM selectors
   - State reactions on `[data-bob-component="<kebab-name>"]`

**Rule**: edit the generator or `.razor.css`, never the generated output.

## Consequences

### Positive

- **Encapsulation**: scoped CSS cannot leak to the consumer app. Global CSS is intentionally minimal and token-based.
- **Themability**: consumers override `--palette-primary`, `--bob-size-multiplier`, etc. at `:root` without fighting component selectors.
- **Consistency**: family CSS (input, picker, data-collection) is generated once and shared across all members of the family.
- **Cacheability**: the global bundle is a single file with a stable URL; browsers cache it effectively.

### Negative

- **Build complexity**: the generator + Vite pipeline adds a build step. Contributors must have Node.js installed.
- **Indirection**: to change a family style, you edit a C# generator and rebuild, not a CSS file. There is a learning curve.
- **No PurgeCSS**: because many selectors are dynamic (`data-bob-*`, variants, consumer fragments), we do not run PurgeCSS. The bundle is larger than a tree-shaken alternative, but safe.

## References

- `src/BlazOrbit.BuildTools/Generators/` — `[AssetGenerator]` classes
- `src/BlazOrbit/CssBundle/` — generated intermediate CSS
- `src/BlazOrbit/wwwroot/css/blazorbit.css` — final Vite bundle
- `CONTRIBUTING.md` — "Generated Assets" section
