# BlazOrbit

[![NuGet](https://img.shields.io/nuget/v/BlazOrbit.svg)](https://www.nuget.org/packages/BlazOrbit)
[![Build](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/release-publish.yml/badge.svg)](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/release-publish.yml)
[![Build](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/preview-publish.yml/badge.svg)](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/preview-publish.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A modern, accessible, and customizable component library for **Blazor** on **.NET 8 / .NET 10**.

BlazOrbit ships **60+ production-ready components** — forms, layouts, navigation, overlays, data grid / cards, **15
chart types** (incl. finance candlestick, gauges, radar, heatmap), drag-and-drop, global hotkeys, and a notification
center — on top of a reflective styling pipeline (`data-bob-*` attributes + CSS custom properties), a design-token
system, a variant registry, light/dark theming, and optional localization. SVG-rendered charts (no Chart.js / D3
dependency), JS-light architecture, full `EditContext` form integration, and a `.skill` bundle that teaches AI coding
agents the canonical API.

BlazOrbit is built on several key principles:

- **Efficiency and fluidity**

*It avoids unnecessary boilerplate code, and component rendering is centralized in a process that prioritizes
efficiency.*

- **Less JS, more happiness**

*The use of JS is minimized as much as possible. Typescript is used in development with efficient bundling.*

- **Don’t reinvent the wheel**

*Everything you do in Blazor works in BlazOrbit.
In cases such as form components, it uses InputBase just like native components. But it improves the usability of native
components and adds features (Styling, Validation...).*

- **Accessibility**

*Make it easy for users to create accessible applications transparently by following the WCAG 2.2 standard.*

- **Customization**

*In addition to exposing variables to modify the theme, BlazOrbit includes a system that allows you to create registered
component variants in the same way you would register a service.*

- **Continuous Development**

*BlazOrbit is not a closed-source project; it is distributed under the MIT license, and contributions are welcome. The
goal is continuous improvement and the ongoing addition of new features.*

---

## Quickstart

### Start a new project

The fastest path. Install the templates package once:

```bash
dotnet new install BlazOrbit.Templates
```

Then scaffold a Blazor Server or WebAssembly project — pick the framework and toggle localization / charts on demand:

```bash
# Blazor Server, .NET 10
dotnet new blazorbit-server -n MyApp -F net10.0

# Blazor WebAssembly, .NET 8, with localization
dotnet new blazorbit-wasm -n MyApp -F net8.0 --IncludeLocalization true

# Blazor WebAssembly, .NET 10, with charts dashboard Home + localization
dotnet new blazorbit-wasm -n MyApp -F net10.0 --IncludeLocalization true --IncludeCharts true

cd MyApp
dotnet run
```

The generated app ships a showcase Home page (or a fully-wired chart dashboard when `IncludeCharts=true`), theme
switcher, modal/toast hosts, favicon set, and (when localization is enabled) a culture selector — all already wired
through `BOBInitializer`. See the [Templates guide](https://blazorbit.com/getting-started/templates) for the full matrix
of options (8 combos: framework × localization × charts).

### Add to an existing project

Install the main package:

```bash
dotnet add package BlazOrbit
```

Register the services in `Program.cs`:

```csharp
using BlazOrbit;

builder.Services.AddBlazOrbit();
```

Add `<BOBInitializer>` once in your root layout (typically `MainLayout.razor`) as wrapper of the @Body content. This
wires up the theme, JS interop, and static assets:

```razor
<BOBInitializer DefaultTheme="dark">
    <main>
        @Body
    </main>
</BOBInitializer>
```

Optionally add hosts after BOBInitializer:

* `BOBDialog` and `BOBDrawer` require `<BOBModalHost />`
* `BOBToast` require `<BOBToastHost MaxVisiblePerPosition="5" />`

```razor
<BOBInitializer DefaultTheme="dark">
    <main>
        @Body
    </main>
</BOBInitializer>
<BOBModalHost />
<BOBToastHost MaxVisiblePerPosition="5" />
```

Now you can use any component:

```razor
@using BlazOrbit.Components

<BOBButton Variant="BOBButtonVariant.Filled" OnClick="@HandleClick">
    Click me
</BOBButton>

<BOBInputText @bind-Value="name" Label="Name" />

<BOBCard Shadow="true">
    <p>Inside a themed card.</p>
</BOBCard>
```

### Add charts (optional)

The chart family lives in a separate package. Install it on demand:

```bash
dotnet add package BlazOrbit.Charts
```

Register the JS interop service:

```csharp
using BlazOrbit.Charts.Services;

builder.Services.AddBlazOrbit();
builder.Services.AddBlazOrbitCharts(); // adds IChartJsInterop
```

Then drop a chart anywhere — the `blazorbit-charts.css` link is auto-injected on first render, no manual `<link>`
plumbing required:

```razor
@using BlazOrbit.Charts.Components
@using BlazOrbit.Charts.Models

<BOBLineChart TX="DateTime" TY="decimal"
              Series="@_revenue"
              ZoomEnabled="true"
              ShowCrosshair="true"
              Smooth="true"
              Width="640" Height="280" />
```

### Add hotkeys (optional)

Global keyboard shortcuts with scope-aware registration:

```bash
dotnet add package BlazOrbit.Hotkeys
```

Register the service and mount the host:

```csharp
using BlazOrbit.Hotkeys;

builder.Services.AddBlazOrbit();
builder.Services.AddBlazOrbitHotkeys();
```

```razor
@using BlazOrbit.Hotkeys.Components

<BOBHotkeyHost />
```

Register shortcuts from any component:

```csharp
@inject IHotkeyService Hotkeys

protected override void OnInitialized()
{
    Hotkeys.Register("ctrl+k", _ => ShowSearch(), HotkeyScope.Global);
}
```

### Add notifications (optional)

Inbox-style notification center with persistent storage:

```bash
dotnet add package BlazOrbit.Notifications
```

Register the service:

```csharp
using BlazOrbit.Notifications;

builder.Services.AddBlazOrbit();
builder.Services.AddBlazOrbitNotifications();
```

Push notifications from any component:

```csharp
@inject INotificationCenter Center

await Center.PushAsync(new BOBNotification
{
    Title = "Welcome",
    Body = "You have a new message.",
    Severity = NotificationSeverity.Info
});
```

Drop the bell badge into your navbar:

```razor
@using BlazOrbit.Notifications.Components

<BOBNotificationBell />
```

---

## Packages

| Package                           | Purpose                                                                                                                                                        |
|-----------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `BlazOrbit`                       | Main component library — 60+ components, variants, theming, JS behaviors.                                                                                      |
| `BlazOrbit.Core`                  | Framework-agnostic primitives — base component types, behavior interfaces (`IHas*`), palette and theme types.                                                  |
| `BlazOrbit.Charts`                | SVG-rendered chart family — 15 chart types with zoom, brush, crosshair, live streaming, annotations. No Chart.js / D3.                                         |
| `BlazOrbit.Hotkeys`               | Global keyboard shortcut registry — document-level keydown listener, modifier-aware combo grammar, scope-aware registration (Global / Page) with auto-cleanup. |
| `BlazOrbit.Notifications`         | Persistent inbox-style notification center with `BOBNotificationBell` badge and swappable `INotificationStore` strategy.                                       |
| `BlazOrbit.SyntaxHighlight`       | Dependency-free syntax highlighter used by `BOBCodeBlock`.                                                                                                     |
| `BlazOrbit.Localization.Server`   | Cookie-based culture persistence and `BOBCultureSelector` for Blazor Server. Integrates with `RequestLocalization`.                                           |
| `BlazOrbit.Localization.Wasm`     | `localStorage`-based culture persistence and `BOBCultureSelector` for Blazor WebAssembly.                                                                      |
| `BlazOrbit.Localization.Shared`   | Shared `BOBCultureSelector` markup + types reused by both Server and Wasm localization integrations. Ships the BOBLocalize source generator under `analyzers/dotnet/cs/` so consumer `[BobLocalizationBundle]` attributes emit registrations automatically. Pulled in transitively. |
| `BlazOrbit.FormsFluentValidation` | Integration with `FluentValidation` for BlazOrbit forms.                                                                                                       |
| `BlazOrbit.Templates`             | `dotnet new` templates — `blazorbit-server` and `blazorbit-wasm` with optional localization, charts, notifications, and hotkeys.                               |

> CSS and JS assets ship pre-built (hand-written, committed) as static web assets. Consumers never need Node, npm,
> Vite, esbuild or any JS toolchain installed; `dotnet build` is enough.

## Localization (BOBLocalize)

BlazOrbit uses **BOBLocalize** — a compile-time localization system that avoids `.resx` files and satellite assemblies:

- Translations live in `.tn` text files.
- A Roslyn source generator reads them at build time and bakes each bundle into a `FrozenDictionary<ulong, string>`.
- Components consume the standard `IStringLocalizer<T>` interface via `BobLocalizer<T>`.
- A pluggable provider chain (`IBobLocalizationProvider`) allows overlays from a database or CMS.

Both integration packages expose the same `BOBCultureSelector` (Dropdown / Flags variants) but store culture differently:

- **`BlazOrbit.Localization.Server`** — cookie-based persistence with `RequestLocalization` + `/BlazOrbit/Culture/Set` endpoint.
- **`BlazOrbit.Localization.Wasm`** — `localStorage`-based persistence; culture is applied before the first component renders.

For prerendered WASM (hosted WASM with Server prerender), install **both** packages. Server handles the initial HTTP
request culture and WASM takes over after boot.

[Read more here about localization integration](https://blazorbit.com/concepts/localization)

---

## Features

### Foundation

- **Theming** — Built-in light and dark themes with CSS custom properties and automatic palette generation. Override
  `--palette-*` to re-skin colors and `--bob-*` to retune typography, sizing, density, borders, focus, z-index, ripple,
  scrollbar, and family defaults.
- **Variants** — Register custom rendering templates for any component via `AddBlazOrbitVariants(...)`. Switch between
  built-in look-and-feels (e.g. `BOBButtonVariant.Filled` / `Outlined` / `Tonal`) or ship your own.
- **Design Tokens** — Unified typography, sizing (5-step scale), density (Comfortable/Standard/Compact), borders,
  outline, opacity, z-index, ripple, transitions, scrollbar, and family defaults — all overridable from a single CSS
  var.
- **Family Pattern** — Components share family-level styling via marker interfaces (`IInputFamilyComponent`,
  `IPickerFamilyComponent`, `IDataCollectionFamilyComponent`, `IDataVisualizationFamilyComponent`) → consistent UX
  without per-component CSS duplication.
- **Reflective styling** — `data-bob-*` attributes on every root element drive scoped CSS without prop-drilling. State (
  `data-bob-active`, `data-bob-loading`, `data-bob-disabled`, …) is reflected automatically.
- **Accessibility (WCAG 2.2 AA)** — ARIA attributes, keyboard navigation, focus management, prefers-reduced-motion, and
  color-contrast tokens built in.
- **JS-light architecture** — Minimal JS interop, TypeScript source-of-truth, esbuild bundling. Scoped CSS auto-injected
  by chart family on first render — no manual `<link>` plumbing.

### Forms & validation

- **`EditContext` / `EditForm`** integration — every input plays nicely with the standard Blazor form pipeline.
- **`BlazOrbit.FormsFluentValidation`** — drop-in `FluentValidation` adapter (
  `<BOBFluentValidator TModel TValidator />`).
- **17 input components** — text, textarea, number, password, checkbox, switch, radio, dropdown (searchable + multi-select + tree), file upload, color picker, date/time, date range, autocomplete, OTP, number-slider, range-slider.

### Data display

- **`BOBDataGrid` + `BOBDataCards`** — column-driven data pipeline: filter, sort, paginate, select, virtualize, custom
  templates. Both share the same column definitions.
- **Aggregate footer** — Sum / Average / Count / Min / Max / Custom per column on the post-filter set.
- **Multi-column sort** — `Shift+Click` headers; priority badges (1, 2, 3 …).
- **Per-row actions** — sticky-right action column on the grid, action strip on the cards. Per-row `Visible` / `Enabled`
  predicates.
- **Bulk actions on selection** — toolbar buttons appear when at least one row is selected; `Enabled` predicate gates
  destructive operations.
- **Master-detail / row expansion** — `RowDetailTemplate` adds a chevron toggle that opens a sub-row (grid) or inline
  section (cards).
- **Skeleton loading** — animated row / card placeholders. `LoadingMode = Skeleton` vs `Spinner`.
- **Error + Empty CTA states** — `Error` / `ErrorContent` for remote-load failures; `EmptyActionTemplate` for "Create
  first record" without rewriting `EmptyContent`.
- **`BOBCodeBlock`** — dependency-free syntax highlighter (Prism-style) for any code language.

### Charts (`BlazOrbit.Charts`)

15 chart types, all SVG-rendered, all keyboard-accessible, all theme-aware:

| Family        | Types                                                                                                                          |
|---------------|--------------------------------------------------------------------------------------------------------------------------------|
| **Cartesian** | Bar (None / Stacked / PercentStacked / Bidirectional / Waterfall), Line, Area, Scatter / Bubble, Sparkline, Histogram, Boxplot |
| **Polar**     | Pie, Donut, Radar, Gauge (Semi / ¾ / Full + zones), Polar Area / Coxcomb                                                       |
| **Grid**      | Heatmap (matrix + calendar)                                                                                                    |
| **Pipeline**  | Funnel (tapered + rectangular)                                                                                                 |
| **Finance**   | Candlestick / OHLC + optional volume pane                                                                                      |

**Cross-cutting features**:

- **Zoom + brush** — wheel-to-zoom, double-click reset, drag-to-select with `OnBrush` callback.
- **Crosshair** — multi-series snap-to-nearest with HTML readout.
- **Annotations** — text labels, vertical bands, shape markers (Circle / Square / Triangle / Star / Diamond).
- **Live streaming** — `AppendPointsAsync`, FIFO `StreamingWindow`, throttle, follow-zoom.
- **Reference lines** — horizontal threshold lines (SLO / target / capacity) rendered above series.
- **Export to PNG** — client-side, no server roundtrip.
- **Auto-injected CSS** — `<link>` to `blazorbit-charts.css` is appended on first JS module import; consumers don't need
  to edit `index.html`.

### Layout & navigation

- **`BOBSidebarLayout`** — sticky header + sticky sidebar + responsive mobile drawer.
- **`BOBTreeMenu`** — hierarchical nav with split-affordance for nodes that both navigate and have children (real
  `<a href>` for the label + dedicated chevron `<button>` for expand). Auto-expands the active route's ancestor chain on
  deep-link / refresh.
- **`BOBTabs`**, **`BOBAccordion`**, **`BOBCarousel`**, **`BOBCard`**, **`BOBFlexStack`**, **`BOBGrid`**.

### Overlays & feedback

- **`BOBDialog`** + **`BOBDrawer`** (require `<BOBModalHost />`).
- **`BOBToast`** with positions, timeouts, action buttons (require `<BOBToastHost />`).
- **`BOBTooltip`** — light JS interop, follows scroll.
- **`BOBLoading`** — spinner + skeleton variants.

### Localization

- **Compile-time bundles** — `.tn` text files → Roslyn source generator → `FrozenDictionary<ulong, string>`. No `.resx`, no satellite assemblies, no runtime reflection.
- **Standard contract** — components inject `IStringLocalizer<T>`; `BobLocalizer<T>` is the implementation. Pluggable provider chain (`IBobLocalizationProvider`) supports DB/CMS overlays.
- **Server + WASM** packages with the same `BOBCultureSelector` UI (Dropdown / Flags variants).
- Cookie-based persistence on Server, `localStorage` on WASM.
- Pre-render compatible — install both packages for hosted WASM with Server prerender.

### Build & tooling

- **`.dotnet new` templates** — `blazorbit-server` + `blazorbit-wasm` with `IncludeLocalization`, `IncludeCharts`,
  `UseNotificationsCenter`, `UseHotKeys`, and `Theme` flags. Optional packages replace the showcase Home with targeted
  demos (dashboard for charts, inbox for notifications, shortcuts for hotkeys).
- **Hand-written assets** — `wwwroot/css/blazorbit.css` and `wwwroot/js/Types/**/*.js` are committed source files
  in every BlazOrbit package. No CSS generator, no TypeScript transpile, no Node, no Vite. `dotnet build` is enough.
- **Public API tracking** — `RoslynAnalyzers.PublicApi` enabled on every package; no symbol leaks across releases.
- **`scripts/`** — one-shot helpers for testing templates end-to-end (`test-templates.ps1`), seeding local NuGet feeds,
  dev / release builds.

### AI assistant integration

- **`.skill` bundle** — packaged knowledge that teaches AI agents (Claude Code, Kimi, OpenCode, generic Anthropic Skill
  loaders) the canonical component API. Regenerated on every release; no hallucinated parameters or stale signatures.

---

## Documentation

Documentation, component catalog and live demos can be found and installed from [the website](https://blazorbit.com)

Autogenerated [API reference](https://blazorbit.github.io/BlazOrbit/) is generated using DocFX

Both are included in the codebase so are closely linked to code development.

You run it locally:

```bash
dotnet run --project docs/BlazOrbit.Docs.Wasm
```

---

## AI assistant integration

BlazOrbit ships a `.skill` bundle that teaches AI coding agents to use the canonical component API, theming pipeline,
and conventions — no more hallucinated parameters or stale BOB* signatures. The bundle is regenerated against every
release so the agent's mental model stays in lock-step with the published library.

Download the latest bundle from the GitHub release:

```bash
curl -L -o blazorbit-user.skill \
  https://github.com/BlazOrbit/BlazOrbit/releases/latest/download/blazorbit-user.skill
```

Compatible loaders:

* **Claude Code** — extract into `~/.claude/skills/blazorbit-user/`.
* **Kimi (Moonshot)** — upload through the Skills tab in the web UI.
* **OpenCode** — extract into the workspace `skills/` folder and reference it from `opencode.json`.
* **Generic loaders** — any agent that consumes the public Anthropic Skill format (`SKILL.md` at the archive root with
  optional `references/` and `scripts/`).

See the [AI Skill guide](https://blazorbit.com/getting-started/ai-skill) for per-agent install snippets, verification
prompts, update flow, and contributor instructions for building the bundle locally.

---

## Contributing

We welcome contributions. See [`CONTRIBUTING.md`](CONTRIBUTING.md) for the full workflow, branch and commit conventions,
and development setup.

Also scripts under the `scripts` folder are done to facilitate contributions to be more friendly to new contributors and
avoid endless PRs.

Bug reports and feature requests: [GitHub Issues](https://github.com/BlazOrbit/BlazOrbit/issues).

---

## License

Released under the [MIT License](LICENSE.txt).  
© 2026 BlazOrbit
