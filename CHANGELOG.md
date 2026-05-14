# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0-preview1] - 2026-05-01

First public preview of BlazOrbit — a modern, accessible component library for Blazor.

### What's Included

#### Actions
- **Button** — The workhorse of any UI. Multiple variants, sizes, loading states, icons, and color choices.

#### Forms
- **InputText** — Single-line text input with outlined, filled, and standard variants. Prefix/suffix slots and full validation support.
- **InputTextArea** — Multi-line text input with auto-resize, character limits, and all standard input states.
- **InputNumber** — Numeric input with min, max, stepper buttons, acceleration, and culture-aware formatting.
- **InputPassword** — Password input with built-in visibility toggle and optional strength meter score.
- **InputCheckbox** — Boolean input checkbox with label, helper text, and custom icons for checked/unchecked states.
- **InputSwitch** — Toggle-style boolean input with optional icons in the thumb and custom track colors.
- **InputRadio** — Radio button group for single selection. Supports vertical or horizontal layout, custom labels, and validation.
- **InputDropdown** — Single and multiple select list with keyboard navigation, search, and typed value entry.
- **InputDropdownTree** — Hierarchical dropdown with single/multiple selection, checkboxes, and cascading expand.
- **InputFile** — Drop-zone file picker with multi-select, MIME / size validation, image previews and upload progress.
- **InputColor** — Full-featured color input with HSL/RGB/Hex output, an inline or dropdown picker, and preset swatches.
- **InputDateTime** — Date and time picker with culture-aware formatting. Supports date-only, time-only, or full datetime modes.
- **InputDateRange** — Date range picker with two date fields and a calendar dialog. Select start and end in one control.
- **AutoComplete** — Async-source typeahead with debounce, cancellation, keyboard navigation and custom item templates.
- **InputOtp** — Multi-slot one-time-code input with auto-advance, backspace navigation, paste distribution, and masking.
- **InputNumberSlider** — Single-thumb numeric slider with min, max, step, ticks, and value label. Supports vertical orientation.
- **InputRangeSlider** — Two-thumb range slider for selecting minimum and maximum values. Supports validation and ticks.

#### Display
- **Avatar / AvatarGroup** — Image avatar with initials fallback (deterministic gradient), shape and status variants. Group stacking with max-overflow.
- **Banner** — Persistent severity message with auto-icon, dismiss affordance and an action slot.
- **Badge / NotificationBadge** — Compact label for status, counts, or tags. Circular, dot-only, or with content variants.
- **Chip / ChipGroup** — Compact tag for filters, selections and removable inline items. Group with overflow and input for free-text chips.
- **ProgressBar / ProgressRing / ProgressIcon** — Linear and circular progress indicators with determinate, indeterminate, and segmented modes.
- **Rating** — Star rating with optional half-step, hover preview, and full keyboard navigation.
- **StatCard** — Compact KPI card with value, delta, trend arrow, optional sparkline slot and footer text.

#### Layout & Containers
- **Accordion / AccordionItem** — Vertically stacked, expandable panels. Choose between multi-open, single-open, or controlled modes.
- **AspectRatio** — Layout box that locks any child to a fixed width-to-height ratio.
- **Card** — A flexible container for content — header, media, body, and actions in a single cohesive surface.
- **Container / Section** — Semantic max-width wrappers with shared container size tiers.
- **FlexStack** — A flexible stack container for horizontal or vertical layouts with alignment, wrap, and gap control.
- **Grid / GridItem** — 12-column responsive grid layout with breakpoint-aware column spans, gaps, direction and justification.
- **PageHeader** — Title strip with eyebrow, lead paragraph, breadcrumbs and right-aligned action buttons.
- **Splitter / SplitterPane** — Resizable multi-pane layout with horizontal or vertical grippers.
- **SidebarLayout / StackedLayout / BlazorLayout** — Application shell layouts with sidebar, stacked, and Blazor-integrated variants.

#### Navigation
- **Breadcrumbs** — Compact trail showing where the user is and how to walk back up the route.
- **Tabs / Tab** — Tab panels with three visual variants. Declare tabs as children — the active one swaps content automatically.
- **Stepper / Step** — Multi-step flow with header rail, body slot, and cancellable step transitions.
- **Timeline** — Vertical or horizontal trail of dated events with optional grouping and alternating layout.
- **TreeMenu / TreeMenuItem** — Hierarchical navigation with icons, nested groups, vertical or horizontal orientation.
- **TreeSelector / TreeSelectorItem** — Hierarchical selection with single/multiple modes, optional checkboxes, cascading selection and search.

#### Data Display
- **CodeBlock** — Syntax-highlighted code with a title, language label, and one-click copy. Auto-detects language or accepts explicit override.
- **DataCards** — Responsive card grid with sorting, filtering, pagination, selection, and custom templates.
- **DataGrid / DataColumn** — Strongly-typed tabular data with sorting, filtering, pagination, selection, custom cell editors, and inline editing.

