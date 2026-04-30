using Microsoft.Extensions.Localization;

namespace BlazOrbit.Localization.Shared;

/// <summary>
/// An <see cref="IStringLocalizerFactory"/> wrapper that redirects resource lookups
/// from a source assembly to a sidecar <c>*.Translations</c> assembly.
/// </summary>
/// <remarks>
/// <para>
/// The default <see cref="ResourceManagerStringLocalizerFactory"/> resolves
/// <c>IStringLocalizer&lt;T&gt;</c> by reading <c>typeof(T).Assembly</c> and looking
/// for an embedded resource manifest under <c>{T.Namespace}.{ResourcesPath}.{T.Name}</c>.
/// That couples the .resx files to the same assembly that defines <c>T</c>.
/// </para>
/// <para>
/// This factory keeps the consumer-facing API identical — components still inject
/// <c>IStringLocalizer&lt;BOBCultureSelectorResources&gt;</c> (or any other anchor type) —
/// but the resource manifest is loaded from a separately versioned translations assembly.
/// The mapping is configured via <c>LocalizationSettings.TranslationsAssemblies</c>:
/// keys are source-assembly simple names, values are target-assembly simple names.
/// </para>
/// <para>
/// For an anchor type <c>Foo.Bar.MyType</c> in source assembly <c>Foo.Bar</c> mapped to
/// translations assembly <c>Foo.Bar.Translations</c>, the rewritten lookup becomes
/// <c>Foo.Bar.Translations.MyType</c>. After the inner factory adds the
/// <see cref="LocalizationOptions.ResourcesPath"/> prefix, the final manifest name is
/// <c>Foo.Bar.Translations.{ResourcesPath}.MyType</c> — so the translations project
/// places its .resx under <c>{ResourcesPath}/MyType.resx</c> with no special pinning.
/// </para>
/// </remarks>
public sealed class ReroutedStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IStringLocalizerFactory _inner;
    private readonly IReadOnlyDictionary<string, string> _assemblyMap;

    public ReroutedStringLocalizerFactory(
        IStringLocalizerFactory inner,
        IReadOnlyDictionary<string, string> assemblyMap)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(assemblyMap);
        _inner = inner;
        _assemblyMap = assemblyMap;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        string? sourceAsm = resourceSource.Assembly.GetName().Name;
        if (sourceAsm is null
            || !_assemblyMap.TryGetValue(sourceAsm, out string? translationsAsm)
            || string.IsNullOrEmpty(translationsAsm))
        {
            return _inner.Create(resourceSource);
        }

        string fullName = resourceSource.FullName ?? resourceSource.Name;
        string remainder = fullName.StartsWith(sourceAsm + ".", StringComparison.Ordinal)
            ? fullName[(sourceAsm.Length + 1)..]
            : fullName;

        string rewritten = $"{translationsAsm}.{remainder}";
        return _inner.Create(rewritten, translationsAsm);
    }

    public IStringLocalizer Create(string baseName, string location)
        => _inner.Create(baseName, location);
}
