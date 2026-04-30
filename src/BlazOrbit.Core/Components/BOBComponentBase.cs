using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazOrbit.Abstractions;

public abstract class BOBComponentBase : ComponentBase, IAsyncDisposable, IBuiltComponent
{
    private readonly BOBComponentPipeline _pipeline = new();

#if DEBUG
    [Inject] private IBOBPerformanceService? PerformanceService { get; set; }
#endif

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // Exposed as `public`: variant templates are `RenderFragment`s authored *outside* the
    // component's own .razor, so they need cross-assembly access to spread `@attributes` onto the
    // `<bob-component>` root. Protected would block the custom-variant pattern that is part of the
    // framework contract.
    public Dictionary<string, object> ComputedAttributes => _pipeline.ComputedAttributes;

    /// <summary>
    /// `true` after <see cref="DisposeAsync"/> has started. Derived components that subscribe to
    /// <c>NavigationManager.LocationChanged</c>, register children through cascading parameters,
    /// or hold a <c>CancellationTokenSource</c> must gate any post-await continuation on this flag
    /// before touching component state. See CLAUDE.md §"Async / JS interop conventions". Derived
    /// classes may set it eagerly at the top of their own override of <c>DisposeAsync</c> so that
    /// the rest of their teardown sees it as disposed.
    /// </summary>
    protected bool IsDisposed { get; set; }

    [Inject] private IBehaviorJsInterop BehaviorJsInterop { get; set; } = default!;

    /// <summary>
    /// Override this method in derived components to add custom CSS variables. These will be merged
    /// with the standard behavior CSS variables. This method is called from
    /// BOBComponentAttributesBuilder during the style building process.
    /// </summary>
    /// <param name="cssVariables">
    /// Dictionary to add CSS variables to
    /// </param>
    public virtual void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
    {
        // Default implementation does nothing Derived classes can override to add their custom variables
    }

    /// <summary>
    /// Override this method to add component-specific data attributes. Called during attribute
    /// building process.
    /// </summary>
    public virtual void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
    {
        // Default: no custom data attributes
    }

    protected override void OnInitialized()
    {
        _pipeline.BeginInit();
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        _pipeline.BeginParametersSet();
        base.OnParametersSet();
        _pipeline.BuildStyles(this, AdditionalAttributes);
#if DEBUG
        if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("TrackPerformanceEnabled", out object? trackPerfAttr))
        {
            bool isEnabled = trackPerfAttr switch
            {
                bool b => b,
                string s => bool.TryParse(s, out bool res) && res,
                _ => false
            };

            _pipeline.EndParametersSet(GetType().Name, PerformanceService, isEnabled);
        }
#endif
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
#if DEBUG
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("TrackPerformanceEnabled", out object? trackPerfAttr))
            {
                bool isEnabled = trackPerfAttr switch
                {
                    bool b => b,
                    string s => bool.TryParse(s, out bool res) && res,
                    _ => false
                };

                _pipeline.EndInit(GetType().Name, PerformanceService, isEnabled);
            }
#endif
            if (IsDisposed)
            {
                return;
            }

            await _pipeline.AttachBehaviorAsync(this, BehaviorJsInterop);
            if (IsDisposed)
            {
                // Raced with dispose while awaiting JS attach: release what was just created.
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
#if DEBUG
        if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("TrackPerformanceEnabled", out object? trackPerfAttr))
        {
            bool isEnabled = trackPerfAttr switch
            {
                bool b => b,
                string s => bool.TryParse(s, out bool res) && res,
                _ => false
            };

            _pipeline.EndRenderTree(GetType().Name, PerformanceService, isEnabled);
        }
#endif
    }

    public virtual ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return _pipeline.DisposeBehaviorAsync();
    }
}
