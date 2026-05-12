namespace BlazOrbit.Localization;

/// <summary>
/// Declares a localization bundle anchored to a marker <c>TResource</c> type. Applied at the
/// assembly level (typically on the <c>AssemblyInfo</c> of a package that ships translations)
/// so the source generator and runtime registry both discover the bundle without reflection
/// in consumer code.
/// </summary>
/// <remarks>
/// <para>
/// The marker type itself has no members — it acts as a strongly-typed key for
/// <c>IStringLocalizer&lt;TResource&gt;</c> injection. Each marker type can have at most one
/// bundle attribute; a single assembly may declare multiple bundles by repeating the attribute
/// with different marker types.
/// </para>
/// <para>
/// <see cref="DefaultCulture"/> declares the culture in which the source literals are written
/// — the source generator hashes those literals, and the runtime fallback chain terminates at
/// that culture (returning the literal itself when nothing else matches).
/// </para>
/// <para>
/// <see cref="Chain"/> is an ordered list of provider <see cref="Type"/>s implementing
/// <see cref="IBobLocalizationProvider"/>. The runtime iterates the chain in declaration order;
/// the first provider that returns <see langword="true"/> wins. Include
/// <c>typeof(LiteralProvider)</c> as the last entry to guarantee the source literal as the
/// terminal fallback.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class BobLocalizationBundleAttribute : Attribute
{
    /// <summary>Marker type used as <c>TResource</c> for <c>IStringLocalizer&lt;TResource&gt;</c> injections.</summary>
    public Type ResourceType { get; }

    /// <summary>Culture of the source literals in code. Defaults to <c>en-US</c>.</summary>
    public string DefaultCulture { get; init; } = "en-US";

    /// <summary>
    /// Ordered provider types. The runtime resolves each via <see cref="IServiceProvider"/> at
    /// first use, then caches the resolved instance. Leave empty to use the default chain of
    /// <c>[BundleProvider, LiteralProvider]</c>.
    /// </summary>
    public Type[] Chain { get; init; } = [];

    /// <summary>
    /// Folder (relative to the project root) where the source generator looks for <c>.tn</c>
    /// translation files. Defaults to <c>Translations</c>.
    /// </summary>
    public string TranslationsFolder { get; init; } = "Translations";

    /// <summary>Initialises the attribute for the supplied marker type.</summary>
    public BobLocalizationBundleAttribute(Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ResourceType = resourceType;
    }
}