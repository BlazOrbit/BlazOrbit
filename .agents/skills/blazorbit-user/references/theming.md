<!-- handcrafted: do NOT regenerate. The regenerator only writes
     components.md / variants.md / icons.md. -->

# Theming and Design Tokens

Snapshot of the public CSS surface that consumers can override.

Two override surfaces ship for downstream apps:

1. **Palette** (`--palette-*`) — semantic colors. Drives every component automatically.
2. **Design tokens** (`--bob-*`) — non-color primitives: typography, sizing/density multipliers, borders, focus outline, opacity, z-index, ripple, scrollbar, input/picker family defaults. Theme-agnostic by default.

Override either at `:root` for global defaults or under `html[data-bob-theme="<id>"]` to scope to a theme.
Per-instance overrides flow through `--bob-inline-*` set automatically by `IHas*` parameters — you rarely touch those by hand.

> Tokens listed below are stable across releases. New tokens may be added; renames will
> be flagged in the changelog. The Theme Generator at `/utils/themegenerator` exposes both surfaces as a live editor with JSON / CSS / C# export.

## Palette

Public color contract. Mirror of `BlazOrbit.Core.Css.PaletteColor` (use those statics
from C# wherever a string color is accepted — they have an implicit `string` conversion).

```css
--palette-primary              --palette-primary-contrast
--palette-secondary            --palette-secondary-contrast
--palette-surface              --palette-surface-contrast
--palette-background           --palette-background-contrast
--palette-error                --palette-error-contrast
--palette-success              --palette-success-contrast
--palette-warning              --palette-warning-contrast
--palette-info                 --palette-info-contrast
--palette-border               --palette-shadow
--palette-highlight            /* focus outline accent */
--palette-hover-tint           /* translucent hover overlay */
--palette-active-tint          /* translucent active overlay */
```

## Sizing & Density

Active multiplier is exposed as `--bob-size-multiplier` / `--bob-density-multiplier` and
set by the framework when the consumer assigns `Size` / `Density` parameters. Override the
per-step values to retune the scale globally.

| Step | Variable | Default | Notes |
|------|----------|---------|-------|
| Small       | `--bob-small-multiplier`       | `0.75` | `BOBSize.Small`           |
| Medium      | `--bob-medium-multiplier`      | `1`    | `BOBSize.Medium` (default)|
| Large       | `--bob-large-multiplier`       | `1.25` | `BOBSize.Large`           |
| Compact     | `--bob-compact-multiplier`     | `0.75` | `BOBDensity.Compact`      |
| Standard    | `--bob-standard-multiplier`    | `1`    | `BOBDensity.Standard` (default) |
| Comfortable | `--bob-comfortable-multiplier` | `1.25` | `BOBDensity.Comfortable`  |

Components scale via `calc(<base> * var(--bob-size-multiplier, 1))` (and the density
counterpart). To tune a single component without affecting others, prefer per-instance
inline overrides (e.g. set `Border` / `Shadow` directly) instead of editing the multipliers.

## Border & Radius defaults

```css
--bob-border-width:           0px;
--bob-border-style:           solid;
--bob-border-radius:          2px;
--bob-input-radius:           4px;
--bob-picker-radius:          8px;
```

## Typography

```css
--bob-font-family:            system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
--bob-font-family-heading:    inherit;
--bob-font-mono:              ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
--bob-font-size-base:         clamp(0.875rem, 0.75rem + 0.25vw, 1.125rem);
--bob-line-height:            1.5;
--bob-line-height-heading:    1.2;
```

## Focus & opacity

```css
--bob-highlight-outline:        2px solid var(--palette-highlight);
--bob-highlight-outline-offset: 0px;
--bob-opacity-disabled:         0.5;
--bob-opacity-placeholder:      0.5;
```

## Z-index scale

Stay inside this scale; do not introduce ad-hoc `z-index` values.

| Layer    | Variable           | Default |
|----------|--------------------|---------|
| Dropdown | `--bob-z-dropdown` | `1000`  |
| Sticky   | `--bob-z-sticky`   | `1100`  |
| Modal    | `--bob-z-modal`    | `1300`  |
| Tooltip  | `--bob-z-tooltip`  | `1400`  |
| Toast    | `--bob-z-toast`    | `1500`  |

## Family-specific tokens

```css
/* Input family (BOBInputText, BOBInputNumber, BOBInputDateTime, ...) */
--bob-input-radius:               4px;
--bob-input-transition-duration:  150ms;
--bob-input-transition-easing:    cubic-bezier(0.4, 0, 0.2, 1);
--bob-input-floated-scale:        0.75;

/* Picker family (BOBDatePicker, BOBTimePicker, BOBColorPicker) */
--bob-picker-radius:              8px;
--bob-picker-cell-size:           36px;
--bob-picker-padding:             0.75rem;

/* Ripple */
--bob-ripple-color:               currentColor;
--bob-ripple-duration:            600ms;

/* Scrollbar */
--bob-scrollbar-width:            10px;
--bob-scrollbar-thumb-radius:     8px;
--bob-scrollbar-thumb-border-width: 2px;
```

## Inline (per-instance) CSS variables

Emitted by `BOBComponentAttributesBuilder` based on which `IHas*` interfaces a component
implements. Set the matching parameter on the component instead of writing CSS.

| Parameter interface | CSS variable(s) emitted |
|---------------------|-------------------------|
| `IHasColor`           | `--bob-inline-color` |
| `IHasBackgroundColor` | `--bob-inline-background` |
| `IHasBorder`          | `--bob-inline-border`, per-side (`--bob-inline-border-top`, …), `--bob-inline-border-radius` |
| `IHasShadow`          | `--bob-inline-shadow` |
| `IHasRipple`          | `--bob-inline-ripple-color`, `--bob-inline-ripple-duration` |
| `IHasPrefix`          | `--bob-inline-prefix-color`, `--bob-inline-prefix-background` |
| `IHasSuffix`          | `--bob-inline-suffix-color`, `--bob-inline-suffix-background` |
| `IHasElevation`       | `--bob-inline-elevation-tint` |

## Transitions

Per-trigger / per-property tokens are emitted dynamically by `IHasTransitions`-aware
components: variables follow `--bob-t-{trigger}-{property}` (e.g. `--bob-t-hover-scale`,
`--bob-t-focus-opacity`). Don't set them directly — pass a `BOBTransitions` value built from
`BOBTransitionPresets` (or compose your own) as the `Transitions` parameter. See
`presets.md`.

## Data attributes (state)

Rewritten on every render via `PatchVolatileAttributes` so they always reflect computed state.
Use them as CSS hooks when you need state-aware styling without writing C#.

| Attribute | Meaning |
|-----------|---------|
| `data-bob-component` | Component kebab name (e.g. `button`, `input-text`, `card`) |
| `data-bob-variant`   | Active variant name |
| `data-bob-size`      | `small` / `medium` / `large` |
| `data-bob-density`   | `compact` / `standard` / `comfortable` |
| `data-bob-disabled`  | Computed `IsDisabled` is `true` |
| `data-bob-loading`   | `Loading` is `true` |
| `data-bob-error`     | Computed `IsError` is `true` (inputs) |
| `data-bob-readonly`  | Computed `IsReadOnly` is `true` (inputs) |
| `data-bob-required`  | Computed `IsRequired` is `true` (inputs) |
| `data-bob-active`    | Computed `IsActive` is `true` |
| `data-bob-fullwidth` | `FullWidth` is `true` |
| `data-bob-shadow`    | A `Shadow` value is set |
| `data-bob-ripple`    | Ripple enabled |
| `data-bob-floated`   | Floated label active (inputs) |

## Override examples

```css
/* Global token tweaks */
:root {
    --bob-font-family:    "Inter", system-ui, sans-serif;
    --bob-border-radius:  8px;
    --bob-input-radius:   6px;
}

/* Theme-scoped palette + token override */
html[data-bob-theme="brand-dark"] {
    --palette-primary:           #5b8def;
    --palette-primary-contrast:  #ffffff;
    --palette-surface:           #1a1d23;
    --palette-surface-contrast:  #f1f3f7;
    --bob-highlight-outline:     3px solid var(--palette-primary);
}
```

