# Contributing to BlazOrbit

Thank you for considering a contribution. This document covers everything you need to get started: local setup, architecture conventions, commit style, and the pull-request workflow.

---

## [Code of Conduct](CODE_OF_CONDUCT.md)

Be civil and constructive. Disagreements about architecture are normal; personal attacks are not. Maintainers reserve the right to close threads that derail.

---

## Local Environment

### Prerequisites

- **.NET SDK** — The repository pins the SDK version in `global.json`. Run `dotnet --version` and ensure it matches the required version (e.g., `10.0.203`).
- **Node.js + npm** — Required because the build regenerates CSS bundles and minifies TypeScript via Vite. The build will locate `node.exe` and `npx.cmd` automatically.
- **PowerShell 7**  — The helper scripts under `scripts/` are written in PowerShell.

### Build & Test

```bash
# Restore and build the whole solution
dotnet restore
dotnet build BlazOrbit.slnx -c Debug

# Run all tests
dotnet test

# Run a single test project or filter
dotnet test test/BlazOrbit.Tests.Integration/BlazOrbit.Tests.Integration.csproj --filter "DisplayName~Button"
```

### Generated Assets

The following files and directories are **auto-generated at build time**. Do not edit them on disk:

- `src/BlazOrbit/CssBundle/*.css`
- `src/BlazOrbit/wwwroot/css/*`
- `src/BlazOrbit/wwwroot/js/*`
- `src/BlazOrbit/package.json`
- `src/BlazOrbit/tsconfig.json`
- `src/BlazOrbit/vite.config*.js`
- `src/BlazOrbit/.npmrc`

If you need to change generated output, edit the corresponding generator or template under `src/BlazOrbit.BuildTools/` (see the `Generators/` and `Infrastructure/BuildTemplates.cs` files).

`dotnet clean` deletes all generated assets; they are recreated on the next build.

---

## How to Report Bugs

Use the **Bug report** issue template under `.github/ISSUE_TEMPLATE/`.

A good bug report includes:

1. **Steps to reproduce** — numbered, minimal, and deterministic.
2. **Expected behavior** vs. **actual behavior**.
3. **Environment** — .NET SDK version, browser (if UI-related), and OS.
4. **Logs or exceptions** — stack traces, console output, or screenshots.
5. **Minimal reproduction** — a small Razor snippet or a stripped-down repo.

Search closed issues first. If you cannot find a duplicate, fill in the template completely or ask if you don't know how to fill some field.

---

## Branching & Workflow

`develop` is the integration branch. All work happens on short-lived branches that target `develop` via Pull Request.

| Prefix | When to use | Example |
|---|---|---|
| `feature/` | New component, behavior, or public API | `feature/datagrid-virtualization` |
| `fix/` | Bug fix or behavior correction | `fix/dropdown-double-dispose` |
| `chore/` | Dependency bumps, scripts, infrastructure, docs | `chore/bump-aspnetcore-10.0.7` |
| `docs/` | Documentation-only changes | `docs/api-samples` |
| `refactor/` | Internal restructuring with no observable change | `refactor/component-pipeline` |
| `test/` | Test-only additions | `test/dropdown-disposal` |

Aim to keep a branch alive for **1–3 days**. Long-lived branches drift from `develop` and create painful rebases.

### Helper Scripts

The `scripts/dev-tools.ps1` script wraps common operations:

```powershell
./scripts/dev-tools.ps1 sync                    # Update develop from origin
./scripts/dev-tools.ps1 feature feature-name    # Create feature branch
./scripts/dev-tools.ps1 fix fix-name            # Create fix branch
./scripts/dev-tools.ps1 commit "message"        # Commit with conventional commits
./scripts/dev-tools.ps1 squash                  # Squash all commits into one
./scripts/dev-tools.ps1 ready                   # Prepare PR: squash + rebase + push
./scripts/dev-tools.ps1 fix-conflict            # After resolving conflicts
./scripts/dev-tools.ps1 push                    # Safe push (force-with-lease)
./scripts/dev-tools.ps1 pr "Title" "Desc"       # Open PR page on GitHub
./scripts/dev-tools.ps1 cleanup                 # Clean merged branches
./scripts/dev-tools.ps1 status                  # Show repository status
```

---

## Commits

