namespace BlazOrbit.Components;

/// <summary>
/// Base contract for components that support visual variants.
/// </summary>
public interface IVariantComponent
{
    /// <summary>Currently resolved variant instance.</summary>
    Variant CurrentVariant { get; }

    /// <summary>Type of variant supported by this component.</summary>
    Type VariantType { get; }
}

/// <summary>
/// Strongly-typed contract for components that support a specific <typeparamref name="TVariant" />.
/// </summary>
/// <typeparam name="TVariant">Concrete variant type.</typeparam>
public interface IVariantComponent<TVariant> : IVariantComponent
    where TVariant : Variant
{
    /// <summary>Currently resolved variant instance.</summary>
    new TVariant CurrentVariant { get; }

    /// <summary>Default variant used when no explicit variant is specified.</summary>
    TVariant DefaultVariant { get; }

    /// <summary>Explicitly requested variant, or <see langword="null"/> to use <see cref="DefaultVariant"/>.</summary>
    TVariant? Variant { get; set; }
}
