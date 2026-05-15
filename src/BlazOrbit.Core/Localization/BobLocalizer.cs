using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.Globalization;

namespace BlazOrbit.Localization;

/// <summary>
/// Adapter that exposes the BOBLocalize runtime as a Microsoft
/// <see cref="IStringLocalizer{T}"/>. Components and services continue to inject
/// <c>IStringLocalizer&lt;TResource&gt;</c> exactly as they did with the resx-based stack -
/// the registration in <see cref="ServiceCollectionExtensions.AddBlazOrbitLocalization"/>
/// reroutes that resolution to this adapter.
/// </summary>
/// <remarks>
/// <para>
/// The adapter holds the bundle spec for <typeparamref name="T"/> (looked up lazily from
/// <see cref="BobLocalize"/>) and caches the resolved provider chain per
/// <see cref="IServiceProvider"/> scope to avoid repeated DI walks on hot paths.
/// </para>
/// <para>
/// When the generator is active for the consumer assembly, call sites of the form
/// <c>Loc["literal"]</c> are intercepted into direct accessor methods that bypass this
/// adapter entirely - measured in single-digit nanoseconds. This adapter is the dynamic
/// fallback for runtime-supplied names.
/// </para>
/// </remarks>
/// <typeparam name="T">Bundle marker type - see <see cref="BobLocalizationBundleAttribute"/>.</typeparam>
public sealed class BobLocalizer<T> : IStringLocalizer<T>
{
    private readonly IServiceProvider _services;
    private readonly ConcurrentDictionary<Type, IBobLocalizationProvider> _providerCache = new();
    private BobLocalizationBundleSpec? _spec;

    /// <summary>Creates a localizer scoped to <typeparamref name="T"/>.</summary>
    public BobLocalizer(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
    }

    /// <inheritdoc />
    public LocalizedString this[string name] => Lookup(name, null);

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] => Lookup(name, arguments);

    private LocalizedString Lookup(string name, object[]? args)
    {
        ArgumentNullException.ThrowIfNull(name);

        ulong hash = BobLocalizationHash.Compute(name);
        CultureInfo culture = CultureInfo.CurrentUICulture;
        string? value = ResolveValue(name, hash, culture);

        string final = args is { Length: > 0 } && value is not null
            ? string.Format(culture, value, args)
            : value ?? FormatLiteral(name, args, culture);

        bool found = value is not null;
        return new LocalizedString(name, final, !found);
    }

    private static string FormatLiteral(string literal, object[]? args, CultureInfo culture)
        => args is { Length: > 0 } ? string.Format(culture, literal, args) : literal;

    private string? ResolveValue(string name, ulong hash, CultureInfo culture)
    {
        BobLocalizationBundleSpec? spec = EnsureSpec();

        // Bundle-less marker: degrade to literal fallback.
        if (spec is null)
        {
            return null;
        }

        // Key-routed providers run before the chain.
        foreach (BobLocalizationKeyRoute route in spec.KeyRoutes)
        {
            if (name.StartsWith(route.Prefix, StringComparison.Ordinal))
            {
                IBobLocalizationProvider routed = ResolveProvider(route.ProviderType);
                if (routed.TryGet(spec, hash, culture, out string? routedValue))
                {
                    return routedValue;
                }

                break; // only the first matching route is consulted before the chain
            }
        }

        foreach (Type providerType in spec.Chain)
        {
            IBobLocalizationProvider provider = ResolveProvider(providerType);
            if (provider.TryGet(spec, hash, culture, out string? chainValue))
            {
                return chainValue;
            }
        }

        return null;
    }

    private BobLocalizationBundleSpec? EnsureSpec()
    {
        if (_spec is not null)
        {
            return _spec;
        }

        BobLocalize.TryGetBundle(typeof(T), out _spec);
        return _spec;
    }

    private IBobLocalizationProvider ResolveProvider(Type providerType)
    {
        return _providerCache.GetOrAdd(providerType, t =>
        {
            // Prefer DI-resolved instances (so scoped providers like a DbProvider get the
            // request scope). Fall back to a default constructor for stateless built-ins.
            object? viaDi = _services.GetService(t);
            if (viaDi is IBobLocalizationProvider p)
            {
                return p;
            }

            return (IBobLocalizationProvider)ActivatorUtilities.CreateInstance(_services, t);
        });
    }

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        // The BOBLocalize design discourages enumerating all strings - the surface is intentionally
        // hash-keyed and the source literals live as code. Return what the SourceLiterals table
        // exposes (generator emits one entry per call site) so tooling that depends on this method
        // (e.g. Microsoft.AspNetCore.Mvc localization providers) still gets a meaningful answer.
        BobLocalizationBundleSpec? spec = EnsureSpec();
        if (spec?.SourceLiterals is null)
        {
            yield break;
        }

        CultureInfo culture = CultureInfo.CurrentUICulture;
        foreach (KeyValuePair<ulong, string> kv in spec.SourceLiterals)
        {
            string? value = ResolveValue(kv.Value, kv.Key, culture);
            yield return new LocalizedString(kv.Value, value ?? kv.Value, value is null);
        }
    }
}