# BlazOrbit

[![NuGet](https://img.shields.io/nuget/v/BlazOrbit.svg)](https://www.nuget.org/packages/BlazOrbit)
[![Build](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/release-publish.yml/badge.svg)](https://github.com/BlazOrbit/BlazOrbit/actions/workflows/release-publish.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A modern and customizable component library for **Blazor** on **.NET**.

BlazOrbit ships accessible, production-ready components built on a reflective styling pipeline (`data-bob-*` attributes + CSS custom properties).
It supports design tokens, component variants, theming, customization. 
It provides optional localization integration.

BlazOrbit is built on several key principles:

- **Efficiency and fluidity**

*It avoids unnecessary boilerplate code, and component rendering is centralized in a process that prioritizes efficiency.*

- **Less JS, more happiness**

*The use of JS is minimized as much as possible. Typescript is used in development with efficient bundling.*

- **Don’t reinvent the wheel**
 
*Everything you do in Blazor works in BlazOrbit. 
In cases such as form components, it uses InputBase just like native components. But it improves the usability of native components and adds features (Styling, Validation...).*

- **Accessibility**

*Make it easy for users to create accessible applications transparently by following the WCAG 2.2 standard.*

- **Customization**

*In addition to exposing variables to modify the theme, BlazOrbit includes a system that allows you to create registered component variants in the same way you would register a service.*

- **Continuous Development**

*BlazOrbit is not a closed-source project; it is distributed under the MIT license, and contributions are welcome. The goal is continuous improvement and the ongoing addition of new features.*

---

## Quickstart

Install the main package:

```bash
dotnet add package BlazOrbit
```

Register the services in `Program.cs`:

```csharp
using BlazOrbit;

builder.Services.AddBlazOrbit();
```

Add `<BOBInitializer>` once in your root layout (typically `MainLayout.razor`) as wrapper of the @Body content. This wires up the theme, JS interop, and static assets:

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

---

## Packages

| Package | Purpose |
| --- | --- |
| `BlazOrbit` | Component library — components, variants, theming, and JS behaviors. |
| `BlazOrbit.Core`| Framework-agnostic primitives — base component types, behavior interfaces (`IHas*`), palette and theme types. |
| `BlazOrbit.SyntaxHighlight` | Dependency-free syntax highlighter used by `BOBCodeBlock`. |
| `BlazOrbit.Localization.Server` | Cookie-based culture persistence and `BOBCultureSelector` for Blazor Server. |
| `BlazOrbit.Localization.Wasm` | `localStorage`-based culture persistence and `BOBCultureSelector` for Blazor WebAssembly. |
| `BlazOrbit.FormsFluentValidation` | Integration with `FluentValidation` for BlazOrbit forms. |

## Localization: Server vs. WASM

Both localization packages expose the same `BOBCultureSelector` component but store the selected culture differently:

- **`BlazOrbit.Localization.Server`** — for Blazor Server hosts. Sets up `RequestLocalization`, persists culture via HTTP cookie, and exposes a `/Culture/Set` endpoint.
- **`BlazOrbit.Localization.Wasm`** — for Blazor WebAssembly hosts. Persists culture in `localStorage` and configures `CultureInfo.DefaultThreadCurrentCulture` / `DefaultThreadCurrentUICulture` at startup.

For prerendered WASM (hosted WASM with Server prerender), install **both** packages. Server handles the initial HTTP request culture and WASM takes over after boot.

[Read more here about localization integration](https://blazorbit.com/concepts/localization)

---

## Features

- **Theming** — Built-in light and dark themes with CSS custom properties and automatic palette generation.
- **Variants** — Register custom rendering templates for any component via `AddBlazOrbitVariants(...)`.
- **Design Tokens** — Unified spacing, sizing, density, elevation, and shadow system across all components.
- **Accessibility** — ARIA attributes, keyboard navigation, and focus management built in.
- **Form Integration** — Full `EditContext` / `EditForm` support with validation states. Custom FluentValidation Validator ready to use.
- **JS Interop** — Modular TypeScript interop for dropdowns, modals, clipboard, color picking, drag-and-drop, and more.
- **Scoped CSS** — Each component ships scoped `.razor.css` alongside a globally generated CSS bundle.
- **Localization Ready** — Server and WASM localization packages with culture selector UI.

---

## Documentation

Documentation, component catalog and live demos can be found and installed from [the website](https://blazorbit.com) 

Autogenerated [API reference]() is generated using DocFX 

Both are included in the codebase so are closely linked to code development. 

You un it locally:

```bash
dotnet run --project docs/BlazOrbit.Docs.Wasm
```

---

## Contributing

We welcome contributions. See [`CONTRIBUTING.md`](CONTRIBUTING.md) for the full workflow, branch and commit conventions, and development setup.

Also scripts under the `scripts` folder are done to facilitate contributions to be more friendly to new contributors and avoid endless PRs.

Bug reports and feature requests: [GitHub Issues](https://github.com/BlazOrbit/BlazOrbit/issues).

---

## License

Released under the [MIT License](LICENSE.txt).  
© 2026 BlazOrbit
