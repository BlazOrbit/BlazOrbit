# BlazOrbit API reference

Auto-generated reference for every public type and member in the [BlazOrbit](https://github.com/BlazOrbit/BlazOrbit) .NET 10 component library.

For tutorials, live examples, and theming guides, see the official documentation site at [blazorbit.dev](https://blazorbit.dev).

## Packages covered

| Package | Purpose |
|---|---|
| [`BlazOrbit`](xref:BlazOrbit) | Component library — Forms, Generic, Layout. |
| [`BlazOrbit.Core`](xref:BlazOrbit.Core) | Framework-agnostic primitives, base components, theming, `IHas*` interfaces. |
| [`BlazOrbit.SyntaxHighlight`](xref:BlazOrbit.SyntaxHighlight) | Tokenizer used by `BOBCodeBlock`. |
| [`BlazOrbit.Localization.Server`](xref:BlazOrbit.Localization.Server) | Server-side cookie + `RequestLocalization` integration. |
| [`BlazOrbit.Localization.Wasm`](xref:BlazOrbit.Localization.Wasm) | WASM `localStorage`-backed localization. |
| [`BlazOrbit.Localization.Shared`](xref:BlazOrbit.Localization.Shared) | Shared `BOBCultureSelector` UI. |
| [`BlazOrbit.Translations`](xref:BlazOrbit.Translations) | Library-shipped translation resources. |
| [`BlazOrbit.FluentValidation`](xref:BlazOrbit.FluentValidation) | FluentValidation adapter for `EditContext`. |

Source generators (`BlazOrbit.CodeGeneration`, `BlazOrbit.Core.CodeGeneration`) and the build-time tool (`BlazOrbit.BuildTools`) are intentionally omitted — consumers do not reference them directly.

## How this site is built

DocFX consumes the `.xml` documentation files emitted by `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `Directory.Build.props`. The build runs once per push to `master` and publishes to GitHub Pages.
