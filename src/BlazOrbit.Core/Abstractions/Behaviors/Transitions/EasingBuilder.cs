namespace BlazOrbit.Components;

/// <summary>Fluent builder for a CSS <c>cubic-bezier(...)</c> easing function.</summary>
public class CubicBezierBuilder
{
    private readonly EasingBuilder _parent;
    private double _x1, _y1, _x2, _y2;

    internal CubicBezierBuilder(EasingBuilder parent) => _parent = parent;

    /// <summary>Preset back-out curve (overshoot near the end).</summary>
    public EasingBuilder BackOut() => WithControlPoints(0.175, 0.885, 0.32, 1.275).Build();

    /// <summary>Preset bounce curve (overshoot at both ends).</summary>
    public EasingBuilder Bounce() => WithControlPoints(0.68, -0.55, 0.265, 1.55).Build();

    /// <summary>Emits the configured <c>cubic-bezier(x1, y1, x2, y2)</c> value back into the parent <see cref="EasingBuilder"/>.</summary>
    public EasingBuilder Build()
    {
        _parent.SetValue(FormattableString.Invariant($"cubic-bezier({_x1:F3}, {_y1:F3}, {_x2:F3}, {_y2:F3})"));
        return _parent;
    }

    /// <summary>Preset elastic curve (strong overshoot at both ends).</summary>
    public EasingBuilder Elastic() => WithControlPoints(0.68, -0.6, 0.32, 1.6).Build();

    /// <summary>Material Design "accelerate" curve.</summary>
    public EasingBuilder MaterialAccelerate() => WithControlPoints(0.4, 0.0, 1, 1).Build();

    /// <summary>Material Design "decelerate" curve.</summary>
    public EasingBuilder MaterialDecelerate() => WithControlPoints(0.0, 0.0, 0.2, 1).Build();

    /// <summary>Material Design "sharp" curve.</summary>
    public EasingBuilder MaterialSharp() => WithControlPoints(0.4, 0.0, 0.6, 1).Build();

    /// <summary>Material Design "standard" curve (accelerate then decelerate).</summary>
    public EasingBuilder MaterialStandard() => WithControlPoints(0.4, 0.0, 0.2, 1).Build();

    /// <summary>Sets the four cubic-bezier control point coordinates.</summary>
    public CubicBezierBuilder WithControlPoints(double x1, double y1, double x2, double y2)
    {
        _x1 = x1;
        _y1 = y1;
        _x2 = x2;
        _y2 = y2;
        return this;
    }
}

/// <summary>Fluent builder for a CSS transition timing function value.</summary>
public class EasingBuilder
{
    private string _value = "ease";

    internal EasingBuilder()
    {
    }

    /// <summary>Implicitly converts the builder to its final CSS string value.</summary>
    public static implicit operator string(EasingBuilder builder)
    {
        return builder.Build();
    }

    /// <summary>Returns the configured CSS easing value.</summary>
    public string Build() => _value;

    /// <summary>Switches to a <see cref="CubicBezierBuilder"/> for custom control points.</summary>
    public CubicBezierBuilder CubicBezier() => new(this);

    /// <summary>Sets an arbitrary CSS timing function string verbatim.</summary>
    public EasingBuilder Custom(string value)
    {
        _value = value;
        return this;
    }

    /// <summary>Uses the CSS <c>ease</c> keyword.</summary>
    public EasingBuilder Ease()
    {
        _value = "ease";
        return this;
    }

    /// <summary>Uses the CSS <c>ease-in</c> keyword.</summary>
    public EasingBuilder EaseIn()
    {
        _value = "ease-in";
        return this;
    }

    /// <summary>Uses the CSS <c>ease-in-out</c> keyword.</summary>
    public EasingBuilder EaseInOut()
    {
        _value = "ease-in-out";
        return this;
    }

    /// <summary>Uses the CSS <c>ease-out</c> keyword.</summary>
    public EasingBuilder EaseOut()
    {
        _value = "ease-out";
        return this;
    }

    /// <summary>Uses the CSS <c>linear</c> keyword.</summary>
    public EasingBuilder Linear()
    {
        _value = "linear";
        return this;
    }

    /// <summary>Switches to a <see cref="StepsBuilder"/> for a stepped timing function.</summary>
    public StepsBuilder Steps(int count) => new(this, count);

    internal void SetValue(string value) => _value = value;
}

/// <summary>Fluent builder for a CSS <c>steps(...)</c> easing function.</summary>
public class StepsBuilder
{
    private readonly int _count;
    private readonly EasingBuilder _parent;
    private string _position = "end";

    internal StepsBuilder(EasingBuilder parent, int count)
    {
        _parent = parent;
        _count = count;
    }

    /// <summary>Emits the configured <c>steps(count, position)</c> value back into the parent <see cref="EasingBuilder"/>.</summary>
    public EasingBuilder Build()
    {
        _parent.SetValue($"steps({_count}, {_position})");
        return _parent;
    }

    /// <summary>Sets the step position to <c>end</c>.</summary>
    public StepsBuilder End()
    {
        _position = "end";
        return this;
    }

    /// <summary>Sets the step position to <c>jump-both</c>.</summary>
    public StepsBuilder JumpBoth()
    {
        _position = "jump-both";
        return this;
    }

    /// <summary>Sets the step position to <c>jump-end</c>.</summary>
    public StepsBuilder JumpEnd()
    {
        _position = "jump-end";
        return this;
    }

    /// <summary>Sets the step position to <c>jump-none</c>.</summary>
    public StepsBuilder JumpNone()
    {
        _position = "jump-none";
        return this;
    }

    /// <summary>Sets the step position to <c>jump-start</c>.</summary>
    public StepsBuilder JumpStart()
    {
        _position = "jump-start";
        return this;
    }

    /// <summary>Sets the step position to <c>start</c>.</summary>
    public StepsBuilder Start()
    {
        _position = "start";
        return this;
    }
}

/// <summary>Constants and entry point for CSS easing values.</summary>
public static class Easing
{
    /// <summary>CSS <c>ease</c> keyword.</summary>
    public static readonly string Ease = "ease";

    /// <summary>CSS <c>ease-in</c> keyword.</summary>
    public static readonly string EaseIn = "ease-in";

    /// <summary>CSS <c>ease-in-out</c> keyword.</summary>
    public static readonly string EaseInOut = "ease-in-out";

    /// <summary>CSS <c>ease-out</c> keyword.</summary>
    public static readonly string EaseOut = "ease-out";

    /// <summary>CSS <c>linear</c> keyword.</summary>
    public static readonly string Linear = "linear";

    /// <summary>Starts a new fluent <see cref="EasingBuilder"/>.</summary>
    public static EasingBuilder Create() => new();
}
