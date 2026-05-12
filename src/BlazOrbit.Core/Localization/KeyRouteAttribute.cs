namespace BlazOrbit.Localization;

/// <summary>
/// Routes any localization key starting with <see cref="Prefix"/> to a specific provider type
/// before the rest of the bundle's <see cref="BobLocalizationBundleAttribute.Chain"/>. Stacks
/// on the same assembly as the bundle declaration; one attribute per prefix.
/// </summary>
/// <remarks>
/// Use to mix back-ends inside a single bundle without splitting it into multiple marker types.
/// Example: keys starting with <c>"cms:"</c> route to a database-backed provider while the rest
/// fall through to the default chain (typically static bundles + literal fallback).
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class BobLocalizationKeyRouteAttribute : Attribute
{
    /// <summary>Marker resource type the route applies to.</summary>
    public Type ResourceType { get; }

    /// <summary>Literal prefix that triggers the route. Compared with <see cref="StringComparison.Ordinal"/>.</summary>
    public string Prefix { get; }

    /// <summary>Provider type consulted first when a key matches <see cref="Prefix"/>.</summary>
    public Type ProviderType { get; }

    /// <summary>Initialises a new route declaration.</summary>
    public BobLocalizationKeyRouteAttribute(Type resourceType, string prefix, Type providerType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(providerType);
        if (prefix.Length == 0)
        {
            throw new ArgumentException("Prefix must not be empty.", nameof(prefix));
        }

        ResourceType = resourceType;
        Prefix = prefix;
        ProviderType = providerType;
    }
}