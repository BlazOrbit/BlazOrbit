# ADR-0002: Interface-Driven Component Styling (`IHas*` Axes)

**Status**: Accepted  
**Date**: 2026-03-03  
**Deciders**: BlazOrbit

---

## Context

In a component library with dozens of components, styling logic tends to scatter:
- Some components accept a `Color` parameter, others `TextColor` or `FgColor`.
- Some toggle a `Disabled` class manually; others rely on a base class.
- CSS selectors use inconsistent naming (`--primary`, `--theme-primary`, `--btn-primary`).

This fragmentation makes the library hard to extend, hard to theme, and hard to test.

## Decision

**Component styling shall be interface-driven.** A component advertises each styling capability by implementing a marker or property interface from `BlazOrbit.Core/Abstractions/Behaviors/`. A single central builder (`BOBComponentAttributesBuilder`) reflects over the component at render time and emits the corresponding `data-bob-*` attributes and `--bob-inline-*` CSS variables.

The component author only:
1. Declares `@implements IHasXxx`.
2. Declares the `[Parameter]` required by the interface.
3. (Optionally) provides a computed `IsX` property for state axes.

The builder handles the rest.

### Axis Categories

| Category | Purpose | Example Interfaces |
|---|---|---|
| **State** | Binary runtime state | `IHasDisabled`, `IHasError`, `IHasLoading`, `IHasActive` |
| **Design** | Visual appearance | `IHasSize`, `IHasDensity`, `IHasColor`, `IHasShadow`, `IHasBorder` |
| **Transitions** | Motion | `IHasTransitions` |
| **JavaScript** | Behavior configuration | `IHasRipple` |
| **Families** | Shared CSS opt-in | `IInputFamilyComponent`, `IPickerFamilyComponent` |
| **Variants** | Template dispatch | `IVariantComponent<TVariant>` |

## Consequences

### Positive

- **Consistency**: every component that can be disabled uses the same `IHasDisabled` contract, emits the same `data-bob-disabled` attribute, and is styled by the same CSS selector.
- **Discoverability**: a new contributor reads the interface catalog and immediately knows which styling axes are available.
- **Extensibility**: adding a new axis is a mechanical 10-step process (see `ARCHITECTURE.md`). No component needs to be updated unless it opts into the new axis.
- **Testability**: the builder can be unit-tested in isolation; components do not need redundant tests for attribute emission.

### Negative

- **Indirection cost**: a contributor must learn the interface catalog before writing a new component. The payoff is worth it after the first component.
- **Collision risk**: two interfaces could theoretically emit the same `data-bob-*` key. The builder does not detect this automatically — the rule is "do not create conflicting interfaces".

## References

- `src/BlazOrbit.Core/Abstractions/Behaviors/` — interface definitions
- `src/BlazOrbit.Core/Components/BOBComponentAttributesBuilder.cs` — reflection, `ComponentFeatures`, emit logic
- `src/BlazOrbit.Core/Css/FeatureDefinitions.cs` — canonical attribute and variable names
