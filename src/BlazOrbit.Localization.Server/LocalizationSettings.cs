using BaseLocalizationSettings = BlazOrbit.Localization.LocalizationSettings;

namespace BlazOrbit.Localization.Server;

/// <summary>
/// Server-side localization settings. Adds the cookie name used by
/// `CookieRequestCultureProvider` to persist the selected culture across requests
/// and the path of the BlazOrbit culture-setting endpoint.
/// </summary>
public class ServerLocalizationSettings : BaseLocalizationSettings
{
    /// <summary>
    /// Cookie name used by the request-localization middleware. Default: <c>.BlazOrbit.Culture</c>.
    /// </summary>
    public string CultureCookieName { get; set; } = ".BlazOrbit.Culture";

    /// <summary>
    /// Path of the BlazOrbit culture-setting endpoint registered by
    /// <c>CultureEndpointStartupFilter</c>. Default: <c>/BlazOrbit/Culture/Set</c> —
    /// prefixed with <c>/BlazOrbit</c> to minimize collisions with consumer routes
    /// (a plain <c>/Culture/Set</c> is a likely controller name).
    /// </summary>
    public string CultureEndpointPath { get; set; } = "/BlazOrbit/Culture/Set";
}
