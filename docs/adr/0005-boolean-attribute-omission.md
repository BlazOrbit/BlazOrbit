# ADR-0005: Boolean Attribute Omission Convention

**Status**: Accepted  
**Date**: 2026-03-03  
**Deciders**: BlazOrbit

---

## Context

`BOBComponentAttributesBuilder` emits boolean state as `data-bob-*` attributes. Two conventions were possible:

1. **Always emit** — `data-bob-disabled="true"` when active, `data-bob-disabled="false"` when inactive.
2. **Omit when false** — `data-bob-disabled="true"` when active, attribute removed entirely when inactive.

The "always emit" pattern is explicit but produces noisier DOM and requires CSS to target both `[data-bob-disabled="true"]` and `:not([data-bob-disabled="true"])` for the negative case. It also creates a risk of inconsistent string values (`"false"`, `"False"`, `"0"`).

## Decision

**Boolean `data-bob-*` attributes are omitted entirely when the state is `false`.** The helper `SetBoolAttr` implements this:

```csharp
private static void SetBoolAttr(Dictionary<string, object> attrs, string key, bool value)
{
    if (value)
        attrs[key] = "true";
    else
        attrs.Remove(key);
}
```

CSS targets the positive case with `[data-bob-disabled="true"]` and the negative case with `:not([data-bob-disabled="true"])` (or simply the absence of the attribute).

Example:

```css
/* Disabled */
[data-bob-component="button"][data-bob-disabled="true"] button {
    opacity: var(--bob-opacity-disabled);
}

/* Not disabled — no extra selector needed; the base style already applies */
```

## Consequences

### Positive

- **Smaller DOM**: inactive state does not bloat the markup.
- **Unambiguous**: `"true"` is the only string value ever emitted for a boolean attribute. No risk of `"false"` / `"False"` / `"0"` drift.
- **Simpler CSS**: most components do not need a negative selector. The base style applies by default; the attribute selector only overrides when active.
- **Cleaner tests**: `GetAttribute("data-bob-disabled").Should().BeNull()` is a crisp assertion for the inactive state.

### Negative

- **Explicit false is impossible**: a consumer cannot distinguish "not disabled" from "disabled was never set" in the DOM. This is intentional — the semantic meaning is identical.
- **Transition from absence**: CSS transitions that animate on attribute removal require `@starting-style` or a technique like `[data-bob-closing="true"]` (see `BOBModalContainer`). For boolean state that needs exit animation, a separate transient attribute (e.g. `data-bob-closing`) is used.

## References

- `src/BlazOrbit.Core/Components/BOBComponentAttributesBuilder.cs` — `SetBoolAttr`
- `test/BlazOrbit.Tests.Integration/Tests/Components/Button/BOBButtonStateTests.cs` — assertions against `null` for inactive state
