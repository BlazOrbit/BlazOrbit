using BlazOrbit.Localization;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

/// <summary>
/// Snapshot + restore helper for the process-global <see cref="BobLocalize"/> registry.
/// Tests register fake bundles via <see cref="RegisterFake"/> instead of calling
/// <see cref="BobLocalize.RegisterBundle"/> directly; on <see cref="Dispose"/> only those
/// fakes are removed (any prior registration for the same type is restored). The registry
/// is never cleared — clearing would race against component tests in parallel collections
/// that depend on <c>[ModuleInitializer]</c>-registered production bundles.
///
/// Lazy-loaded module initializers also matter: a sibling test may load an assembly mid-run
/// and register a new production bundle. We must never unregister a type the current test
/// did not register itself.
/// </summary>
internal sealed class BobLocalizeStateSnapshot : IDisposable
{
    private readonly Dictionary<Type, BobLocalizationBundleSpec?> _displaced = [];

    /// <summary>
    /// Registers a fake bundle for the duration of the test. On dispose the prior entry
    /// (or absence) for <paramref name="spec"/>'s ResourceType is restored.
    /// </summary>
    public void RegisterFake(BobLocalizationBundleSpec spec)
    {
        if (!_displaced.ContainsKey(spec.ResourceType))
        {
            _displaced[spec.ResourceType] =
                BobLocalize.TryGetBundle(spec.ResourceType, out BobLocalizationBundleSpec? prior)
                    ? prior
                    : null;
        }

        BobLocalize.RegisterBundle(spec);
    }

    public void Dispose()
    {
        foreach (KeyValuePair<Type, BobLocalizationBundleSpec?> kv in _displaced)
        {
            if (kv.Value is null)
            {
                BobLocalize.UnregisterBundle(kv.Key);
            }
            else
            {
                BobLocalize.RegisterBundle(kv.Value);
            }
        }
    }
}