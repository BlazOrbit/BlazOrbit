using System.Globalization;

namespace BlazOrbit.Components;

public class BOBTransitionsBuilder
{
    private readonly BOBTransitions _transitions = new();

    public BOBTransitions Build() => _transitions;

    public TriggerTransitionBuilder On(params TransitionTrigger[] triggers)
        => new(_transitions, this, triggers);

    public TriggerTransitionBuilder OnActive() => On(TransitionTrigger.Active);

    public TriggerTransitionBuilder OnFocus() => On(TransitionTrigger.Focus);

    public TriggerTransitionBuilder OnHover() => On(TransitionTrigger.Hover);
}

public class TriggerTransitionBuilder
{
    private readonly BOBTransitionsBuilder _parent;
    private readonly BOBTransitions _transitions;
    private readonly TransitionTrigger[] _triggers;

    internal TriggerTransitionBuilder(
        BOBTransitions transitions,
        BOBTransitionsBuilder parent,
        TransitionTrigger[] triggers)
    {
        _transitions = transitions;
        _parent = parent;
        _triggers = triggers;
    }

    public BOBTransitionsBuilder And() => _parent;

    public BOBTransitions Build() => _transitions;

    // === Transform (individual CSS properties) ===

    public TriggerTransitionBuilder Scale(float value = 1.05f, Action<TransitionTiming>? timing = null)
        => AddEntry("scale", value.ToString(CultureInfo.InvariantCulture), timing);

    public TriggerTransitionBuilder Rotate(string angle = "5deg", Action<TransitionTiming>? timing = null)
        => AddEntry("rotate", angle, timing);

    public TriggerTransitionBuilder Translate(string x = "0", string y = "0", Action<TransitionTiming>? timing = null)
        => AddEntry("translate", $"{x} {y}", timing);

    // === Visual ===

    public TriggerTransitionBuilder Opacity(float value = 0.7f, Action<TransitionTiming>? timing = null)
        => AddEntry("opacity", value.ToString(CultureInfo.InvariantCulture), timing);

    public TriggerTransitionBuilder Filter(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("filter", value, timing);

    public TriggerTransitionBuilder BackdropFilter(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("backdrop-filter", value, timing);

    // === Shadows ===

    public TriggerTransitionBuilder BoxShadow(ShadowStyle shadow, Action<TransitionTiming>? timing = null)
    => AddEntry("box-shadow", shadow.ToCss(), timing);

    public TriggerTransitionBuilder TextShadow(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("text-shadow", value, timing);

    // === Colors ===

    public TriggerTransitionBuilder Color(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("color", value, timing);

    public TriggerTransitionBuilder BackgroundColor(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("background-color", value, timing);

    public TriggerTransitionBuilder Border(BorderStyle border, Action<TransitionTiming>? timing = null)
    {
        string? color = border.GetColorCss();
        if (color != null)
        {
            AddEntry("border-color", color, timing);
        }

        string? radius = border.GetRadiusCss();
        if (radius != null)
        {
            AddEntry("border-radius", radius, timing);
        }

        return this;
    }

    public TriggerTransitionBuilder OutlineColor(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("outline-color", value, timing);

    // === Background ===

    public TriggerTransitionBuilder Background(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("background", value, timing);

    // === Box model ===
    public TriggerTransitionBuilder Outline(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("outline", value, timing);

    public TriggerTransitionBuilder OutlineOffset(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("outline-offset", value, timing);

    public TriggerTransitionBuilder Padding(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("padding", value, timing);

    public TriggerTransitionBuilder Gap(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("gap", value, timing);

    // === Generic escape hatch ===

    public TriggerTransitionBuilder Property(string cssProperty, string value, Action<TransitionTiming>? timing = null)
        => AddEntry(cssProperty, value, timing);

    // === Internal ===

    private TriggerTransitionBuilder AddEntry(string cssProperty, string value, Action<TransitionTiming>? timing)
    {
        TransitionTiming resolved = new();
        timing?.Invoke(resolved);

        string? easing = null;
        if (resolved.Easing != null)
        {
            EasingBuilder easingBuilder = new();
            resolved.Easing(easingBuilder);
            easing = easingBuilder.Build();
        }

        TransitionEntry entry = new()
        {
            CssProperty = cssProperty,
            Value = value,
            Duration = resolved.Duration,
            Easing = easing,
            Delay = resolved.Delay
        };

        foreach (TransitionTrigger trigger in _triggers)
        {
            _transitions.AddEntry(trigger, entry);
        }

        return this;
    }
}

public class TransitionTiming
{
    public TimeSpan? Delay { get; set; }
    public TimeSpan? Duration { get; set; }
    public Action<EasingBuilder>? Easing { get; set; }
}