#### Charts (15 SVG chart types)
- **BarChart** — Vertical bar chart with multi-series support. Categorical X axis.
- **LineChart / Sparkline** — Continuous line chart with smooth curves, per-point markers, tooltips, zoom, annotations and area fill. Sparkline variant hides axes for inline KPI use.
- **AreaChart** — Area chart with filled regions between the line and the baseline. Supports overlapping series and smooth curves.
- **PieChart** — Full circular distribution chart. Each slice value is auto-normalized to 100% and labeled.
- **DonutChart** — Annular distribution chart — same shape as the pie but with a hollow centre.
- **ScatterChart** — Cartesian scatter / bubble chart. Plots disconnected XY points to expose distributions and outliers.
- **RadarChart** — Radar / spider chart — multi-dimensional comparison across N axes (3–12).
- **PolarAreaChart** — Polar-area / Coxcomb / Rose chart — pie variant where every slice has the same angle and radius encodes value.
- **HeatmapChart** — 2D matrix where each cell's intensity maps to a color along a sequential ramp.
- **HistogramChart** — Distribution of a single numeric variable. Auto-bins raw observations using Sturges' rule.
- **BoxplotChart** — Box-and-whisker plot for comparing distributions across groups.
- **CandlestickChart** — Candlestick chart for financial time series.
- **FunnelChart** — Multi-stage drop-off visualization.
- **GaugeChart** — KPI gauge — single value rendered as an arc fill against a track.

#### Feedback
- **Dialog / Drawer** — Accessible modal dialogs and edge-anchored drawers. Use declaratively in markup or imperatively via `IDialogService`.
- **ModalContainer / ModalHost** — Global modal host that renders dialogs and drawers at the app root.
- **Toast / ToastHost** — Transient toast notifications with six positions, configurable duration, animations, and severity levels.
- **ConfirmDialog** — Themed replacement for `window.confirm()` — async, severity-aware, modal-stack-aware.
- **Tooltip** — Context popups with 12 placements, five triggers, interactive content, delays, and arrow indicator.

#### Utils
- **Draggable** — Make any element mouse-draggable with fine-grained events.
- **SvgIcon** — Render SVG icons from bundled Material or UI sets, or provide your own custom vector paths.
- **ThemeSelector** — Theme toggle switch between light, dark, and custom themes. Automatically persists selection and applies `data-bob-theme` to `<html>`.
- **Initializer** — App-shell initializer that mounts theme management, modal host, and toast host.

#### Services
- **ConfirmService** — Imperative confirmation dialogs via `IConfirmService` with severity-aware defaults.
- **HotkeyService** — Global keyboard shortcut registry with auto-cleanup, modifier-aware combos, and scoped contexts.
- **NotificationCenter** — Persistent inbox-style notifications with a paired bell badge.

### Architecture Highlights

- **Reflective styling pipeline** — components declare capabilities via `IHas*` interfaces; `data-bob-*` attributes and
  CSS custom properties drive the visual layer without brittle class toggles.
- **Design tokens & theming** — built-in Light/Dark themes with a full CSS-variable palette (`--palette-*`) and a
  complete design-token catalog (`--bob-*`: typography, sizing, density, borders, outline, opacity, z-index, ripple,
  scrollbar, input/picker family defaults). Consumer overrides flow through `--palette-*`, `--bob-*`, and per-instance
  `--bob-inline-*`. The shipped Theme Generator tool exposes both palette and tokens as a live editor with JSON/CSS/C#
  export.
- **Component variants** — register custom render templates per component type through `AddBlazOrbitVariants(...)`.
- **JS interop modules** — minimal, tree-shaken TypeScript bundles for ripple, dropdowns, modals, color-picker,
  clipboard, draggable, theme switching, and local-storage.
- **Accessibility first** — WCAG 2.2 AA compliant focus states, `aria-*` attributes, reduced-motion support, and
  keyboard navigation built in.
- **Localization (BOBLocalize)** — compile-time translation bundles from `.tn` text files via Roslyn source generator;
  zero runtime reflection, no `.resx` or satellite assemblies. Pluggable provider chain (`IBobLocalizationProvider`)
  supports database or CMS overlays. Ships with `BlazOrbit.Localization.Server` (cookie-based) and
  `BlazOrbit.Localization.Wasm` (`localStorage`-based) packages, both providing `BOBCultureSelector` with Dropdown
  and Flags variants. Components consume the standard `IStringLocalizer<T>` interface via `BobLocalizer<T>`.
- **Optional integrations** — `BlazOrbit.FormsFluentValidation` for FluentValidation-powered form validation, and
  `BlazOrbit.SyntaxHighlight` for zero-dependency code highlighting.
- **Multi-targeting** — ships for both .NET 8.0 and .NET 10.0.

---

[Unreleased]: https://github.com/BlazOrbit/BlazOrbit/compare/v0.1.0-preview1...HEAD

[0.1.0-preview1]: https://github.com/BlazOrbit/BlazOrbit/releases/tag/v0.1.0-preview1
