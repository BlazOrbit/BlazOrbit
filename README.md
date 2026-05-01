# BlazOrbit

[![NuGet](https://img.shields.io/nuget/v/BlazOrbit.svg)](https://www.nuget.org/packages/BlazOrbit)
[![Build](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/release-publish.yml/badge.svg)](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/release-publish.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Component library for Blazor Server and Blazor WebAssembly on .NET 10. Ships themeable, accessible components driven by a reflective `data-bob-*` + CSS-custom-property pipeline, with variants, design tokens, and JS-behavior interop for features like ripples, transitions, dropdowns, and modals.

## Quickstart

```bash
dotnet add package BlazOrbit
```

Register the services in `Program.cs`:

```csharp
using BlazOrbit;

builder.Services.AddBlazOrbit();
```

Place `<BOBInitializer />` once in the root layout (typically `MainLayout.razor`) so the theme, JS interop, and asset references are wired up:

```razor
<BOBInitializer />

<main>
    @Body
</main>
```

That's enough to start using any component:

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

## Packages

| Package | Purpose |
|---|---|
| `BlazOrbit` | Component library (components, variants, theming, JS behaviors). |
| `BlazOrbit.Core` | Framework-agnostic primitives: base component types, `IHas*` behavior interfaces, palette/theme types. |
| `BlazOrbit.SyntaxHighlight` | Dependency-free syntax highlighter used by `BOBCodeBlock`. |
| `BlazOrbit.Localization.Server` | Cookie-based `RequestLocalization` + culture endpoint + `BOBCultureSelector` for Blazor Server hosts. |
| `BlazOrbit.Localization.Wasm` | `localStorage`-persisted culture + `BOBCultureSelector` for Blazor WebAssembly hosts. |
| `BlazOrbit.FluentValidation` | `FluentValidation` integration for BlazOrbit forms. |
| `BlazOrbit.BuildTools` | `dotnet` tool invoked by the shipped `.targets` to generate the CSS bundle at build time. |

## Localization: Server vs. WASM

Both localization packages provide the same `BOBCultureSelector` component but persist the user's culture differently:

- **`BlazOrbit.Localization.Server`** — use when hosting in Blazor Server. Registers `app.UseRequestLocalization(...)` via an `IStartupFilter`, persists the selected culture as an HTTP cookie, and adds a `/Culture/Set` endpoint so non-JS redirects still update the culture.
- **`BlazOrbit.Localization.Wasm`** — use when hosting in Blazor WebAssembly. Persists the selected culture in `localStorage` via JS interop and configures `CultureInfo.DefaultThreadCurrentCulture` / `DefaultThreadCurrentUICulture` at startup.

For prerendered WASM (hosted WASM app with a Server prerender), install **both** — Server handles the initial HTTP response culture and WASM takes over after boot.

## Documentation

The component catalog, live demos, and API reference live in the docs site (Blazor WASM). Run locally:

```bash
dotnet run --project docs/BlazOrbit.Docs.Wasm
```

## Contributing

1. Fork the repository and create a topic branch off `develop`.
2. Install the pinned SDK — `dotnet --version` must match `global.json` (10.0.203 at time of writing).
3. Run `dotnet build BlazOrbit.slnx -c Debug` and `dotnet test` before opening a PR.
4. Follow the conventions in `AGENTS.md` (component architecture, CSS rules, async/JS interop patterns).
5. Open the PR against `develop`. CI packs and surfaces the `.nupkg` artifacts so reviewers can test-install.

Bug reports and feature requests: [GitHub Issues](https://github.com/BlazOrbit/BlazOrbit/issues).

## License

Released under the [MIT License](LICENSE.txt). © 2026 Samuel Maícas (@cdcsharp).
