using BlazOrbit.Localization;

// Bundle declarations for BlazOrbit's built-in localized resources. Each marker type already
// exists alongside this file (see `Resources/*.cs`) and ships with `.tn` translations under
// `Translations/`. The source generator (`BlazOrbit.Localization.CodeGeneration`) reads these
// attributes, parses every matching `.tn` file at build time, and emits the runtime
// registration via `[ModuleInitializer]` — bundles are ready before any consumer resolves
// `IStringLocalizer<TResource>`.

[assembly: BobLocalizationBundle(typeof(BlazOrbit.BOBFormsResources), DefaultCulture = "en-US")]
[assembly: BobLocalizationBundle(typeof(BlazOrbit.BOBLayoutResources), DefaultCulture = "en-US")]
[assembly: BobLocalizationBundle(typeof(BlazOrbit.BOBDisplayResources), DefaultCulture = "en-US")]
[assembly: BobLocalizationBundle(typeof(BlazOrbit.BOBDataCollectionResources), DefaultCulture = "en-US")]