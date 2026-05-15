using BlazOrbit.Localization.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace BlazOrbit.Localization;

/// <summary>
/// DI registration for the BOBLocalize runtime. The single entry point -
/// <see cref="AddBlazOrbitLocalization"/> - wires the <see cref="IStringLocalizer{T}"/>
/// resolution to <see cref="BobLocalizer{T}"/> and registers the built-in providers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the BOBLocalize runtime. Built-in providers (<see cref="LiteralProvider"/>,
    /// <see cref="BundleProvider"/>) are added as singletons; the
    /// <see cref="IStringLocalizer{T}"/> open generic is reassigned to
    /// <see cref="BobLocalizer{T}"/>. Custom providers register via the returned
    /// <see cref="IBobLocalizationBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Safe to call multiple times - duplicate registrations are idempotent (<c>TryAdd</c>
    /// variants). The actual bundle data comes from each owning assembly's
    /// <c>[ModuleInitializer]</c> emitted by the source generator and registered through
    /// <see cref="BobLocalize.RegisterBundle"/> before any consumer runs.
    /// </remarks>
    public static IBobLocalizationBuilder AddBlazOrbitLocalization(
        this IServiceCollection services,
        Action<BobLocalizationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        BobLocalizationOptions options = new();
        configure?.Invoke(options);

        services.TryAddSingleton(options);

        // Built-in providers - must be available before any chain in any bundle resolves.
        services.TryAddSingleton<LiteralProvider>();
        services.TryAddSingleton<BundleProvider>();

        // The Microsoft `AddLocalization()` extension registers
        // `IStringLocalizer<>` → `StringLocalizer<>` via `Add`, which means a `TryAdd` here
        // would silently no-op. Use `Replace` so a later `AddBlazOrbitLocalization()` call
        // genuinely takes over the resolution regardless of prior registrations.
        services.Replace(new ServiceDescriptor(
            typeof(IStringLocalizer<>),
            typeof(BobLocalizer<>),
            ServiceLifetime.Singleton));

        return new BobLocalizationBuilder(services);
    }

    private sealed class BobLocalizationBuilder : IBobLocalizationBuilder
    {
        public BobLocalizationBuilder(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }

        public IBobLocalizationBuilder AddProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProvider : class, IBobLocalizationProvider
        {
            Services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));
            return this;
        }
    }
}