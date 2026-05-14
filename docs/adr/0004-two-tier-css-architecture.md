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

1. **Global bundle** — hand-written `src/BlazOrbit/wwwroot/css/blazorbit.css`. Contains:
   - CSS reset and typography
   - Theme tokens (`--palette-*`) and base variables (`--bob-size-multiplier`, `--bob-density-multiplier`)
   - Base component styles and family shared styles (input / picker / data-collection)
   - Transition classes and animation keyframes
   - `data-bob-size` / `data-bob-density` multiplier mapping

   Single source of truth. No generator, no bundler, no transpile step.

2. **Scoped component CSS** (hand-written `.razor.css`) — one file per `.razor`. Contains:
   - Component-specific layout
   - Private-variable declarations (`--_<component>-*`)
   - Child-element BEM selectors
   - State reactions on `[data-bob-component="<kebab-name>"]`

**Rule**: edit `blazorbit.css` in its matching section, or the per-component `.razor.css` next to its `.razor`.

## Consequences

### Positive

- **Encapsulation**: scoped CSS cannot leak to the consumer app. Global CSS is intentionally minimal and token-based.
- **Themability**: consumers override the full palette (`--palette-*`) and the full design-token catalog (`--bob-*` — typography, sizing, density, borders, outline, opacity, z-index, ripple, scrollbar, input/picker family defaults) at `:root` (or scoped per `html[data-bob-theme]`) without fighting component selectors. Per-instance escape hatches flow through `--bob-inline-*` set automatically by `IHas*` parameters.
- **Consistency**: family CSS (input, picker, data-collection) is generated once and shared across all members of the family.
- **Cacheability**: the global bundle is a single file with a stable URL; browsers cache it effectively.

### Negative

- **Bundle size**: because many selectors are dynamic (`data-bob-*`, variants, consumer fragments), we don't run any tree-shaker / PurgeCSS. The bundle is larger than a tree-shaken alternative, but safe and predictable. Consumers compress at the HTTP layer.
- **No mechanical reuse**: family-shared rules cannot be regenerated from a single C# source. Maintainers keep token names in sync with `FeatureDefinitions` by convention; `CssArchitectureLintTests` catches the cases that matter.

## References

- `src/BlazOrbit/wwwroot/css/blazorbit.css` — global bundle, single source of truth
- `CONTRIBUTING.md` — "Static Assets" section
