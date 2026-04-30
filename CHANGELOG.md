# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed (breaking)

- **CSS palette variable naming migrated to kebab-case** (decision D-10, THEME-06). Compound names that previously merged into one token now insert dashes:
  - `--palette-primarycontrast` → `--palette-primary-contrast`
  - `--palette-secondarycontrast` → `--palette-secondary-contrast`
  - `--palette-backgroundcontrast` → `--palette-background-contrast`
  - `--palette-successcontrast` → `--palette-success-contrast`
  - `--palette-warningcontrast` → `--palette-warning-contrast`
  - `--palette-errorcontrast` → `--palette-error-contrast`
  - `--palette-infocontrast` → `--palette-info-contrast`
  - `--palette-surfacecontrast` → `--palette-surface-contrast`
  - `--palette-hovertint` → `--palette-hover-tint`
  - `--palette-activetint` → `--palette-active-tint`
  Same migration applies to the per-theme aliases (`--dark-X`, `--light-X`). Single-word names (`--palette-primary`, `--palette-background`, etc.) are unchanged. Consumers with custom CSS overrides referencing the old single-token names must migrate; the rename is a one-time, mechanical search-and-replace.
- **`IModalService` / `IToastService` event and method signatures migrated to async** (ASYNC-12). Subscribers and callers must update:
  - `IModalService.OnChange` (`event Action?`) → `IModalService.OnChangeAsync` (`event Func<Task>?`).
  - `IToastService.OnChange` (`event Action?`) → `IToastService.OnChangeAsync` (`event Func<Task>?`).
  - `IToastService.Show*/Close/CloseAll/Pause/Resume` (`void`) → `ShowAsync*/CloseAsync/CloseAllAsync/PauseAsync/ResumeAsync` (`Task`).
  - `ToastService.ShowFragment` → `ShowFragmentAsync` (`Task`); `ToastService.Remove` → `RemoveAsync` (`Task`, internal).
  Migration: rename method calls and add `await`; convert `service.OnChange += Handler;` to `service.OnChangeAsync += HandlerAsync;` where `HandlerAsync` returns `Task`. Awaiting `Show*Async` now propagates exceptions from subscribers (e.g. a misbehaving `BOBToastHost`) instead of dropping them in `Task.UnobservedTaskException`. The 4 internal call sites that previously wrapped notifications in `BOBAsyncHelper.SafeFireAndForget` (`BOBModalHost.HandleModalChange`, `BOBToastHost.HandleToastChange`, `ToastService.ScheduleDismiss → DismissAfterDelay → CloseAsync`, `ModalService.OnModalClose`) now await directly. `SafeFireAndForget` remains for intrinsic timer scheduling (`BOBTooltip.StartAutoClose`, `ToastService.ScheduleDismiss` itself).

### Added

- Permanent resolution flow in `TASKS.md` (fix commit + follow-up `docs(tasks)` with short hash).
- `FeatureDefinitions.Tokens.Size`, `Density`, `Border`, `Highlight`, `Ripple`, `Scrollbar`, `Input`, `Picker`, `Transitions` — centralized the values previously hardcoded across generators.
- `ScrollBarGenerator` opt-in via `[data-bob-scrollbars]` / `.bob-scrollbars`; library no longer touches consumer scrollbars by default.
- `VerifyBlazorBitAssets` MSBuild target that fails the build when any expected `CssBundle/*.css` partial or `wwwroot/js/Types/**/*.min.js` interop module is missing.
- `BOBGEN010` diagnostic: reports when `[AutogenerateCssColors]` targets a class that is not declared as `public static partial`.
- `AnalyzerReleases.Shipped.md` + `AnalyzerReleases.Unshipped.md` for both generator projects (release-tracking ready).
- `global.json` pinning the SDK; CI uses `global-json-file` so local dev and CI run the same version.
- `.github/dependabot.yml` (nuget + github-actions, grouped).
- `.github/workflows/codeql.yml` (csharp + javascript-typescript, security-extended queries).
- `SECURITY.md` and `CODE_OF_CONDUCT.md`.
- README rewritten with badges, quickstart, package table, docs link, and contribution flow; shipped inside every packable .nupkg.

### Changed

