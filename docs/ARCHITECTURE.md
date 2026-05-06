# BlazOrbit Architecture Guide

> **Living document** — last verified against `src/` and `test/` on 2026-05-05.  
> When you change a public contract, a DOM convention, or a lifecycle rule, update this file in the same PR.

This guide captures the cross-cutting standards that every contributor must follow when adding or modifying components, styles, tests, or public API surface. It is the public counterpart to the agent-only deep references under `.exclude/agents/` (which must never be referenced from committed files).

---

## Table of Contents

1. [Component Architecture](#component-architecture)
   - [Base Classes & Pipeline](#base-classes--pipeline)
   - [Interface-Driven Styling (`IHas*`)](#interface-driven-styling-ihas)
   - [State Axes: `X` vs `IsX`](#state-axes-x-vs-isx)
   - [`IBuiltComponent` Hooks](#ibuiltcomponent-hooks)
     - [`IPureBuiltComponent` — cache-friendly variant](#ipurebuiltcomponent--cache-friendly-variant)
   - [JS Behavior Lifecycle](#js-behavior-lifecycle)
   - [Variants](#variants)
2. [CSS Architecture](#css-architecture)
   - [Two-Tier System](#two-tier-system)
   - [`data-bob-*` Attributes](#data-bob--attributes)
   - [Private-Variable Pattern](#private-variable-pattern)
   - [Sizing & Density](#sizing--density)
   - [BEM Conventions](#bem-conventions)
3. [Async / JS Interop](#async--js-interop)
   - [Behavior Module Pattern](#behavior-module-pattern)
4. [Testing](#testing)
   - [Stack & Layout](#stack--layout)
   - [Per-Component File Matrix](#per-component-file-matrix)
   - [Assertion Rules](#assertion-rules)
   - [Snapshot Scrubbing](#snapshot-scrubbing)
5. [Public API Tracking](#public-api-tracking)
6. [Build Pipeline](#build-pipeline)
7. [Mechanized Lints & Audits](#mechanized-lints--audits)
8. [Documented Exceptions](#documented-exceptions)

---

## Component Architecture

### Base Classes & Pipeline

All library components derive from one of three bases under `BlazOrbit.Core/Components`:

| Base | Use When |
|---|---|
| `BOBComponentBase` | General components (cards, dialogs, toasts, buttons …) |
| `BOBInputComponentBase<TValue>` | Form-bound inputs (text, checkbox, dropdown …) |
| `BOBVariantComponentBase<TComponent, TVariant>` / `BOBInputComponentBase<TValue, TComponent, TVariant>` | Components with registered variant templates |

`BOBInputComponentBase<TValue>` cannot inherit `BOBComponentBase` because it must derive from Blazor's `InputBase<TValue>` to participate in `EditContext`. The shared lifecycle logic lives in **`BOBComponentPipeline`** (composed into both bases). **When extending the lifecycle, edit the pipeline once** — not the two bases independently.

#### Lifecycle Hooks

| Hook | Pipeline Action |
|---|---|
| `OnInitialized` | `BeginInit()` — DEBUG stopwatch only |
| `OnParametersSet` | `BeginParametersSet()` → `base` → `BuildStyles(this, AdditionalAttributes)` (see ordering note below) |
| `BuildRenderTree` | `BeginRenderTree()` → `PatchVolatileAttributes(this)` → `base` |
| `OnAfterRenderAsync(firstRender)` | `AttachBehaviorAsync(this, BehaviorJsInterop)` |
| `DisposeAsync` | `IsDisposed = true` → `DisposeBehaviorAsync()` |

> **Ordering note — `OnParametersSet`**:
> - `BOBComponentBase` calls `base.OnParametersSet()` **before** `BuildStyles`.
> - `BOBInputComponentBase<TValue>` calls `BuildStyles` **first**, then performs `EditContext` subscription / validation-state bookkeeping, and finally calls `base.OnParametersSet()`.
>
> The reversal in the input base is deliberate: it needs the style pipeline ready before `InputBase<TValue>.OnParametersSet()` fires, and `HandleValidationStateChanged` calls `BuildStyles` manually outside the standard parameter-change flow.

### Interface-Driven Styling (`IHas*`)

Style is **interface-driven, not class-driven**. A component advertises capabilities by implementing marker/property interfaces under `BlazOrbit.Core/Abstractions/Behaviors/`. At render time `BOBComponentAttributesBuilder` reflects over the component, builds a `ComponentFeatures` bitmask, and emits:

- **`data-bob-*` attributes** on the `<bob-component>` root (state, family, variant, size, density, …)
- **`--bob-inline-*` CSS custom properties** collapsed into a single `style="…"` string (color, background, border, ripple, transitions, …)

The component only declares the interface and its `[Parameter]`; the builder does the rest.

> See ADR-0002 for the original decision and the motivation behind interface-driven styling.

#### Key Interface Groups

| Group | Examples | What Gets Emitted |
|---|---|---|
| **State** | `IHasDisabled`, `IHasError`, `IHasReadOnly`, `IHasRequired`, `IHasActive`, `IHasLoading` | `data-bob-disabled`, `data-bob-error`, … |
| **Design** | `IHasSize`, `IHasDensity`, `IHasFullWidth`, `IHasColor`, `IHasBackgroundColor`, `IHasBorder`, `IHasShadow`, `IHasElevation`, `IHasPrefix`, `IHasSuffix` | `data-bob-size`, `--bob-inline-color`, `--bob-inline-border`, … |
| **Transitions** | `IHasTransitions` | `data-bob-transitions`, transition CSS variables |
| **JavaScript** | `IHasRipple` (`: IJsBehavior`) | `--bob-inline-ripple-color`, `--bob-inline-ripple-duration` |
| **Families** | `IInputFamilyComponent`, `IPickerFamilyComponent`, `IDataCollectionFamilyComponent` | `data-bob-input-base`, `data-bob-picker-base`, `data-bob-data-collection` |
| **Variants** | `IVariantComponent` / `IVariantComponent<TVariant>` | `data-bob-variant=<name-lower>` |

#### Family Marker Pattern

A family marker (`data-bob-input-base`, `data-bob-picker-base`, `data-bob-data-collection`) is the bridge between an `IXxxFamilyComponent` interface and the corresponding global generator (`InputFamilyCssGenerator`, `PickerFamilyGenerator`, `DataCollectionFamilyCssGenerator`). The generator emits its rules under `bob-component[data-bob-<family>-base]`; any component implementing the interface inherits the family CSS without further wiring. To add a new family:

1. Define `IXxxFamilyComponent` interface (no members; pure marker).
2. Add `XxxFamily` flag to `ComponentFeatures`.
3. Reflect over the interface in `ComputeTypeInfo`.
4. Emit `data-bob-<family>-base` in `BuildStyles`.
5. Create the generator that selects on `bob-component[data-bob-<family>-base]`.

#### Volatile vs. Stable Axes

An axis is **volatile** when its value can change *between renders without flowing through `OnParametersSet`* — typically because it reflects internal component state (timers, counters, focus, JS-driven flags) rather than caller-supplied parameters. The current `VolatileMask` covers six high-frequency state attributes: `Active`, `Disabled`, `Loading`, `Error`, `ReadOnly`, `Required`. Volatile axes are re-emitted in `PatchVolatileAttributes` on every `BuildRenderTree` and are excluded from the style fingerprint cache (otherwise the cache would freeze the stale value).

`FullWidth` (`IHasFullWidth`) is a stable design parameter, not a volatile axis. It is only supplied by the parent and is already folded into `ComputeStyleFingerprint`, so a change triggers a full `BuildStyles` rebuild via `OnParametersSet`.

If your new axis reads a `[Parameter]` only — and that parameter is the sole source of truth — it is **stable**: fold it into `ComputeStyleFingerprint` and emit once in `BuildStyles`. If the value can shift mid-render or is computed from non-parameter state, mark it volatile.

> See ADR-0003 for the performance context and the original split between fingerprint cache and volatile patching.

#### Adding a New Axis

1. Create `IHasXxx.cs` in the appropriate `Behaviors/` sub-folder.
2. Add a flag to `ComponentFeatures` in `BOBComponentAttributesBuilder`.
3. Add reflection in `ComputeTypeInfo`.
4. Add folding in `ComputeStyleFingerprint` (if it affects style and is not volatile).
5. Add emit in `BuildStyles` and, if volatile, in `PatchVolatileAttributes` + `VolatileMask`.
6. Add constants to `FeatureDefinitions.DataAttributes` and/or `FeatureDefinitions.InlineVariables`.
7. Add a unit test in `BOBComponentAttributesBuilderUnitTests`.
8. If the attribute name or value can match `b-[a-z0-9]{10}` (Razor SDK isolation scope) or includes a GUID-suffixed component-specific prefix, extend `BuiGeneratedIdRegex` / `CssIsolationScopeRegex` in `test/BlazOrbit.Tests.Integration/Infrastructure/VerifyConfig.cs` so snapshots scrub it deterministically.
9. Let the Public API analyzer update `PublicAPI.Unshipped.txt`.

### State Axes: `X` vs `IsX`

Six state interfaces expose the `[Parameter] bool X { get; set; }` pattern. Five of them also expose a computed `bool IsX { get; }` property:

- `[Parameter] bool X { get; set; }` — caller override.
- `bool IsX { get; }` — **computed truth** (where present).

Contract: `IsX = X || <internal>`. The caller override **cannot negate** an internal condition; it can only force it to `true`.

| Interface | Parameter | Computed (`IsX`) | Internal Factor |
|---|---|---|---|
| `IHasDisabled` | `Disabled` | `IsDisabled` | `Disabled \|\| Loading` |
| `IHasError` | `Error` | `IsError` | `Error \|\| EditContext validation messages` |
| `IHasReadOnly` | `ReadOnly` | `IsReadOnly` | passthrough (reserved for evolution) |
| `IHasRequired` | `Required` | `IsRequired` | passthrough |
| `IHasActive` | `Active` | `IsActive` | component-specific (e.g. tab selected, toast open) |
| `IHasLoading` | `Loading` | — | externally driven; feeds into `IsDisabled` |

`IHasLoading` is special: it does not define its own `IsLoading` because `Loading` is set from the outside and its only internal effect is to drive `IsDisabled` (see `BOBInputComponentBase.IsDisabled`). Components that implement `IHasLoading` show a loading indicator when the parameter is `true`.

**Always read `IsX` in render decisions, event handlers, and tests.** Never read the raw parameter — it silently breaks layered conditions such as *loading implies disabled*.

### `IBuiltComponent` Hooks

Components that need custom `data-bob-*` or `--bob-inline-*` beyond the standard axes implement `IBuiltComponent`:

```csharp
@implements IBuiltComponent

public void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
{
    if (IsFloated)
        dataAttributes[FeatureDefinitions.DataAttributes.Floated] = "true";
    else
        dataAttributes.Remove(FeatureDefinitions.DataAttributes.Floated);
}

public void BuildComponentCssVariables(Dictionary<string, string> cssVariables) { }
```

Rules:
- Implement **both** methods. If only one is needed, the other is a no-op.
- Component-supplied keys are written **first**; framework-owned keys are written **after** and win on collision.
- Implementing `IBuiltComponent` **opts the component out of the fingerprint cache** because the hooks may read opaque state (timers, counters, etc.).

#### `IPureBuiltComponent` — cache-friendly variant

When the hooks read **only** `[Parameter]` properties or computed `IsX` getters (no internal fields like `_isFocused`, no service state), declare `IPureBuiltComponent` instead of `IBuiltComponent`. The marker promises the hooks are a pure function of parameters; the builder folds the hook output into the style fingerprint so repeated `BuildStyles` calls with identical parameters short-circuit.

```csharp
@implements IPureBuiltComponent

public void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes) { }

public void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
{
    if (Gap != null)
        cssVariables["--_gap"] = Gap;
}
```

`IPureBuiltComponent : IBuiltComponent`, so the hook signatures are the same. Switching back to plain `IBuiltComponent` re-opts out of the cache — do that the moment a hook starts touching internal state. The audit `Pure_Built_Components_Should_Not_Read_Instance_Fields_In_Hooks` (CACHE-PURE-01) catches the most common drift mode (a `_x` field reference inside a hook), but any non-parameter source — a service, a counter, a captured event arg — will silently freeze the cached value if you forget.

### JS Behavior Lifecycle

`OnAfterRenderAsync(firstRender: true)` attaches the JS behavior via `BOBComponentJsBehaviorBuilder`. `DisposeAsync` disposes it with the canonical **4-tuple catch**:

```csharp
catch (JSDisconnectedException) { }
catch (ObjectDisposedException) { }
catch (InvalidOperationException) { }
catch (TaskCanceledException) { }
```

These map to non-actionable teardown paths (circuit gone, runtime disposed, prerender without circuit, await cancelled).

**Ordering constraint**: `ObjectDisposedException` derives from `InvalidOperationException`, so it must precede `InvalidOperationException` in the catch chain — otherwise the inherited type swallows the disposed exception and the `catch (ObjectDisposedException)` arm is unreachable. The other three may appear in any relative order. The example above (JSDisconnected → ObjectDisposed → InvalidOperation → TaskCanceled) is the conventional shape; `BOBModalHost` puts `ObjectDisposedException` first because that path disposes a JS module reference whose disposed state is the dominant teardown signal — both orders are valid as long as the inheritance constraint holds.

**`IsDisposed` guard**: both bases expose `protected bool IsDisposed { get; }`. Any post-`await` continuation that touches component state must gate on it:

```csharp
await SomethingAsync();
if (IsDisposed) return;
StateHasChanged();
```

### Variants

Variants are registered at startup:

```csharp
builder.Services.AddBlazOrbitVariants(b =>
    b.ForComponent<UIButton>().AddVariant(MyVariant.Special, comp => @<...>));
```

Built-in variants live in `BuiltInTemplates` on the component class. The variant helper resolves: registry override → built-in → null (component's own render only).

---

## CSS Architecture

### Two-Tier System

Two layers ship with the library:

1. **Global bundle** (generated) — `src/BlazOrbit.BuildTools/Generators/` write into `src/BlazOrbit/CssBundle/`; Vite bundles them into `wwwroot/css/blazorbit.css`. Edit the generator, never the generated `.css`.
2. **Scoped component CSS** (hand-written `.razor.css` next to the `.razor`) — scoped per-component by Blazor CSS isolation.

The bundle's `@import` chain is fixed in `BuildTemplates.GetMainCssTemplate()`:

```
1. Reset & Base       → _reset.css, _typography.css
2. Theme Variables    → _themes.css, _initialize-themes.css
3. Universal styles   → _tokens.css, _base.css, _scrollbar.css, _transition-classes.css
4. Family-based       → _input-family.css, _picker-family.css, _data-collection-family.css
```

Order matters — later layers depend on tokens / variables emitted by earlier ones. When adding a new generator, place its `@import` in the correct phase and update `BuildTemplates.GetMainCssTemplate()` in the same change.

> See ADR-0004 for the trade-off analysis that led to the global + scoped CSS split.

### `data-bob-*` Attributes

**Runtime state on the root `<bob-component>` is expressed exclusively via `data-bob-*` attributes**, never CSS class modifiers.

The full inventory lives in `FeatureDefinitions.DataAttributes` (`src/BlazOrbit.Core/Css/FeatureDefinitions.cs`). Below is the grouping that drives style selectors today; add a row whenever a new constant lands.

| Group | Attributes | Source |
|---|---|---|
| **Identity** | `data-bob-component` | `BOBComponentAttributesBuilder` (always emitted) |
| **Family marker** | `data-bob-input-base`, `data-bob-picker-base`, `data-bob-data-collection` | `IInputFamilyComponent`, `IPickerFamilyComponent`, `IDataCollectionFamilyComponent` |
| **State** | `data-bob-disabled`, `data-bob-loading`, `data-bob-error`, `data-bob-active`, `data-bob-readonly`, `data-bob-required` | `IHasDisabled`, `IHasLoading`, `IHasError`, `IHasActive`, `IHasReadOnly`, `IHasRequired` |
| **Design** | `data-bob-size`, `data-bob-density`, `data-bob-fullwidth`, `data-bob-shadow`, `data-bob-elevation`, `data-bob-variant`, `data-bob-floated` | `IHasSize`, `IHasDensity`, `IHasFullWidth`, `IHasShadow`, `IHasElevation`, `IVariantComponent`, input floated-label state |
| **Transitions** | `data-bob-transitions`, `data-bob-transitioning` | `IHasTransitions`; mid-flight self-managed transitions |
| **Theme** | `data-bob-theme` (on `<html>`) | `BOBInitializer` / `ThemeInterop` |
| **Component-specific** | `data-bob-button-placement`, `data-bob-resize`, `data-bob-autoresize`, `data-bob-dot`, `data-bob-circular`, `data-bob-position`, `data-bob-hoverable`, `data-bob-row-pattern`, `data-bob-checked`, `data-bob-indeterminate`, `data-bob-selection-mode`, `data-bob-orientation`, `data-bob-trigger`, `data-bob-expand-mode`, `data-bob-closing`, `data-bob-paused`, `data-bob-animation`, `data-bob-muted` | Set by individual components via `IBuiltComponent.BuildComponentDataAttributes` or directly in their `.razor` |
| **Opt-in styling** | `data-bob-scrollbars` | Consumer-applied marker for branded global scrollbars |

State / Design / Transitions / Family markers are emitted by the framework (`BOBComponentAttributesBuilder`) when the corresponding `IHas*` interface is implemented — components do not write them by hand. Component-specific attributes are emitted directly by the component (typically via `IBuiltComponent`) and do not flow through the standard pipeline.

**Boolean axes use presence-as-truth**: when the value is `true`, the attribute is `="true"`; when `false`, the attribute is **absent** rather than `="false"`. CSS selectors must match `[data-bob-x="true"]`, never `[data-bob-x="false"]`.

> See ADR-0001 for the historical decision to move runtime state from BEM modifiers to `data-bob-*` attributes, and ADR-0005 for the boolean omission convention.

**Scoped CSS selector form** (inside `.razor.css`):

```css
[data-bob-component="button"] { … }
[data-bob-component="button"][data-bob-disabled="true"] button { … }
```

Three shapes appear across the codebase, each with a clear domicile:

1. **Inside scoped `.razor.css`** — prefer the qualified form `[data-bob-component="<name>"]` (or `bob-component[data-bob-component="<name>"]`) as a self-documenting prefix. Blazor CSS isolation appends a `[b-xxx]` attribute automatically, which is what actually scopes the rule to the file — the qualifier is for grepability when reading a rule out of context, not for correctness. Bare `bob-component { … }` is reserved for declarations targeting the host root tag (root-level layout / size / box-model).
2. **Inside global `CssBundle/*.css` and generators** — must qualify, since there is no isolation attribute to fall back on. Use:
   - `bob-component[data-bob-component="<name>"]` for one specific component, or
   - `bob-component[data-bob-<family>-base]` (e.g. `data-bob-input-base`, `data-bob-picker-base`) for a whole family, or
   - bare `bob-component { … }` only for cross-component baseline rules in `_base.css` / `_typography.css` where the rule is *meant* to match every component.
3. **Avoid** bare `bob-component` inside scoped files for descendant selectors — it works, but the qualified form keeps every scoped rule self-identifying.

### Private-Variable Pattern

At the root, declare private vars that resolve inline overrides with a fallback:

```css
[data-bob-component="button"] {
    --_button-background: var(--bob-inline-background, var(--palette-primary));
    --_button-color: var(--bob-inline-color, var(--palette-primary-contrast));
}

[data-bob-component="button"] button {
    background-color: var(--_button-background);
    color: var(--_button-color);
}
```

This creates **one predictable override surface**: consumers (or the builder) set `--bob-inline-*`; the component references only `--_<component>-*`.

### Sizing & Density

Sizing uses multipliers, not breakpoints:

```css
min-height: calc(2.5rem * var(--bob-size-multiplier, 1));
gap: calc(0.5rem * var(--bob-density-multiplier));
```

`_base.css` maps `[data-bob-size]` and `[data-bob-density]` once globally.  
**Exception**: layout components (`BOBGrid`, `BOBGridItem`, `BOBSidebarLayout`, `BOBStackedLayout`, `BOBToastHost`) may use `@media` because they change *flow*, not dimensions.

### BEM Conventions

- **Root**: `<bob-component>` with `@attributes="ComputedAttributes"`.
- **Children**: `bob-<component>__<element>[--<modifier>]`. Reuse family class names (e.g. `bob-input__field`) when applicable.
- **BEM modifiers on children are allowed** (e.g. `bob-button__icon--leading`, `bob-picker__cell--selected`).
- **BEM modifiers on the root are forbidden**. Root state must be `data-bob-*`.

---

## Async / JS Interop

- **No `ConfigureAwait(false)`** in library code. Blazor Server's renderer relies on its synchronization context.
- **4-tuple catch** on all teardown-path interop calls: `JSDisconnectedException`, `ObjectDisposedException`, `InvalidOperationException`, `TaskCanceledException`. The only ordering constraint is that `ObjectDisposedException` must precede `InvalidOperationException` (it derives from it); the other three may appear in any relative order. See the JS Behavior Lifecycle section for the conventional shape.
- **Module load** (when a page imports an enhancement script) catches the 4-tuple **plus** `JSException` (404, parse error, module-side throw). Components degrade gracefully.
- **Fire-and-forget**: never `_ = FooAsync()`. Use `BOBAsyncHelper.SafeFireAndForget(() => FooAsync(), logger)` only when the caller cannot become `async Task` (timer scheduling, sync event handlers, sync lifecycle overrides).

### Behavior Module Pattern

JavaScript-backed enhancements ship as TypeScript modules under `src/BlazOrbit/Types/<Feature>/<Feature>Interop.ts`, bundled by Vite into `wwwroot/js/Types/<Feature>/<Feature>Interop.min.js`. Conventions:

- **Source location**: one folder per feature under `Types/`. Filename is always `<Feature>Interop.ts`.
- **Reference constant**: every module gets a `public const string` in `BlazOrbit.Types.JSModulesReference` pointing to the `_content/BlazOrbit/js/Types/<Feature>/<Feature>Interop.min.js` static asset.
- **Component entry**: components import the module via `IJSObjectReference module = await JS.InvokeAsync<IJSObjectReference>("import", JSModulesReference.<Name>)` inside `OnAfterRenderAsync(firstRender: true)`, guarded by the 5-tuple catch (4-tuple + `JSException`).
- **Disposal**: keep the `IJSObjectReference` in a field; dispose it in `DisposeAsync` with the 4-tuple catch.
- **No globals**: modules export functions; components never assume window-level state.

---

## Testing

### Stack & Layout

| Package | Version | Role |
|---|---|---|
| `bunit` | 2.7.2 | Razor renderer, `BunitContext`, `IRenderedComponent<T>` |
| `xunit.v3` | 3.2.2 | Test runner |
| `FluentAssertions` | 8.9.0 | Fluid assertion API |
| `Verify` + `Verify.Blazor` + `Verify.DiffPlex` | 31.16.2 / 11.0.0 / 3.1.2 | Snapshot tests |
| `NSubstitute` | 5.3.0 | Stubs for JS/services |

### Per-Component File Matrix

One folder per component at `Tests/Components/<ComponentName>/`:

| Context | When Required |
|---|---|
| `<C>RenderingTests` | Always |
| `<C>SnapshotTests` | Always |
| `<C>StateTests` | If the component exposes state axes (`Disabled`, `Loading`, `Error`, …) |
| `<C>InteractionTests` | If there are clicks, keyboard, or focus handlers |
| `<C>VariantTests` | If the component uses `IVariantRegistry` or has a `Variant` parameter |
| `<C>AccessibilityTests` | If WCAG 2.2 AA applies (roles, `aria-*`, tab order, contrast) |
| `<C>ValidationTests` | If the component is form-bound (`EditContext`) |
| `<C>IntegrationTests` | Optional — for multi-component compositions |

**Class naming**: `<Component><Context>Tests`  
**Trait**: `[Trait("Component <Context>", "<ComponentName>")]`  
**Method naming**: `Should_<ExpectedBehavior>[_When_<Condition>]`

**Hosting-model coverage**: any test that renders a component must use:

```csharp
[Theory]
[MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
```

This runs the test under both Server and Wasm scenarios. Reserve `[Fact]` for pure C# tests that do not create a `BunitContext`.

### Assertion Rules

- **State**: assert against `data-bob-*` on the root `bob-component`. Never CSS classes for state.
- **Inline style**: assert against `style` containing `--bob-inline-*`. Do not assert against private `--_<component>-*` vars.
- **Structure**: BEM classes on children (`bob-card__header`, `bob-button__icon--leading`) are fine.
- **Volatile-attribute drill**: capture the same `IElement` before and after `cut.Render(...)` to exercise `PatchVolatileAttributes`.

Example:

```csharp
IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p.Add(c => c.Disabled, false));
IElement root = cut.Find("bob-component");
root.GetAttribute("data-bob-disabled").Should().BeNull();

cut.Render(p => p.Add(c => c.Disabled, true));
root.GetAttribute("data-bob-disabled").Should().Be("true");
```

**Snapshots**: never commit `*.received.txt`. Review, then rename to `*.verified.txt` to accept.

### Snapshot Scrubbing

`test/BlazOrbit.Tests.Integration/Infrastructure/VerifyConfig.cs` strips non-deterministic tokens before snapshot comparison:

- `ElementReferenceRegex` — `_bl_<guid>` references emitted by `@ref`.
- `OnClickRegex` — Blazor's per-render `blazor:onclick="N"` event-id counter.
- `BuiGeneratedIdRegex` — component-local GUID-suffixed ids (`bob-input-<32 hex>`, `bob-helper-…`, etc.). **Extend the regex's alternation when a new component emits a GUID-suffixed id with a fresh prefix** — it does not auto-detect.
- `PatternIdRegex`, `DropdownIdRegex`, `DialogTitleIdRegex` — feature-specific id formats.
- `CssIsolationScopeRegex` — Razor SDK's `b-[a-z0-9]{10}` per-file scope marker.

**Known footgun**: `CssIsolationScopeRegex` matches *any* `b-` followed by ten alphanum chars, including kebab-cased data attributes whose value happens to be ten characters (`b-transition`, `b-comfortabl`, `b-component`). When introducing a `data-bob-<x>` attribute whose value-or-token name is exactly ten chars, expect spurious snapshot diffs and either rename or tighten the regex.

---

## Public API Tracking

Every publishable project enables `Microsoft.CodeAnalysis.PublicApiAnalyzers` with two sidecar files:

- `PublicAPI.Shipped.txt` — surface already released to NuGet. Do not hand-edit mid-release.
- `PublicAPI.Unshipped.txt` — additions/removals staged for the next release.

Workflow on public-surface change: build → analyzer raises `RS0016` (missing) / `RS0017` (stale) → apply the Roslyn code-fix → review `PublicAPI.Unshipped.txt` as the contract diff.

Rules:
- Types that must stay `public` for Razor SDK / reflection / DI but should not appear in IntelliSense get `[EditorBrowsable(EditorBrowsableState.Never)]`.
- Nested `public` types inside an `internal` parent still count as surface — collapse to `internal`.
- `InternalsVisibleTo` does **not** affect the analyzer.

#### Renaming or moving public surface

Treat a rename as `delete + add` in `PublicAPI.Unshipped.txt`:

```
~old.namespace.OldName.Method() -> void   ← removal (note the leading `~`)
new.namespace.NewName.Method() -> void    ← addition
```

The `~` prefix marks the entry as a deletion against `PublicAPI.Shipped.txt`. Reviewers read the unshipped diff as the literal contract change for the next release; never edit `PublicAPI.Shipped.txt` mid-release.

#### Razor `BuildRenderTree` and RS0041

The Razor SDK generates an `override` of `BuildRenderTree` whose signature includes a Roslyn-nullable warning suppressed by `RS0041`. Because the override sits in a generated partial, the analyzer flags it on every build unless **both** of these are present:

- An entry in `PublicAPI.Unshipped.txt` of the form `~override <component>.BuildRenderTree(…)` (the deletion-style entry is what RS0041 expects for compiler-generated overrides).
- A matching `[SuppressMessage("ApiDesign", "RS0041:…")]` attribute in `GlobalSuppressions.cs` for the same symbol.

The maintainer runs an autogen project that reconciles these two artifacts; do not hand-edit either file expecting them to stay in sync. If you add a new public component, build once, then run the autogen to refresh the entries.

---

## Build Pipeline

CI build order: `CodeGeneration` → `Core` → `Main` → `BuildTools`. Mirror this when building incrementally if analyzers/generators behave oddly.

Generated assets (do not hand-edit):

- `src/BlazOrbit/CssBundle/*.css`
- `src/BlazOrbit/wwwroot/css/*`
- `src/BlazOrbit/wwwroot/js/*`
- `src/BlazOrbit/package.json`, `tsconfig.json`, `vite.config*.js`, `.npmrc`

Edit the corresponding `[AssetGenerator]` or `[BuildTemplate]` under `src/BlazOrbit.BuildTools/` instead.

### `[AssetGenerator]` vs `[BuildTemplate]`

| Attribute | Use For | Mechanism |
|---|---|---|
| `[AssetGenerator]` | Files whose contents are **computed** from the source tree (themes from palettes, family CSS from interface registrations, design tokens from constants) | Class implements `IAssetGenerator`; the build invokes `Generate()` to produce the file |
| `[BuildTemplate("path")]` | Files with **static** content the project needs verbatim (`entry.js`, `main.css`, `.npmrc`, `tsconfig.json`) | Static method returns a string literal; the build writes the literal to `path` |

Rule of thumb: if the output depends on anything the maintainer might add later (a new theme, a new family, a new component), use `[AssetGenerator]`. If the output is fixed boilerplate, use `[BuildTemplate]`.

---

## Mechanized Lints & Audits

Most rules in this document are enforced by tests under `test/BlazOrbit.Tests.Integration/Tests/`. New rules should ship with a corresponding test; failing builds beat doc drift.

| Test class | Rule(s) enforced |
|---|---|
| `Library/ArchitectureAuditTests` | ASYNC-01 (no `_ = AsyncCall`), ASYNC-02 (no `ConfigureAwait`), ASYNC-03 (4-tuple catch consistency), TEST-TRAIT-01 (`Component <Context>` traits), CSS-MEDIA-01 (`@media` only in layout exceptions), CACHE-PURE-01 (`IPureBuiltComponent` hooks must not read instance fields), INPUT-BG-01, INPUT-COLOR-01 (input-family inline CSS consumption) |
| `Library/ComponentMarkupLintTests` | No native `onX="…"` inline handlers, no `<script>` tags, no hardcoded `style=""` in `.razor` |
| `Library/ComponentParameterAuditTests` | PascalCase attributes passed to BlazOrbit components must match a declared `[Parameter]` |
| `Library/ComponentRootContractLintTests` | Components deriving from the `BOBComponentBase` family must root in `<bob-component>` |
| `Library/CssAuditTests` | DOM-emitted `data-bob-*` ↔ CSS selector ↔ TS consumer parity; every `FeatureDefinitions.InlineVariables` referenced in CSS |
| `Library/CssClassAuditTests` | No CSS classes applied in markup without a matching declaration |
| `Library/CssScopedAuditTests` | Every public component ships a `.razor.css` (CSS-SCOPED-09) unless allowlisted |
| `Library/CssScopedSelectorAuditTests` | No dead selectors in scoped CSS files |
| `Library/CssVarDeclarationAuditTests` | `var(--name)` references in scoped CSS must resolve to a declared variable |
| `Library/ScopedCssLintTests` | Residual scoped-CSS hygiene rules not covered above |
| `Core/CssArchitectureLintTests` | CSS-SCOPED-LINT-01 (no BEM modifiers on root), A11Y-02 (`prefers-reduced-motion` override in `_base.css`) |

Allowlists in these tests freeze a controlled exception. Each entry must include a one-line justification; remove it once the underlying issue is fixed.

---

## Documented Exceptions

The following exceptions to the rules above are deliberate and recorded:

| Rule Relaxed | Where | Why |
|---|---|---|
| All components derive from `BOBComponentBase` | `BOBInputDropdown` is a `ComponentBase` and composes an internal `BOBDropdownContainer : BOBInputComponentBase` | Trigger + menu have different rendering needs. The container forwards `Value`/`ValueChanged`/`ValueExpression` and `IHas*` state. |
| State via `data-bob-*`, never CSS classes | BEM modifiers on **child elements** (`bob-x__item--selected`, `--focused`, `--open`) | Children do not own `IHas*` state. A parallel reflection pass would be over-engineering. Guarded by `CssArchitectureLintTests.RazorCss_Should_Not_Use_Bem_State_Modifiers_On_Root`. |
| Sizing via multiplier, not breakpoints | Layout components (`BOBGrid`, `BOBGridItem`, `BOBSidebarLayout`, `BOBStackedLayout`, `BOBToastHost`) | Layout changes *flow* (grid columns, drawer activation), which is a categorical viewport switch, not a scalable dimension. |

If you believe a new exception is warranted, add it here with the rule it relaxes, the file(s) involved, and the rationale.
