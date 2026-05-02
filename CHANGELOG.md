# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0-preview1] - 2026-05-01

First public preview of BlazOrbit — a modern, accessible component library for Blazor.

### What's Included

- **Core component set** — Button, Input, Select, Badge, Tabs, Tooltip, Card, DataGrid, DataCards, TreeMenu, TreeSelector, Dialog, Drawer, Toast, and more.
- **Reflective styling pipeline** — components declare capabilities via `IHas*` interfaces; `data-bob-*` attributes and CSS custom properties drive the visual layer without brittle class toggles.
- **Design tokens & theming** — built-in Light/Dark themes with a full CSS-variable palette; consumer overrides flow through `--bob-inline-*` and `--palette-*` variables.
- **Component variants** — register custom render templates per component type through `AddBlazOrbitVariants(...)`.
- **JS interop modules** — minimal, tree-shaken TypeScript bundles for ripple, dropdowns, modals, color-picker, clipboard, draggable, theme switching, and local-storage.
- **Accessibility first** — WCAG 2.2 AA compliant focus states, `aria-*` attributes, reduced-motion support, and keyboard navigation built in.
- **Localization** — Server-side cookie-based and WASM `localStorage`-based culture switching with `BOBCultureSelector`; Spanish and English resources included.
- **Optional integrations** — `BlazOrbit.FluentValidation` for FluentValidation-powered form validation, and `BlazOrbit.SyntaxHighlight` for zero-dependency code highlighting.
- **Multi-targeting** — ships for both .NET 8.0 and .NET 10.0.

> ⚠️ This is a pre-release. APIs and CSS variables may change before the stable 1.0.0.

---

[Unreleased]: https://github.com/BlazOrbit/BlazOrbit/compare/v0.1.0-preview1...HEAD
[0.1.0-preview1]: https://github.com/BlazOrbit/BlazOrbit/releases/tag/v0.1.0-preview1
