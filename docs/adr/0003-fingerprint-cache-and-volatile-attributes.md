# ADR-0003: Fingerprint Cache with Volatile Attribute Patching

**Status**: Accepted  
**Date**: 2026-03-03 (revised 2026-05-06)  
**Deciders**: BlazOrbit

---

## Context

`BOBComponentAttributesBuilder.BuildStyles` reflects over the component, computes a hash of all style-relevant inputs (size, density, color, border, disabled, error, etc.), and rebuilds `ComputedAttributes` and the inline `style` string. This is expensive:
- Dictionary allocations for `ComputedAttributes` and CSS variables.
- StringBuilder work to concatenate CSS variables.
- Multiple interface casts.

In a typical UI session, a parent re-renders for reasons unrelated to style (e.g. `ChildContent` changed, a sibling updated, a timer fired). Rebuilding styles on every render would waste CPU and allocate memory unnecessarily.

At the same time, some state changes are **ultra-frequent**:
- `Disabled` flipping during a click handler.
- `Loading` toggling on every async operation.
- `Active` tracking hover or focus state.

These must update the DOM immediately without waiting for `OnParametersSet` (which only fires when a parameter reference changes).

## Decision

**Split style computation into two paths:**

1. **`BuildStyles` (fingerprint cache)** — runs in `OnParametersSet`. Computes a fingerprint over all style-relevant inputs. If the fingerprint and `AdditionalAttributes` reference are unchanged, skips the rebuild entirely.
2. **`PatchVolatileAttributes`** — runs in `BuildRenderTree` (every render). Re-writes the **six high-frequency state attributes** (`active`, `disabled`, `loading`, `error`, `readonly`, `required`) by reading the current `IsX` values directly. Defined as `ComponentFeatures.VolatileMask` in `BOBComponentAttributesBuilder`.

`FullWidth` (`IHasFullWidth`) is **not** volatile: it is a stable styling parameter set by the parent, never mutated by internal state, so it is folded into the fingerprint and emitted by `BuildStyles` only. This was a deliberate refinement on 2026-05-06: an earlier draft of this ADR listed it among the volatiles, but in practice no component flips it from internal state.

### Defense in depth

The volatile attributes are **also folded into the fingerprint**, not excluded. This is intentional:

- The fingerprint catches **parameter-driven** changes via the standard cache invalidation path (a flipped `Disabled` parameter triggers a full `BuildStyles` rebuild).
- `PatchVolatileAttributes` covers **internal-state** changes that fire `StateHasChanged` outside `OnParametersSet` — e.g. an input flipping `Loading` from a private async handler, or a tab updating `Active` on click. The patch runs on every `BuildRenderTree`, so cache hits never serve stale state.

The two paths overlap on parameter-driven changes (the patch re-emits the same value the rebuild already wrote), which is harmless and keeps the contract simple: "volatile attrs always reflect the current `IsX`, regardless of which path got us here".

### Cache Eligibility

Components implementing `IBuiltComponent` are **opted out of the cache** because their `BuildComponentDataAttributes` / `BuildComponentCssVariables` hooks may read opaque state (timers, GUIDs, counters) that is not folded into the fingerprint. Components implementing the refined `IPureBuiltComponent` marker promise their hooks read only `[Parameter]` / `IsX` state; the builder folds the hook output into the fingerprint via `OrderIndependentDictHash` and the cache applies.

### Fingerprint Inputs

```csharp
HashCode hc = new();
hc.Add(component.GetType());
// Variant, Size, Density, FullWidth,
// Loading, IsError, IsDisabled, IsActive, IsReadOnly, IsRequired,  ← also patched volatilely
// Prefix/Suffix colors, Shadow, Elevation, Ripple config,
// Color, BackgroundColor, Border, Transitions
```

## Consequences

### Positive

- **Performance**: style rebuilds are skipped for the majority of re-renders. The volatile patch is extremely cheap (six dictionary operations).
- **Correctness**: high-frequency state changes are visible in the DOM on the very next render, even when `OnParametersSet` does not fire. Cache hits never serve stale volatile state because the patch re-runs every render.
- **Simplicity**: the split is mechanical. Component authors do not think about it — the pipeline handles both paths transparently.

### Negative

- **Fingerprint maintenance**: adding a new `IHas*` axis requires updating `ComputeStyleFingerprint` or the cache will miss legitimate changes.
- **Reference-equality trap**: `AdditionalAttributes` is compared by reference identity. A parent that creates a new dictionary on every render defeats the cache.

## References

- `src/BlazOrbit.Core/Components/BOBComponentAttributesBuilder.cs` — `ComputeStyleFingerprint`, `BuildStyles`, `PatchVolatileAttributes`
- `src/BlazOrbit.Core/Components/BOBComponentPipeline.cs` — lifecycle hook wiring
- `src/BlazOrbit.Core/Components/BOBComponentBase.cs` — `OnParametersSet` / `BuildRenderTree` delegation
