using BlazOrbit.Localization;
using BlazOrbit.Localization.Wasm;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers BlazOrbit localization services for Blazor WebAssembly hosts.</summary>
public static class WasmLocalizationServiceCollectionExtensions
{
    /// <summary>Registers the BlazOrbit localization pipeline and the WASM-side culture persistence service.</summary>
    public static IServiceCollection AddBlazOrbitLocalizationWasm(
        this IServiceCollection services,
        Action<WasmLocalizationSettings>? configure = null)
    {
        WasmLocalizationSettings options = new();
        configure?.Invoke(options);
        services.AddSingleton<LocalizationSettings>(options);
        services.AddSingleton(options);

        // BOBLocalize takes over IStringLocalizer<T> resolution. Built-in providers
        // (Bundle, Literal) plus per-bundle `.tn` data are registered via the
        // `[ModuleInitializer]` emitted by `BlazOrbit.Localization.CodeGeneration` in each
        // assembly that declares `[BobLocalizationBundle]` - by the time any consumer
        // resolves `IStringLocalizer<TResource>`, every bundle is already in place.
        services.AddBlazOrbitLocalization();

        // WASM-specific persistence - culture cookie/localStorage round-trip.
        services.AddScoped<ILocalizationPersistence, WasmLocalizationPersistence>();

        return services;
    }
}

/// <summary>WebAssembly host extensions that apply the persisted culture before the app starts.</summary>
public static class WasmLocalizationHostExtensions
{
    /// <summary>Resolves the persisted culture (or <paramref name="defaultCulture"/>) and assigns it to the current thread.</summary>
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