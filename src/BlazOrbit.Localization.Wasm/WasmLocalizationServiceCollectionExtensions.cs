using BlazOrbit.Localization;
using BlazOrbit.Localization.Shared;
using BlazOrbit.Localization.Wasm;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Microsoft.Extensions.DependencyInjection;

public static class WasmLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddBlazOrbitLocalizationWasm(
        this IServiceCollection services,
        Action<WasmLocalizationSettings>? configure = null)
    {
        WasmLocalizationSettings options = new();
        configure?.Invoke(options);
        services.AddSingleton<LocalizationSettings>(options);
        services.AddSingleton(options);

        // Add standard localization
        services.AddLocalization(opts => opts.ResourcesPath = options.ResourcesPath);

        // Reroute IStringLocalizer<T> lookups to the configured *.Translations assemblies.
        services.Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory>(sp =>
        {
            IOptions<LocalizationOptions> localOpts = sp.GetRequiredService<IOptions<LocalizationOptions>>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ResourceManagerStringLocalizerFactory inner = new(localOpts, loggerFactory);
            return new ReroutedStringLocalizerFactory(inner, options.TranslationsAssemblies);
        }));

        // Add WASM-specific persistence
        services.AddScoped<ILocalizationPersistence, WasmLocalizationPersistence>();

        return services;
    }
}

public static class WasmLocalizationHostExtensions
{
    public static async Task<WebAssemblyHost> UseBlazOrbitLocalizationWasm(
        this WebAssemblyHost host,
        string defaultCulture = "en-US")
    {
        ILocalizationPersistence locPersistence = host.Services.GetRequiredService<ILocalizationPersistence>();
        try
        {
            string? storedCulture = await locPersistence.GetStoredCultureAsync();

            if (!string.IsNullOrEmpty(storedCulture))
            {
                CultureInfo culture = new(storedCulture);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            else
            {
                CultureInfo culture = new(defaultCulture);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                await locPersistence.SetStoredCultureAsync(defaultCulture);
            }
        }
        catch
        {
            CultureInfo culture = new(defaultCulture);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        return host;
    }
}
