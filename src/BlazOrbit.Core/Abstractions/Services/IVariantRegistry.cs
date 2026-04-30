using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Abstractions;

public interface IVariantRegistry
{
    RenderFragment? GetTemplate(
        Type componentType,
        Variant variant,
        ComponentBase component);

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