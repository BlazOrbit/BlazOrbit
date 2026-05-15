using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazOrbit.Abstractions;

/// <summary>Variant-aware <see cref="BOBComponentBase"/>. Resolves the active variant template against <see cref="BuiltInTemplates"/> plus any external registrations.</summary>
public abstract class BOBVariantComponentBase<TComponent, TVariant> : BOBComponentBase, IVariantComponent<TVariant>
    where TComponent : BOBVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private RenderFragment? _resolvedTemplate;
    private VariantHelper<TComponent, TVariant>? _variantHelper;

    Variant IVariantComponent.CurrentVariant => CurrentVariant;

    /// <summary>Effective variant for this render (parameter or <see cref="DefaultVariant"/>).</summary>
    public TVariant CurrentVariant => Variant ?? DefaultVariant;

    /// <summary>Variant used when no <see cref="Variant"/> is supplied.</summary>
    public abstract TVariant DefaultVariant { get; }

    /// <summary>Selected variant. <see langword="null"/> falls back to <see cref="DefaultVariant"/>.</summary>
    [Parameter]
    public TVariant? Variant { get; set; }

    Type IVariantComponent.VariantType => typeof(TVariant);

    /// <summary>Compile-time map of variants to their built-in render templates.</summary>
    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }
    [Inject] private IVariantRegistry? VariantRegistry { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        if (_resolvedTemplate is not null)
        {
            builder.AddContent(0, _resolvedTemplate);
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _variantHelper ??= new VariantHelper<TComponent, TVariant>(
            (TComponent)this,
            VariantRegistry);

        Variant ??= DefaultVariant;
        _resolvedTemplate = _variantHelper.ResolveTemplate(Variant, BuiltInTemplates);
    }
}