using System.Globalization;

namespace BlazOrbit.Localization;

/// <summary>
/// Common host-side localization settings shared by the Server and WASM integrations.
/// Concrete packages derive their host-specific subclass — `ServerLocalizationSettings`
/// adds cookie configuration, `WasmLocalizationSettings` is a marker for symmetry —
/// so that consumers referencing both packages can disambiguate without `using` aliases.
/// </summary>
public class LocalizationSettings
{
    /// <summary>
    /// Default UI culture applied when no host-side persistence (cookie / localStorage)
    /// has recorded a previous selection.
    /// </summary>
    public string DefaultCulture { get; set; } = "en-US";

    /// <summary>Cultures the host advertises as supported (for negotiation + the UI selector).</summary>
    public List<CultureInfo> SupportedCultures { get; set; } =
    [
        new("en-US"),
        new("es-ES")
    ];
}