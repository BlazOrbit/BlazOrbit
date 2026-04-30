namespace BlazOrbit.Components;

public static class BOBTransitionPresets
{
    public static BOBTransitions HoverScale => new BOBTransitionsBuilder()
        .OnHover().Scale()
        .Build();

    public static BOBTransitions HoverShadow => new BOBTransitionsBuilder()
        .OnHover().BoxShadow(BOBShadowPresets.Elevation(4))
        .Build();

    public static BOBTransitions HoverFade => new BOBTransitionsBuilder()
        .OnHover().Opacity()
        .Build();

    public static BOBTransitions HoverLift => new BOBTransitionsBuilder()
        .OnHover()
            .Translate("0", "-4px")
            .BoxShadow(BOBShadowPresets.Elevation(4))
        .Build();

    public static BOBTransitions HoverGlow => new BOBTransitionsBuilder()
        .OnHover()
            .BoxShadow(ShadowStyle.Create(
                    y: 0,
                    blur: 20,
                    opacity: 0.5f,
                    x: 0,
                    spread: 0,
                    color: PaletteColor.Shadow,
                    inset: false
                ))
            .Scale(1.02f)
        .Build();

    public static BOBTransitions CardHover => new BOBTransitionsBuilder()
        .OnHover()
            .Translate("0", "-4px", t =>
            {
                t.Duration = TimeSpan.FromMilliseconds(300);
                t.Easing = e => e.CubicBezier().MaterialStandard();
            })
            .BoxShadow(BOBShadowPresets.Elevation(8))
        .Build();

    public static BOBTransitions FocusRing => new BOBTransitionsBuilder()
        .OnFocus().BoxShadow(BOBShadowPresets.Elevation(2))
        .Build();

    public static BOBTransitions Interactive => new BOBTransitionsBuilder()
        .OnHover()
            .Translate("0", "-4px")
            .BoxShadow(BOBShadowPresets.Elevation(4))
        .And()
        .OnFocus().BoxShadow(BOBShadowPresets.Elevation(2))
        .And()
        .OnActive().Scale(0.98f)
        .Build();

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

    public static BOBTransitions GlassMorphism => new BOBTransitionsBuilder()
        .OnHover()
            .BackdropFilter("blur(16px)")
            .BoxShadow(BOBShadowPresets.Elevation(6))
            .Scale(1.02f)
        .Build();

    public static BOBTransitions Neumorphism => new BOBTransitionsBuilder()
        .OnHover().BoxShadow(
            ShadowStyle.Create(8, 16, 0.1f, x: 8)
                .Add(-8, 16, 0.7f, x: -8, color: "#ffffff"))
        .Build();

    public static BOBTransitionsBuilder Create() => new();
}