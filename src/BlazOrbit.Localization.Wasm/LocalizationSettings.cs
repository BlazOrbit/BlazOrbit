using BaseLocalizationSettings = BlazOrbit.Localization.LocalizationSettings;

namespace BlazOrbit.Localization.Wasm;

/// <summary>
/// WASM-side localization settings. Currently a marker subclass of
/// <see cref="BaseLocalizationSettings"/> — preserves API symmetry with
/// <c>ServerLocalizationSettings</c> and reserves the type name for future
/// WASM-only options without a breaking change.
/// </summary>
public class WasmLocalizationSettings : BaseLocalizationSettings
{
}
