using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Services;

internal sealed class VariantRegistry : IVariantRegistry
{
    private readonly bool _initialized;
    private readonly Dictionary<(Type, Type, string), Delegate> _templates = [];

    public VariantRegistry(IEnumerable<IVariantRegistryInitializer> initializers)
    {
        foreach (IVariantRegistryInitializer initializer in initializers)
        {
            initializer.Initialize(this);
        }

        _initialized = true;
    }

    public RenderFragment? GetTemplate(
        Type componentType,
        Variant variant,
        ComponentBase component)
    {
        Type? currentType = componentType;

        while (currentType != null && currentType != typeof(ComponentBase))
        {
            (Type currentType, Type, string Name) key = (currentType, variant.GetType(), variant.Name);

            if (_templates.TryGetValue(key, out Delegate? template))
            {
                return template.DynamicInvoke(component) as RenderFragment;
            }

            currentType = currentType.BaseType;
        }

        return null;
    }

    public void Register<TComponent, TVariant>(
            TVariant variant,
        Func<TComponent, RenderFragment> template)
        where TComponent : ComponentBase
        where TVariant : Variant
    {
        if (_initialized)
        {
            throw new InvalidOperationException(
                "Variants must be registered during startup");
        }

        (Type, Type, string Name) key = (typeof(TComponent), typeof(TVariant), variant.Name);
        _templates[key] = template;
    }
}

internal sealed class VariantRegistryInitializer : IVariantRegistryInitializer
{
    private readonly Action<VariantBuilder> _configure;

    public VariantRegistryInitializer(Action<VariantBuilder> configure) => _configure = configure;

    public void Initialize(IVariantRegistry registry)
    {
        VariantBuilder builder = new(registry);
        _configure(builder);
    }
}