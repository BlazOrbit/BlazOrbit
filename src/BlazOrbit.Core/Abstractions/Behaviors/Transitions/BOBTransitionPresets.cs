namespace BlazOrbit.Components;

/// <summary>Ready-made <see cref="BOBTransitions"/> recipes for common interactive effects.</summary>
public static class BOBTransitionPresets
{
    /// <summary>Subtle scale-up on hover.</summary>
    public static BOBTransitions HoverScale => new BOBTransitionsBuilder()
        .OnHover().Scale()
        .Build();

    /// <summary>Elevation shadow on hover.</summary>
    public static BOBTransitions HoverShadow => new BOBTransitionsBuilder()
        .OnHover().BoxShadow(BOBShadowPresets.Elevation(4))
        .Build();

    /// <summary>Opacity fade on hover.</summary>
    public static BOBTransitions HoverFade => new BOBTransitionsBuilder()
        .OnHover().Opacity()
        .Build();

    /// <summary>Translate + elevation on hover (lift effect).</summary>
    public static BOBTransitions HoverLift => new BOBTransitionsBuilder()
        .OnHover()
        .Translate("0", "-4px")
        .BoxShadow(BOBShadowPresets.Elevation(4))
        .Build();

    /// <summary>Wide shadow + slight scale on hover (glow effect).</summary>
    public static BOBTransitions HoverGlow => new BOBTransitionsBuilder()
        .OnHover()
        .BoxShadow(ShadowStyle.Create(
            0,
            20,
            0.5f,
            0,
            0,
            PaletteColor.Shadow,
            false
        ))
        .Scale(1.02f)
        .Build();

    /// <summary>Material-style card hover (lift + tall shadow with custom timing).</summary>
    public static BOBTransitions CardHover => new BOBTransitionsBuilder()
        .OnHover()
        .Translate("0", "-4px", t =>
        {
            t.Duration = TimeSpan.FromMilliseconds(300);
            t.Easing = e => e.CubicBezier().MaterialStandard();
        })
        .BoxShadow(BOBShadowPresets.Elevation(8))
        .Build();

    /// <summary>Focus-only elevation ring.</summary>
    public static BOBTransitions FocusRing => new BOBTransitionsBuilder()
        .OnFocus().BoxShadow(BOBShadowPresets.Elevation(2))
        .Build();

    /// <summary>Composite: hover lift, focus ring, active scale-down.</summary>
    public static BOBTransitions Interactive => new BOBTransitionsBuilder()
        .OnHover()
        .Translate("0", "-4px")
        .BoxShadow(BOBShadowPresets.Elevation(4))
        .And()
        .OnFocus().BoxShadow(BOBShadowPresets.Elevation(2))
        .And()
        .OnActive().Scale(0.98f)
        .Build();

    /// <summary>Material Design button preset (hover elevation, active scale-down).</summary>
    public static BOBTransitions MaterialButton => new BOBTransitionsBuilder()
        .OnHover().BoxShadow(BOBShadowPresets.Elevation(4), t =>
        {
            t.Duration = TimeSpan.FromMilliseconds(200);
            t.Easing = e => e.CubicBezier().MaterialStandard();
        })
        .And()
        .OnActive().Scale(0.96f, t =>
        {
            t.Duration = TimeSpan.FromMilliseconds(100);
            t.Easing = e => e.CubicBezier().MaterialAccelerate();
        })
        .Build();

    /// <summary>Premium button preset (hover scale + large shadow, active scale-down).</summary>
    public static BOBTransitions PremiumButton => new BOBTransitionsBuilder()
        .OnHover()
        .Scale(1.05f, t =>
        {
            t.Duration = TimeSpan.FromMilliseconds(200);
            t.Easing = e => e.CubicBezier().MaterialStandard();
        })
        .BoxShadow(
            BOBShadowPresets.Elevation(12))
        .And()
        .OnActive().Scale(0.98f, t => t.Duration = TimeSpan.FromMilliseconds(50))
        .Build();

    /// <summary>Glassmorphism preset (backdrop blur + shadow + scale on hover).</summary>
    public static BOBTransitions GlassMorphism => new BOBTransitionsBuilder()
        .OnHover()
        .BackdropFilter("blur(16px)")
        .BoxShadow(BOBShadowPresets.Elevation(6))
        .Scale(1.02f)
        .Build();

    /// <summary>Neumorphism preset (paired light/dark shadows on hover).</summary>
    public static BOBTransitions Neumorphism => new BOBTransitionsBuilder()
        .OnHover().BoxShadow(
            ShadowStyle.Create(8, 16, 0.1f, 8)
                .Add(-8, 16, 0.7f, -8, color: "#ffffff"))
        .Build();

    /// <summary>Starts a new fluent <see cref="BOBTransitionsBuilder"/>.</summary>
    public static BOBTransitionsBuilder Create() => new();
}