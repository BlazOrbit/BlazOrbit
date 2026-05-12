using System.Collections.Frozen;
using System.Globalization;

namespace BlazOrbit.Localization.Providers;

/// <summary>
/// Built-in provider that resolves a hash against the per-culture translation tables baked
/// into each <see cref="BobLocalizationBundleSpec"/> by the source generator. Stateless,
/// thread-safe, singleton.
/// </summary>
/// <remarks>
/// <para>
/// Lookup strategy per call: walk the registered bundles, and for each test the requested
/// culture, its parent chain, and finally the bundle's <c>DefaultCulture</c>. The first
/// matching entry wins.
/// </para>
/// <para>
/// In the fast path (call sites the source generator can resolve at build time) the accessor
/// method skips this provider entirely — the generator emits a per-bundle static class with a
/// direct <see cref="System.Collections.Frozen.FrozenDictionary{TKey, TValue}"/> lookup. This
/// runtime provider serves the dynamic-key slow path and the bundle-discovery fallback.
/// </para>
/// </remarks>
public sealed class BundleProvider : IBobLocalizationProvider
{
    /// <inheritdoc />
    public bool TryGet(ulong hash, CultureInfo culture, out string? value)
    {
        ArgumentNullException.ThrowIfNull(culture);

        foreach (BobLocalizationBundleSpec spec in BobLocalize.AllBundles)
        {
            if (TryLookupInBundle(spec, hash, culture, out value))
            {
                return true;
            }
        }

        value = null;
        return false;
    }

    private static bool TryLookupInBundle(
        BobLocalizationBundleSpec spec,
        ulong hash,
        CultureInfo culture,
        out string? value)
    {
        if (spec.Translations is null)
        {
            value = null;
            return false;
        }

        // Walk the culture's ancestry. CultureInfo.Parent on the invariant culture returns
        // the invariant culture itself — break out when we encounter that sentinel.
        CultureInfo? c = culture;
        while (c is not null && c.Name.Length > 0)
        {
            if (spec.Translations.TryGetValue(c.Name, out FrozenDictionary<ulong, string>? byHash) &&
                byHash.TryGetValue(hash, out value))
            {
                return true;
            }

            CultureInfo parent = c.Parent;
            if (ReferenceEquals(parent, c))
            {
                break;
            }

            c = parent;
        }

        // Bundle default culture as the terminal step (skip if already tried).
        if (!string.Equals(spec.DefaultCulture, culture.Name, StringComparison.OrdinalIgnoreCase) &&
            spec.Translations.TryGetValue(spec.DefaultCulture, out FrozenDictionary<ulong, string>? byHashDefault) &&
            byHashDefault.TryGetValue(hash, out value))
        {
            return true;
        }

        value = null;
        return false;
    }
}