We use [Conventional Commits](https://www.conventionalcommits.org/) so the changelog can be inferred automatically:

```
<type>(<scope>): <description>

<optional body>

<optional footer, e.g. Fixes #123>
```

Allowed types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `perf`, `breaking`.

`<scope>` is a short component or area name (e.g., `dropdown`, `theme`, `button`).

---

## Pull Request Process

**Strategy: each PR = one squash commit on `develop`.** Keep your local history readable while you work, then squash before opening the PR (preferible). The `dev-tools.ps1 ready` command runs `squash + rebase + push` in one step.

Before requesting review, run:

```bash
dotnet build BlazOrbit.slnx -c Release
dotnet test
```

Both must be green. The PR is also blocked on:

- **CI gates** (`preview-gate.yml`): build, tests, public API diff.
- **At least one approving review** from a maintainer.
- **`PublicAPI.Unshipped.txt` updated** if you changed the public surface (the analyzer's code-fix does this for you — just apply the IDE suggestion).

Open the PR against `develop`. CI packs and surfaces `.nupkg` artifacts so reviewers can test-install.

---

## Component Architecture

All library components derive from `BOBComponentBase`, `BOBVariantComponentBase` or `BOBInputComponentBase<TValue>`. Unless they are explicitly excluded because they are internal components or there are valid reasons. In general do not inherit directly from `ComponentBase` and use the `<bob-component>` DOM rendering standards.

### Styling Pipeline

Styling is driven by marker/property interfaces (`IHas*`) along with date attributes rather than per-feature CSS class toggles:

- `BOBComponentAttributesBuilder` reflects over the component at render time to emit `data-bob-*` attributes and `--bob-inline-*` CSS custom properties on the root element.
- CSS selectors target `[data-bob-*]` consistently. Do not add component-specific CSS classes to express state that already has a data-attribute.
- Components implement `BuildComponentDataAttributes` and `BuildComponentCssVariables` (virtuals on `BOBComponentBase`) to contribute extra attributes/variables. These run during `OnParametersSet`, not inside `BuildRenderTree`.

### DOM Rules

1. **Root element**: render a `<bob-component>` custom tag as the outermost element, with `@attributes="ComputedAttributes"` spread onto it.
2. **Selector form**: inside scoped `.razor.css`, target the component with `[data-bob-component="<kebab-name>"]` (e.g., `button` for `BOBButton`). Blazor CSS isolation appends a scope attribute automatically.
3. **BEM inside the component**: children use `bob-<component>__<element>` / `bob-<component>__<element>--<modifier>` classes. Reuse family class names (e.g., `bob-input__field`) when a component belongs to a family.
4. **Private-var pattern**: at the component root, declare `--_<component>-<property>` custom properties that resolve `var(--bob-inline-*, <default>)`. Reference only the `--_<component>-*` vars in actual declarations.

### State Parameters: `[Parameter] X` vs `IsX`

State axes (`Disabled`, `Error`, `ReadOnly`, `Required`, `Active`) expose two members:

- `[Parameter] public bool X { get; set; }` — external override.
- `public bool IsX { get; }` — computed truth (e.g., `IsDisabled = Disabled || Loading`).

`BOBComponentAttributesBuilder` reads `IsX`, never the raw parameter.

### Async & JS Interop

- JS interop is split into topic-specific interfaces in `Services/JsInterop/` (`IThemeJsInterop`, `IBehaviorJsInterop`, etc.).
- Corresponding TypeScript lives under `Types/<Feature>/` and is bundled by Vite.
- Every `IJSObjectReference` invocation is wrapped in the canonical 4-tuple catch: `JSDisconnectedException`, `OperationCanceledException`, `TaskCanceledException`, `ObjectDisposedException`.
- `InvokeVoidAsync` is preferred when no return value is needed.

---

## CSS Architecture

Two layers ship with the library:

1. **Global CSS bundle** (generated by `BlazOrbit.BuildTools`) — reset, typography, themes, tokens, base component styles, family shared styles, and transition classes. These files live in `src/BlazOrbit/CssBundle/` and are produced by `[AssetGenerator]` classes.
2. **Scoped component CSS** (hand-written `.razor.css`) — handles layout and appearance specific to a single component.

All generators reference `FeatureDefinitions` constants when emitting selectors and custom property names. Never hardcode a `data-bob-*` attribute, `--bob-*` variable, or `bob-*` class name in a generator.

---

## Testing

We use **bUnit** + **Verify** for snapshot tests, plus **xUnit**.

- Add snapshot tests for any new component or visual change.
- Run `dotnet test` before pushing. CI runs the full suite on `Debug` and `Release`.
- If a snapshot changes intentionally, commit the updated `.verified.` files.

---

## Public API Tracking

The project uses the Roslyn Public API analyzer. If you add, remove, or change a public member, the build will warn with `RS0016` / `RS0017`. Apply the IDE code-fix to update `PublicAPI.Unshipped.txt`. Do not suppress these warnings.

---

## Security

If you discover a security vulnerability, please send a private email to the maintainers rather than opening a public issue. See [`SECURITY.md`](SECURITY.md) for contact details.
