using System.Globalization;

namespace BlazOrbit.Localization;

/// <summary>
/// Common localization settings shared by the Server and WASM hosting integrations.
/// Concrete packages derive their host-specific subclass — `ServerLocalizationSettings`
/// adds cookie configuration, `WasmLocalizationSettings` is a marker for symmetry —
/// so that consumers referencing both packages can disambiguate without `using` aliases.
/// </summary>
public class LocalizationSettings
{
    public string DefaultCulture { get; set; } = "en-US";

    public string ResourcesPath { get; set; } = "Resources";

    public List<CultureInfo> SupportedCultures { get; set; } =
        [
            new CultureInfo("en-US"),
            new CultureInfo("es-ES")
        ];

    /// <summary>
    /// Maps source-assembly names to translations-assembly names. The configured
    /// <c>IStringLocalizerFactory</c> uses this map to reroute resource lookups
    /// from the assembly that defines a marker type to a separately versioned
    /// <c>*.Translations</c> assembly.
    /// </summary>
    /// <remarks>
    /// Default entries cover BlazOrbit's own resources. Add an entry per consumer
    /// assembly that ships a sidecar <c>*.Translations</c> project — for example,
    /// <c>settings.TranslationsAssemblies["MyApp"] = "MyApp.Translations";</c>.
    /// Keys are case-sensitive simple assembly names (no version, no extension).
    /// </remarks>
    public Dictionary<string, string> TranslationsAssemblies { get; } = new(StringComparer.Ordinal)
    {
        ["BlazOrbit.Localization.Shared"] = "BlazOrbit.Translations",
        ["BlazOrbit"] = "BlazOrbit.Translations",
    };
}
