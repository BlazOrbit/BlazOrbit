using BlazOrbit.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

internal sealed class VariantHelper<TComponent, TVariant>
    where TComponent : ComponentBase
    where TVariant : Variant
{
    private readonly TComponent _component;
    private readonly IVariantRegistry? _registry;

    public VariantHelper(TComponent component, IVariantRegistry? registry)
    {
        _component = component;
        _registry = registry;
    }

    public RenderFragment? ResolveTemplate(
        TVariant variant,
        IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>>? builtInTemplates)
    {
        return builtInTemplates?.TryGetValue(variant, out Func<TComponent, RenderFragment>? builtIn) == true
            ? builtIn(_component)
            : (_registry?.GetTemplate(_component.GetType(), variant, _component));
    }
}