using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Localization;
using BlazOrbit.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazOrbit(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);

        // BOBLocalize runtime — registers IStringLocalizer<T> => BobLocalizer<T> so the ~35
        // built-in components that inject IStringLocalizer<BOBFormsResources> /
        // <BOBLayoutResources> / ... resolve without the consumer having to call
        // AddBlazOrbitLocalizationServer/Wasm explicitly. BobLocalizer falls back to literal
        // when no [BobLocalizationBundle] spec is registered for the marker, so the no-loc
        // template path stays functional. AddBlazOrbitLocalizationServer/Wasm later layer
        // request/wasm-specific wiring on top of this idempotent base.
        services.AddBlazOrbitLocalization();

        // Un solo registry para todo
        services.AddSingleton<IVariantRegistry, VariantRegistry>();

        // JS interop
        services.AddScoped<IThemeJsInterop, ThemeJsInterop>();
        services.AddScoped<IBehaviorJsInterop, BehaviorJsInterop>();
        services.AddScoped<IPatternJsInterop, PatternJsInterop>();
        services.AddScoped<IDropdownJsInterop, DropdownJsInterop>();
        services.AddScoped<IClipboardJsInterop, ClipboardJsInterop>();
        services.AddScoped<ITextAreaJsInterop, TextAreaJsInterop>();
        services.AddScoped<IDraggableJsInterop, DraggableJsInterop>();
        services.AddScoped<IColorPickerJsInterop, ColorPickerJsInterop>();
        services.AddScoped<ISliderJsInterop, SliderJsInterop>();
        services.AddScoped<ICarouselJsInterop, CarouselJsInterop>();

        // Modal
        services.AddScoped<IModalService, ModalService>();
        services.AddScoped<IModalJsInterop, ModalJsInterop>();

        // Toast
        services.AddScoped<IToastService, ToastService>();

        // Confirm — themed wrapper over IModalService. Zero JS bundle so it ships with
        // the core registration; opt-out simply by ignoring the IConfirmService injection.
        services.AddScoped<IConfirmService, ConfirmService>();

        // Data-collection state persistence — default no-op so grids without a
        // PersistenceKey behave as before. Consumers opt in to localStorage-backed
        // persistence by registering LocalStorageStatePersistence on top of this entry.
        services.AddScoped<IDataCollectionStatePersistence, NullStatePersistence>();

#if DEBUG
        services.AddSingleton<IBOBPerformanceService, BOBPerformanceService>();
#endif

        return services;
    }

    /// <summary>
    /// Replaces the default <see cref="NullStatePersistence"/> with
    /// <see cref="LocalStorageStatePersistence"/>. Call after <c>AddBlazOrbit()</c> to
    /// opt every grid in to <c>localStorage</c>-backed state persistence whenever a
    /// <c>PersistenceKey</c> parameter is set on the grid.
    /// </summary>
    public static IServiceCollection AddBlazOrbitDataCollectionLocalStorage(this IServiceCollection services)
    {
        services.AddScoped<IDataCollectionStatePersistence, LocalStorageStatePersistence>();
        return services;
    }

    public static IServiceCollection AddBlazOrbitVariants(
        this IServiceCollection services,
        Action<VariantBuilder> configure)
    {
        services.AddSingleton<IVariantRegistryInitializer>(
            new VariantRegistryInitializer(configure));

        return services;
    }
}

#region Variant Registry Builders

/// <summary>
/// Builder for registering variants for a specific component type
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ComponentVariantBuilder<TComponent>
    where TComponent : ComponentBase
{
    private readonly IVariantRegistry _registry;

    internal ComponentVariantBuilder(IVariantRegistry registry) => _registry = registry;

    /// <summary>
    /// Registers a custom variant with its render template
    /// </summary>
    /// <typeparam name="TVariant">The variant type</typeparam>
    /// <param name="variant">The variant instance</param>
    /// <param name="template">The render function for this variant</param>
    /// <returns>This builder for chaining</returns>
    public ComponentVariantBuilder<TComponent> AddVariant<TVariant>(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
        where TVariant : Variant
    {
        _registry.Register(variant, template);
        return this;
    }
}

/// <summary>
/// Builder for registering custom component variants
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class VariantBuilder
{
    private readonly IVariantRegistry _registry;

    internal VariantBuilder(IVariantRegistry registry) => _registry = registry;

    /// <summary>
    /// Start configuration for a specific component type
    /// </summary>
    /// <typeparam name="TComponent">The component type to configure variants for</typeparam>
    /// <returns>A builder for the component variants</returns>
    /// <example>
    /// <code>
    /// builder.ForComponent&lt;BOBButton&gt;()
    ///     .AddVariant(MyCustomVariants.Gradient, button => ...)
    ///     .AddVariant(MyCustomVariants.Ghost, button => ...);
    /// </code>
    /// </example>
    public ComponentVariantBuilder<TComponent> ForComponent<TComponent>()
        where TComponent : ComponentBase => new(_registry);
}

#endregion