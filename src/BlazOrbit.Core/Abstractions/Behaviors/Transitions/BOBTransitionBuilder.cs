using System.Globalization;

namespace BlazOrbit.Components;

/// <summary>Fluent root builder for a <see cref="BOBTransitions"/> set keyed by trigger.</summary>
public class BOBTransitionsBuilder
{
    private readonly BOBTransitions _transitions = new();

    /// <summary>Returns the accumulated <see cref="BOBTransitions"/>.</summary>
    public BOBTransitions Build() => _transitions;

    /// <summary>Opens a transition scope for the supplied triggers.</summary>
    public TriggerTransitionBuilder On(params TransitionTrigger[] triggers)
        => new(_transitions, this, triggers);

    /// <summary>Shortcut for <c>On(TransitionTrigger.Active)</c>.</summary>
    public TriggerTransitionBuilder OnActive() => On(TransitionTrigger.Active);

    /// <summary>Shortcut for <c>On(TransitionTrigger.Focus)</c>.</summary>
    public TriggerTransitionBuilder OnFocus() => On(TransitionTrigger.Focus);

    /// <summary>Shortcut for <c>On(TransitionTrigger.Hover)</c>.</summary>
    public TriggerTransitionBuilder OnHover() => On(TransitionTrigger.Hover);
}

/// <summary>Fluent builder for the CSS properties animated by a given set of triggers.</summary>
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

    /// <summary>Returns to the parent builder to chain another trigger scope.</summary>
    public BOBTransitionsBuilder And() => _parent;

    /// <summary>Returns the accumulated <see cref="BOBTransitions"/>.</summary>
    public BOBTransitions Build() => _transitions;

    /// <summary>Animates the CSS <c>scale</c> transform.</summary>
    public TriggerTransitionBuilder Scale(float value = 1.05f, Action<TransitionTiming>? timing = null)
        => AddEntry("scale", value.ToString(CultureInfo.InvariantCulture), timing);

    /// <summary>Animates the CSS <c>rotate</c> transform.</summary>
    public TriggerTransitionBuilder Rotate(string angle = "5deg", Action<TransitionTiming>? timing = null)
        => AddEntry("rotate", angle, timing);

    /// <summary>Animates the CSS <c>translate</c> transform.</summary>
    public TriggerTransitionBuilder Translate(string x = "0", string y = "0", Action<TransitionTiming>? timing = null)
        => AddEntry("translate", $"{x} {y}", timing);

    /// <summary>Animates <c>opacity</c>.</summary>
    public TriggerTransitionBuilder Opacity(float value = 0.7f, Action<TransitionTiming>? timing = null)
        => AddEntry("opacity", value.ToString(CultureInfo.InvariantCulture), timing);

    /// <summary>Animates the <c>filter</c> property.</summary>
    public TriggerTransitionBuilder Filter(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("filter", value, timing);

    /// <summary>Animates the <c>backdrop-filter</c> property.</summary>
    public TriggerTransitionBuilder BackdropFilter(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("backdrop-filter", value, timing);

    /// <summary>Animates <c>box-shadow</c> using a strongly-typed <see cref="ShadowStyle"/>.</summary>
    public TriggerTransitionBuilder BoxShadow(ShadowStyle shadow, Action<TransitionTiming>? timing = null)
        => AddEntry("box-shadow", shadow.ToCss(), timing);

    /// <summary>Animates <c>text-shadow</c>.</summary>
    public TriggerTransitionBuilder TextShadow(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("text-shadow", value, timing);

    /// <summary>Animates the foreground <c>color</c>.</summary>
    public TriggerTransitionBuilder Color(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("color", value, timing);

    /// <summary>Animates <c>background-color</c>.</summary>
    public TriggerTransitionBuilder BackgroundColor(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("background-color", value, timing);

    /// <summary>Animates the border-color and/or border-radius portion of a <see cref="BorderStyle"/>.</summary>
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

    /// <summary>Animates <c>outline-color</c>.</summary>
    public TriggerTransitionBuilder OutlineColor(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("outline-color", value, timing);

    /// <summary>Animates the <c>background</c> shorthand.</summary>
    public TriggerTransitionBuilder Background(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("background", value, timing);

    /// <summary>Animates the <c>outline</c> shorthand.</summary>
    public TriggerTransitionBuilder Outline(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("outline", value, timing);

    /// <summary>Animates <c>outline-offset</c>.</summary>
    public TriggerTransitionBuilder OutlineOffset(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("outline-offset", value, timing);

    /// <summary>Animates <c>padding</c>.</summary>
    public TriggerTransitionBuilder Padding(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("padding", value, timing);

    /// <summary>Animates the flex/grid <c>gap</c>.</summary>
    public TriggerTransitionBuilder Gap(string value, Action<TransitionTiming>? timing = null)
        => AddEntry("gap", value, timing);

    /// <summary>Escape hatch: animate an arbitrary CSS property to an arbitrary value.</summary>
    public TriggerTransitionBuilder Property(string cssProperty, string value, Action<TransitionTiming>? timing = null)
        => AddEntry(cssProperty, value, timing);

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

/// <summary>Per-entry CSS transition timing parameters (delay, duration, easing).</summary>
public class TransitionTiming
{
    /// <summary>Optional delay before the transition starts.</summary>
    public TimeSpan? Delay { get; set; }

    /// <summary>Optional duration of the transition.</summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>Optional configuration callback to build the CSS easing function.</summary>
    public Action<EasingBuilder>? Easing { get; set; }
}
