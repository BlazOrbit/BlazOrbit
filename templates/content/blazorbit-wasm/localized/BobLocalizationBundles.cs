using BlazOrbit.Localization;

// Bundle declarations for the template's localized resources.
// The source generator reads these attributes and emits runtime
// registration via [ModuleInitializer] so bundles are available
// before any consumer resolves IStringLocalizer<T>.

[assembly: BobLocalizationBundle(typeof(BlazorApp.Layout.NavMenu), DefaultCulture = "en-US")]
[assembly: BobLocalizationBundle(typeof(BlazorApp.Pages.LocalizationDemo), DefaultCulture = "en-US")]
