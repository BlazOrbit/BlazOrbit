using BlazOrbit.Localization;
using BlazOrbit.Localization.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers BlazOrbit localization services for Blazor Server / interactive hosts.</summary>
public static class ServerLocalizationServiceCollectionExtensions
{
    /// <summary>Registers the BlazOrbit localization pipeline, request-localization middleware, and culture-switch endpoint.</summary>
    public static IServiceCollection AddBlazOrbitLocalizationServer(
        this IServiceCollection services,
        Action<ServerLocalizationSettings>? configure = null)
    {
        ServerLocalizationSettings options = new();
        configure?.Invoke(options);
        services.AddSingleton<LocalizationSettings>(options);
        services.AddSingleton(options);

        // BOBLocalize takes over IStringLocalizer<T> resolution. Bundle data lands via
        // [ModuleInitializer]s emitted by `BlazOrbit.Localization.CodeGeneration` in every
        // assembly that declares `[BobLocalizationBundle]`.
        services.AddBlazOrbitLocalization();

        services.AddHttpContextAccessor();

        // Configure request localization
        services.Configure<RequestLocalizationOptions>(opts =>
        {
            opts.DefaultRequestCulture = new RequestCulture(options.DefaultCulture);
            opts.SupportedCultures = options.SupportedCultures;
            opts.SupportedUICultures = options.SupportedCultures;

            // Cookie provider should be first
            opts.RequestCultureProviders.Insert(0,
                new CookieRequestCultureProvider { CookieName = options.CultureCookieName });
        });

        services.AddTransient<IStartupFilter, CultureEndpointStartupFilter>();

        return services;
    }
}