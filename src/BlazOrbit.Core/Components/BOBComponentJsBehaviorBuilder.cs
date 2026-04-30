using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Abstractions;

internal sealed class BOBComponentJsBehaviorBuilder
{
    private readonly ComponentBase _component;
    private readonly BehaviorConfiguration _config = new();
    private readonly IBehaviorJsInterop _jsInterop;

    private BOBComponentJsBehaviorBuilder(
        ComponentBase component,
        IBehaviorJsInterop jsInterop)
    {
        _component = component;
        _jsInterop = jsInterop;
    }

    public static BOBComponentJsBehaviorBuilder For(
        ComponentBase component,
        IBehaviorJsInterop jsInterop) => new(component, jsInterop);

    public async Task<IJSObjectReference?> BuildAndAttachAsync()
    {
        if (_jsInterop == null || _component is not IJsBehavior)
        {
            return null;
        }

        ConfigureRipple();

        return !_config.HasAnyBehavior ? null : await _jsInterop.AttachBehaviorsAsync(_config);
    }

    // ───────────────────────────────────────────── Behavior configurations ─────────────────────────────────────────────

    private void ConfigureRipple()
    {
        if (_component is not IHasRipple hasRipple || hasRipple.DisableRipple)
        {
            return;
        }

        _config.Ripple = new RippleConfiguration
        {
            Color = hasRipple.RippleColor,
            Duration = hasRipple.RippleDurationMs,
            RippleContainer = hasRipple.GetRippleContainer(),
        };
    }
}