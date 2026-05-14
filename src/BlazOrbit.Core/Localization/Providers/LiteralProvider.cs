using System.Globalization;

namespace BlazOrbit.Localization.Providers;

/// <summary>
/// Marker provider that declares the literal-fallback position in a bundle's chain. Always
/// returns <see langword="false"/> — the source literal is carried by the caller (either the
/// generator-emitted accessor method or <see cref="BobLocalizer{T}"/>) and surfaced as
/// <see cref="Microsoft.Extensions.Localization.LocalizedString.ResourceNotFound"/> when no
/// real provider matched.
/// </summary>
/// <remarks>
/// Treating the literal as a "served" value inside this provider would erase the
/// <c>ResourceNotFound</c> diagnostic that consumers rely on to detect untranslated keys. The
/// type stays in the chain as a sentinel so <see cref="BobLocalizationBundleAttribute.Chain"/>
/// can declare it explicitly and the generator can reason about its presence at build time.
/// </remarks>
public sealed class LiteralProvider : IBobLocalizationProvider
{
    /// <inheritdoc />
    public bool TryGet(BobLocalizationBundleSpec spec, ulong hash, CultureInfo culture, out string? value)
    {
        value = null;
        return false;
    }
}