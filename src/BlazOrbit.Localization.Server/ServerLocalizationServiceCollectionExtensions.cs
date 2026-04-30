using BlazOrbit.Localization;
using BlazOrbit.Localization.Server;
using BlazOrbit.Localization.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServerLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddBlazOrbitLocalizationServer(
        this IServiceCollection services,
        Action<ServerLocalizationSettings>? configure = null)
    {
        ServerLocalizationSettings options = new();
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

        // Add Server-specific services
        services.AddHttpContextAccessor();

        // Configure request localization
        services.Configure<RequestLocalizationOptions>(opts =>
        {
            opts.DefaultRequestCulture = new RequestCulture(options.DefaultCulture);
            opts.SupportedCultures = options.SupportedCultures;
            opts.SupportedUICultures = options.SupportedCultures;

            // Cookie provider should be first
            opts.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
            {
                CookieName = options.CultureCookieName
            });
        });

        services.AddTransient<IStartupFilter, CultureEndpointStartupFilter>();

        return services;
    }
}
