using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;

namespace BlazOrbit.Abstractions;

// Base class without variants
public abstract class BOBInputComponentBase<TValue> :
    InputBase<TValue>,
    IAsyncDisposable,
    IBuiltComponent,
    IHasReadOnly,
    IHasDisabled,
    IHasRequired,
    IHasError
{
    // Shared style + JS-behavior pipeline. InputBase<TValue> is a mandatory ancestor for
    // EditContext/ValueExpression participation, so this class cannot inherit BOBComponentBase
    // directly. Composing the pipeline instead of re-implementing it keeps the two base classes
    // from drifting apart.
    private readonly BOBComponentPipeline _pipeline = new();
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _previousEditContext;
    private bool _lastValidationError;
    // Cache the synthetic ValueExpression for the no-EditContext path. The expression
    // captures `this`, so a single instance is valid for the component's lifetime and
    // re-uses the same Expression<Func<TValue>> across every SetParametersAsync call
    // instead of rebuilding it.
    private Expression<Func<TValue>>? _valueExpressionFallback;

    // Echo-guard. After a parent receives ValueChanged and re-renders, it pushes the
    // same Value back down. Without intervention every keystroke triggers a redundant
    // BuildRenderTree on this component (and cascades through the parent tree). The
    // guard distinguishes parameter-change paths (SetParametersAsync) from explicit
    // StateHasChanged paths (focus/blur, validation flips, derived-component private
    // state), and on the parameter path suppresses the render when both the bound
    // Value matches what was last rendered AND the style fingerprint cache hit (i.e.
    // no style-affecting parameter changed either).
    private TValue? _lastRenderedValue;
    private bool _hasRenderedOnce;
    private bool _isFromParameterChange;

    // Common parameters for all inputs — "force from outside": parent overrides the
    // computed state. The computed truth lives in IsX below.
    /// <summary>When <see langword="true" />, the input is disabled. Combined with internal state via <see cref="IsDisabled"/>.</summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>When <see langword="true" />, the input is read-only. Combined with internal state via <see cref="IsReadOnly"/>.</summary>
    [Parameter] public bool ReadOnly { get; set; }
    /// <summary>When <see langword="true" />, the input is marked required for validation. Combined with internal state via <see cref="IsRequired"/>.</summary>
    [Parameter] public bool Required { get; set; }
    /// <summary>When <see langword="true" />, the input is forced into the error state. Combined with <c>EditContext</c> validation via <see cref="IsError"/>.</summary>
    [Parameter] public bool Error { get; set; }

    // Computed states — source of truth for gating, aria-* and the attributes builder.
    // IsDisabled is virtual so derived inputs can decouple Loading from Disabled — for
    // example, a debounced search input that wants to show a spinner while still
    // accepting keystrokes overrides this to drop the IHasLoading branch.
    public virtual bool IsDisabled => Disabled || (this is IHasLoading loading && loading.Loading);
    public bool IsError => Error || _lastValidationError;
    public bool IsReadOnly => ReadOnly;
    public bool IsRequired => Required;

    // See BOBComponentBase.ComputedAttributes for why this is `public`: variant templates live
    // cross-assembly and need to spread this dictionary onto the `<bob-component>` root.
    public Dictionary<string, object> ComputedAttributes => _pipeline.ComputedAttributes;

    /// <summary>
    /// `true` once <see cref="Dispose(bool)"/> / <see cref="DisposeAsync"/> has started. See
    /// BOBComponentBase.IsDisposed for the contract — gate post-await continuations in derived
    /// components on this flag.
    /// </summary>
    protected bool IsDisposed { get; set; }

    [Inject] private IBehaviorJsInterop BehaviorJsInterop { get; set; } = default!;

#if DEBUG
    [Inject] private IBOBPerformanceService? PerformanceService { get; set; }

    /// <summary>DEBUG-only opt-out from performance instrumentation. Set <see langword="false"/> to skip stopwatch tracking on this instance.</summary>
    [Parameter]
#pragma warning disable RS0016 // Add public types and members to the declared API
    public bool TrackPerformanceEnabled { get; set; } = true;
#pragma warning restore RS0016 // Add public types and members to the declared API
#endif

    public override Task SetParametersAsync(ParameterView parameters)
    {
        // Flag the parameter-change path so ShouldRender can distinguish it from
        // explicit StateHasChanged calls (focus/blur, validation flip, derived
        // component private state). Cleared inside ShouldRender after consumption.
        _isFromParameterChange = true;

        bool hasValueExpression = false;
        bool hasEditContext = false;
        foreach (ParameterValue p in parameters)
        {
            if (p.Name == nameof(ValueExpression))
            {
                hasValueExpression = true;
            }
            else if (p.Cascading && p.Value is EditContext)
            {
                hasEditContext = true;
            }
        }

        if (hasValueExpression || hasEditContext)
        {
            return base.SetParametersAsync(parameters);
        }

        _valueExpressionFallback ??= () => Value!;

        Dictionary<string, object?> patched = [];
        foreach (ParameterValue p in parameters)
        {
            patched[p.Name] = p.Value;
        }

        patched[nameof(ValueExpression)] = _valueExpressionFallback;
        return base.SetParametersAsync(ParameterView.FromDictionary(patched));
    }

    /// <summary>
    /// Echo-guard. Suppresses redundant render-tree rebuilds triggered by the
    /// <c>ValueChanged → parent → SetParametersAsync</c> round-trip when nothing
    /// observable changed. Fires only on the parameter-change path; explicit
    /// <c>StateHasChanged</c> calls (focus/blur, validation flips, derived-component
    /// private state, JS-driven updates) always render.
    /// </summary>
    protected override bool ShouldRender()
    {
        bool wasParameterChange = _isFromParameterChange;
        _isFromParameterChange = false;

        if (!wasParameterChange || !_hasRenderedOnce)
        {
            return true;
        }

        bool valueUnchanged = EqualityComparer<TValue?>.Default.Equals(CurrentValue, _lastRenderedValue);
        bool stylesUnchanged = _pipeline.LastBuildSkipped;

        return !(valueUnchanged && stylesUnchanged);
    }

    public virtual void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
    { }

    public virtual void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
    { }

    /// <summary>
    /// Async disposal path. Blazor invokes this first when the component is unmounted
    /// because the type implements <see cref="IAsyncDisposable"/>; it tears down the
    /// JS-side behavior instance via <see cref="BOBComponentPipeline.DisposeBehaviorAsync"/>.
    /// </summary>
    /// <remarks>
    /// Both this method and <see cref="Dispose(bool)"/> run on disposal — Blazor calls
    /// <c>DisposeAsync</c> for the async work and the framework's <see cref="IDisposable"/>
    /// contract still invokes <c>Dispose(true)</c> afterward. The split is intentional:
    /// async work (JS interop teardown) lives here, sync work (event unsubscribe) lives in
    /// <c>Dispose(true)</c>. Both set <see cref="IsDisposed"/> so derived components can
    /// guard post-await continuations regardless of which path runs first.
    /// </remarks>
    public virtual ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return _pipeline.DisposeBehaviorAsync();
    }

    /// <summary>
    /// Sync disposal path inherited from <see cref="InputBase{TValue}"/>. Unsubscribes
    /// the <see cref="EditContext"/> validation handler so the next component instance
    /// does not double-fire validation. JS-side cleanup belongs in <see cref="DisposeAsync"/>.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            IsDisposed = true;
            EditContext?.OnValidationStateChanged -= HandleValidationStateChanged;
        }

        base.Dispose(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
#if DEBUG
            _pipeline.EndInit(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
            if (IsDisposed)
            {
                return;
            }

            await _pipeline.AttachBehaviorAsync(this, BehaviorJsInterop);
            if (IsDisposed)
            {
                await _pipeline.DisposeBehaviorAsync();
                return;
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        _pipeline.BeginRenderTree();
        _pipeline.PatchVolatileAttributes(this);
        base.BuildRenderTree(builder);

        // Snapshot the rendered Value so the next parameter-change can detect a
        // Value echo. Captured *after* base.BuildRenderTree so derived razors that
        // read `CurrentValue` during render see the same value the guard will
        // compare against.
        _lastRenderedValue = CurrentValue;
        _hasRenderedOnce = true;
#if DEBUG
        _pipeline.EndRenderTree(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
    }

    protected override void OnInitialized()
    {
        _pipeline.BeginInit();
        base.OnInitialized();

        if (ValueExpression != null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    protected override void OnParametersSet()
    {
        _pipeline.BeginParametersSet();
        _pipeline.BuildStyles(this, AdditionalAttributes);

        // Only re-subscribe if EditContext actually changed
        if (EditContext != null && ValueExpression != null)
        {
            if (_previousEditContext != EditContext)
            {
                // Unsubscribe from old context
                _previousEditContext?.OnValidationStateChanged -= HandleValidationStateChanged;

                // Subscribe to new context
                EditContext.OnValidationStateChanged += HandleValidationStateChanged;
                _previousEditContext = EditContext;
            }

            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            _lastValidationError = EditContext.GetValidationMessages(_fieldIdentifier).Any();
        }
        else
        {
            _lastValidationError = false;
        }

        base.OnParametersSet();
#if DEBUG
        _pipeline.EndParametersSet(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        bool current = EditContext != null && ValueExpression != null
            && EditContext.GetValidationMessages(_fieldIdentifier).Any();
        if (current != _lastValidationError)
        {
            _lastValidationError = current;
            // Full rebuild instead of PatchVolatileAttributes: Razor-generated BuildRenderTree on
            // derived .razor files does not call base.BuildRenderTree, so the patching hook is
            // bypassed. Rebuilding styles here (identical to the post-change path of OnParametersSet)
            // keeps data-bob-error in sync for the next render.
            _pipeline.BuildStyles(this, AdditionalAttributes);
            StateHasChanged();
        }
    }
}

// Base class with variants
public abstract class BOBInputComponentBase<TValue, TComponent, TVariant>
    : BOBInputComponentBase<TValue>, IVariantComponent<TVariant>
    where TComponent : BOBInputComponentBase<TValue, TComponent, TVariant>
    where TVariant : Variant
{
    private RenderFragment? _resolvedTemplate;
    private VariantHelper<TComponent, TVariant>? _variantHelper;

    // Implementation of IVariantComponent interfaces
    Variant IVariantComponent.CurrentVariant => CurrentVariant;

    public TVariant CurrentVariant => Variant ?? DefaultVariant;
    public abstract TVariant DefaultVariant { get; }
    /// <summary>Selected variant. <see langword="null"/> falls back to <see cref="DefaultVariant"/>.</summary>
    [Parameter] public TVariant? Variant { get; set; }

    Type IVariantComponent.VariantType => typeof(TVariant);
    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }
    [Inject] private IVariantRegistry? VariantRegistry { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Let the base class handle its render logic first
        base.BuildRenderTree(builder);

        // Then add the variant template
        if (_resolvedTemplate is not null)
        {
            builder.AddContent(0, _resolvedTemplate);
        }
    }

    protected override void OnParametersSet()
    {
        // First let the base class handle its parameter setting
        base.OnParametersSet();

        // Then handle variant resolution
        _variantHelper ??= new VariantHelper<TComponent, TVariant>(
            (TComponent)this,
            VariantRegistry);

        Variant ??= DefaultVariant;
        _resolvedTemplate = _variantHelper.ResolveTemplate(Variant, BuiltInTemplates);
    }
}
