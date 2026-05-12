using System.Collections.Concurrent;
using System.Globalization;

namespace BlazOrbit.Localization;

/// <summary>
/// Global registry of localization bundles. Each bundle's owning assembly registers itself via
/// a <c>[ModuleInitializer]</c>-marked method emitted by the source generator; consumer code
/// rarely touches this type directly.
/// </summary>
/// <remarks>
/// <para>
/// The registry is process-wide. Bundles are immutable once registered; re-registering the same
/// <c>TResource</c> replaces the previous entry (handy in hot-reload / test contexts, never
/// expected in production).
/// </para>
/// <para>
/// Culture changes are signalled via <see cref="CultureChanged"/> so consumer caches (e.g.
/// rendered output) can invalidate. The culture itself is read from
/// <see cref="CultureInfo.CurrentUICulture"/> — this type does not own the active culture.
/// </para>
/// </remarks>
public static class BobLocalize
{
    private static readonly ConcurrentDictionary<Type, BobLocalizationBundleSpec> _bundles = new();

    /// <summary>
    /// Raised after the active culture changes. Consumers — typically components rendering
    /// localized strings — subscribe to re-render on switch.
    /// </summary>
    public static event Action<CultureInfo>? CultureChanged;

    /// <summary>
    /// Registers a bundle. The source generator emits a <c>[ModuleInitializer]</c> method that
    /// calls this when the owning assembly loads, so bundles are available before any consumer
    /// resolves <c>IStringLocalizer&lt;TResource&gt;</c>.
    /// </summary>
    public static void RegisterBundle(BobLocalizationBundleSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        _bundles[spec.ResourceType] = spec;
    }

    /// <summary>Attempts to resolve a registered bundle by its marker type.</summary>
    public static bool TryGetBundle(Type resourceType, out BobLocalizationBundleSpec? spec)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        return _bundles.TryGetValue(resourceType, out spec);
    }

    /// <summary>Snapshot of every registered bundle. Read-only.</summary>
    public static IReadOnlyCollection<BobLocalizationBundleSpec> AllBundles
        => _bundles.Values.ToArray();

    /// <summary>
    /// Notifies subscribers that the active UI culture changed. Called by the localization
    /// host integration (Server cookie endpoint, Wasm localStorage handler) after updating
    /// <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    public static void NotifyCultureChanged(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        CultureChanged?.Invoke(culture);
    }

    /// <summary>Removes a registered bundle. Used by tests; not expected in production.</summary>
    public static bool UnregisterBundle(Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        return _bundles.TryRemove(resourceType, out _);
    }

    /// <summary>Clears all registered bundles. Tests only.</summary>
    internal static void ClearAllBundles() => _bundles.Clear();
}