- `TypographyGenerator`, `DesignTokensGenerator`, `TransitionsCssGenerator`, `InputFamilyCssGenerator`, `PickerFamilyGenerator`, `CssInitializeThemesGenerator` — all literal values tokenized via `FeatureDefinitions`.
- `IAssetGenerator.GetContent` implementations drop `async` in favor of `Task.FromResult` (no actual awaits); CS1998 eliminated.
- `ColorClassGenerator` migrated to `ForAttributeWithMetadataName`, emits via `StringBuilder` instead of Roslyn syntax tree + `NormalizeWhitespace`, and propagates `CancellationToken`.
- `ComponentInfoGenerator.XmlSummaryFromSymbol` reads via `IPropertySymbol.GetDocumentationCommentXml` + `XDocument` instead of manual trivia regex; external-assembly docs now flow through.
- `publish.yml`: `dotnet pack` runs on every run (including PRs); `dotnet nuget push` remains gated by `should_publish`. Adds `concurrency`, `permissions`, NuGet cache, Node 20 pin, TRX + coverage collection, artifact uploads, and CodeQL workflow.
- `CleanBlazorBitAssets` tolerates Windows handle locks via `ContinueOnError="WarnAndContinue"` + `TreatErrorsAsWarnings="true"`.
- `BOBTabs.RegisterTab`/`UnregisterTab` call `StateHasChanged()` directly instead of wrapping `InvokeAsync(StateHasChanged)` in `BOBAsyncHelper.SafeFireAndForget`. Both entry points are invoked from `BOBTab.OnInitialized`/`Dispose`, which Blazor dispatches on the renderer's `SynchronizationContext`, making `InvokeAsync` redundant.
- `_base.css` (via `BaseComponentGenerator`) now emits a global `@media (prefers-reduced-motion: reduce)` block that neutralizes `animation-duration`, `animation-iteration-count`, `transition-duration`, and `scroll-behavior` for `*, *::before, *::after`. WCAG 2.2 AA 2.3.3 (Animation from Interactions). The 0.01ms residual preserves `animationend` / `transitionend` callbacks; functional close timers (Toast, Dialog, Drawer) use `Task.Delay` independent of CSS duration and keep working unchanged.
- `BOBCultureSelector` (Server + WASM) now resolves all visible accessibility text through `IStringLocalizer<BOBCultureSelectorResources>`. The `<select>` and the flag-list `<div>` carry `aria-label="Select language"` (Spanish: "Seleccionar idioma"); the active flag button's `title` becomes "Current language: {DisplayName}" / "Idioma actual: {DisplayName}". The flag list is now an explicit `role="radiogroup"` whose buttons report `role="radio"` + `aria-checked`. English (neutral) and Spanish `.resx` ship embedded inside both Localization packages; consumers add cultures by dropping additional `BOBCultureSelectorResources.<culture>.resx` overrides in their own assemblies (standard satellite-assembly fallback chain). Resource manifest names are pinned via `<LogicalName>` in each csproj to match what `ResourceManagerStringLocalizerFactory` composes for `ResourcesPath="Resources"`.
- Docs WASM site `Layout/NavMenu.razor` now resolves all menu text through `IStringLocalizer<NavMenu>` instead of hardcoded English strings. `Resources/Layout/NavMenu.resx` (English neutral) + `NavMenu.es.resx` ship inside the docs assembly; switching culture via `BOBCultureSelector` now re-renders the navigation tree in the active language.

### Removed

- `BOBThemePaletteBase.Black` / `White` properties and the matching `--palette-black` / `--palette-white` variables (they were absolute constants mis-filed under the theme API; consumers use `BOBColor.Black.Default` / hex literals).
- `BOBThemePaletteBase.GetPaletteMapping()` — emitted aliased `--palette-X: var(--{themeId}-X)` entries that no generator consumed.
- `CssColor.SetContrastBlack` / `SetContrastWhite` setters (dead since introduction).
- `NavigationLoading/NavigationLoadingInterop.min.js` ghost entry from the asset verifier.

### Fixed

- `[AutogenerateCssColors]` syntactic predicate matched any attribute whose name *contained* `"AutogenerateCssColors"`; replaced with `ForAttributeWithMetadataName` exact-FQN match.
- `BuildTemplates.GetPackageJsonTemplate` no longer emits the invalid `"public static": true` key; uses `"private": true`.
- `.js.map` Vite source maps no longer ship inside Release `.nupkg` (dropped after `BuildBlazorBitAssets`, before the StaticWebAssets manifest step).
- `wwwroot/css/blazorbit.css` no longer tracked in git — regenerated by the build pipeline.
- slnx: every project now carries an `Id` attribute (was missing on `Docs.Components` and the root `BlazorBit` project).

## [1.0.0] - _TBD_

The initial public release. Release notes will be finalized here when the 1.0 tag is cut.

---

[Unreleased]: https://github.com/BlazOrbit/BlazOrbit/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/BlazOrbit/BlazOrbit/releases/tag/v1.0.0
