using System.Collections.Frozen;

namespace BlazOrbit.Localization;

/// <summary>
/// Immutable specification of a registered localization bundle. Built either by hand for
/// runtime-only scenarios or — typically — by the source generator and supplied to
/// <see cref="BobLocalize.RegisterBundle"/> from a <c>[ModuleInitializer]</c>-marked method
/// inside the bundle's owning assembly.
/// </summary>
/// <param name="ResourceType">Marker type used by <c>IStringLocalizer&lt;TResource&gt;</c>.</param>
/// <param name="DefaultCulture">Culture of the source literals in code (terminal fallback).</param>
/// <param name="Chain">Ordered provider types iterated by <see cref="BobLocalizer{T}"/>.</param>
/// <param name="KeyRoutes">
/// Optional prefix routes; when a key matches, the listed provider is consulted before
/// <paramref name="Chain"/>.
/// </param>
/// <param name="Translations">
/// Per-culture hash → translation tables baked at compile time. Read by the built-in
/// <c>BundleProvider</c>. <see langword="null"/> when no <c>.tn</c> files were supplied —
/// the bundle still works, but every lookup falls through to the source literal.
/// </param>
/// <param name="SourceLiterals">
/// Aggregated hash → source literal table used for diagnostics and the dynamic-key slow path.
/// Generator emits this from every <c>Loc[literal]</c> call site found in the assembly.
/// </param>
public sealed record BobLocalizationBundleSpec(
    Type ResourceType,
    string DefaultCulture,
    IReadOnlyList<Type> Chain,
    IReadOnlyList<BobLocalizationKeyRoute> KeyRoutes,
    FrozenDictionary<string, FrozenDictionary<ulong, string>>? Translations,
    FrozenDictionary<ulong, string>? SourceLiterals);

/// <summary>
/// Single key-routing rule: keys with <see cref="Prefix"/> consult <see cref="ProviderType"/>
/// before the chain.
/// </summary>
/// <param name="Prefix">Literal key prefix.</param>
/// <param name="ProviderType">Provider type consulted on prefix match.</param>
public sealed record BobLocalizationKeyRoute(string Prefix, Type ProviderType);