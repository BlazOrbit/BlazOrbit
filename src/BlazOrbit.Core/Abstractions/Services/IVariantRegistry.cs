using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Abstractions;

/// <summary>Runtime registry of variant render templates contributed from outside a component's own assembly.</summary>
public interface IVariantRegistry
{
    /// <summary>Resolves the registered render fragment for <paramref name="componentType"/> + <paramref name="variant"/>, or null when none is registered.</summary>
    RenderFragment? GetTemplate(
        Type componentType,
        Variant variant,
        ComponentBase component);

    /// <summary>Registers (or overrides) the template used to render <typeparamref name="TComponent"/> for the given variant.</summary>
    void Register<TComponent, TVariant>(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
        where TComponent : ComponentBase
        where TVariant : Variant;
}

internal interface IVariantRegistryInitializer
{
    void Initialize(IVariantRegistry registry);
}