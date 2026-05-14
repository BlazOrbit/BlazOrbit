using System.Globalization;

namespace BlazOrbit.Localization;

/// <summary>
/// Pluggable backend that resolves a translation for a precomputed key hash. Implementations
/// hold their own per-culture cache; the framework never wraps them in an external cache layer.
/// </summary>
/// <remarks>
/// <para>
/// The contract is intentionally minimal — one method, one lookup. The build-time source
/// generator wires call sites to the provider chain declared on each
/// <see cref="BobLocalizationBundleAttribute"/>; at runtime the chain is iterated in order
/// and the first provider that returns <see langword="true"/> wins.
/// </para>
/// <para>
/// Cache key inside the provider is the <c>hash</c> only. Providers that need per-culture
/// sub-caches MUST select the sub-cache by <c>culture</c> before hashing — the framework
/// guarantees no two distinct call sites share a hash within a bundle.
/// </para>
/// <para>
/// Built-in implementations: <see cref="Providers.BundleProvider"/> (translations baked at
/// compile time from <c>.tn</c> files) and <see cref="Providers.LiteralProvider"/> (sentinel
/// that resolves to the source literal — terminal fallback, never fails).
/// </para>
/// </remarks>
public interface IBobLocalizationProvider
{
    /// <summary>
    /// Attempts to resolve a translation for the given <paramref name="hash"/> in the
    /// requested <paramref name="culture"/>.
    /// </summary>
    /// <param name="hash">FNV-1a 64-bit hash of the source literal, precomputed at build time.</param>
    /// <param name="culture">Culture to resolve against; provider applies its own fallback rules within.</param>
    /// <param name="value">Receives the translation when this provider has a hit; <see langword="null"/> otherwise.</param>
    /// <returns><see langword="true"/> when the provider served a value; <see langword="false"/> to defer to the next provider in the chain.</returns>
    bool TryGet(ulong hash, CultureInfo culture, out string? value);
}