using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Localization;

/// <summary>
/// Fluent builder returned by
/// <see cref="ServiceCollectionExtensions.AddBlazOrbitLocalization"/> so consumers can chain
/// custom provider registrations without re-typing the service collection.
/// </summary>
public interface IBobLocalizationBuilder
{
    /// <summary>The underlying service collection.</summary>
    IServiceCollection Services { get; }

    /// <summary>Registers a custom <see cref="IBobLocalizationProvider"/> with the DI container.</summary>
    /// <typeparam name="TProvider">The provider implementation type.</typeparam>
    /// <param name="lifetime">Service lifetime — defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    IBobLocalizationBuilder AddProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TProvider : class, IBobLocalizationProvider;
}