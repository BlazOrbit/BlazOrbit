using FluentValidation;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering FluentValidation validators in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers all FluentValidation validators from the assembly containing <typeparamref name="TEntry" />.
        /// </summary>
        /// <typeparam name="TEntry">A type from the assembly to scan.</typeparam>
        /// <param name="lifetime">Service lifetime for resolved validators.</param>
        /// <param name="filter">Optional predicate to filter scanned types.</param>
        /// <param name="includeInternalTypes">Whether to include internal validators.</param>
        /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
        public IServiceCollection AddBOBFluentValidation<TEntry>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
        bool includeInternalTypes = false)
        {
            services.AddValidatorsFromAssemblyContaining<TEntry>(lifetime, filter, includeInternalTypes);
            return services;
        }

        /// <summary>
        /// Registers all FluentValidation validators from the specified <paramref name="assembly" />.
        /// </summary>
        /// <param name="assembly">Assembly to scan. Defaults to the executing assembly when <see langword="null" />.</param>
        /// <param name="lifetime">Service lifetime for resolved validators.</param>
        /// <param name="filter">Optional predicate to filter scanned types.</param>
        /// <param name="includeInternalTypes">Whether to include internal validators.</param>
        /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
        public IServiceCollection AddBOBFluentValidation(
            Assembly? assembly = null,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
            bool includeInternalTypes = false)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            services.AddValidatorsFromAssembly(assembly, lifetime, filter, includeInternalTypes);
            return services;
        }
    }
}
