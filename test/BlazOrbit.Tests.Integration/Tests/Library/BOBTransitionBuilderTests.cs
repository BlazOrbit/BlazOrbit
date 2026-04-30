using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Direct unit tests for <see cref="BOBTransitionsBuilder" />, <see cref="TriggerTransitionBuilder" />,
/// <see cref="EasingBuilder" />, <see cref="CubicBezierBuilder" />, <see cref="StepsBuilder" />
/// and the <see cref="BOBTransitions" /> emission contract (CSS variables + data-attribute + merge).
/// </summary>
[Trait("Core", "BOBTransitionBuilder")]
public class BOBTransitionBuilderTests
{
    // ─────────── BOBTransitionsBuilder + triggers ───────────

    [Fact]
    public void Empty_Builder_Should_Produce_Transitions_With_No_Entries()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder().Build();

        transitions.HasTransitions.Should().BeFalse();
    }

    [Fact]
    public void OnHover_Scale_Should_Add_Entry_For_Hover_Trigger()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale(1.1f)
            .Build();

        transitions.HasTransitions.Should().BeTrue();

        Dictionary<string, string> vars = transitions.GetCssVariables();
        vars.Should().ContainKey("--bob-t-hover-scale");
        vars["--bob-t-hover-scale"].Should().Be("1.1");
    }

    [Fact]
    public void OnFocus_Opacity_Should_Emit_Focus_Variable()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnFocus().Opacity(0.5f)
            .Build();

        Dictionary<string, string> vars = transitions.GetCssVariables();
        vars.Should().ContainKey("--bob-t-focus-opacity");
        vars["--bob-t-focus-opacity"].Should().Be("0.5");
    }

    [Fact]
    public void OnActive_Translate_Should_Emit_Active_Variable()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnActive().Translate("1px", "2px")
            .Build();

        Dictionary<string, string> vars = transitions.GetCssVariables();
        vars["--bob-t-active-translate"].Should().Be("1px 2px");
    }

    [Fact]
    public void On_With_Multiple_Triggers_Should_Add_Same_Entry_To_Each_Trigger()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .On(TransitionTrigger.Hover, TransitionTrigger.Focus).Opacity(0.8f)
            .Build();

        Dictionary<string, string> vars = transitions.GetCssVariables();
        vars["--bob-t-hover-opacity"].Should().Be("0.8");
        vars["--bob-t-focus-opacity"].Should().Be("0.8");
    }

    [Fact]
    public void And_Should_Chain_Back_To_Root_Builder_For_Multi_Trigger_Definitions()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale(1.05f)
            .And()
            .OnFocus().Opacity(0.5f)
            .Build();

        Dictionary<string, string> vars = transitions.GetCssVariables();
        vars.Should().ContainKey("--bob-t-hover-scale");
        vars.Should().ContainKey("--bob-t-focus-opacity");
    }

    // ─────────── Entry properties (duration / easing / delay) ───────────

    [Fact]
    public void Duration_Delay_And_Easing_Should_Be_Captured_In_Transition_Shorthand()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale(1.05f, t =>
            {
                t.Duration = TimeSpan.FromMilliseconds(300);
                t.Delay = TimeSpan.FromMilliseconds(50);
                t.Easing = e => e.EaseInOut();
            })
            .Build();

        string shorthand = transitions.GetCssVariables()["--bob-t-transition"];

        shorthand.Should().Contain("scale");
        shorthand.Should().Contain("300ms");
        shorthand.Should().Contain("50ms");
        shorthand.Should().Contain("ease-in-out");
    }

    [Fact]
    public void Missing_Duration_Should_Default_To_200ms_In_Shorthand()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale()
            .Build();

        transitions.GetCssVariables()["--bob-t-transition"]
            .Should().Contain("200ms");
    }

    [Fact]
    public void Missing_Easing_Should_Default_To_Ease_In_Out()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale()
            .Build();

        transitions.GetCssVariables()["--bob-t-transition"]
            .Should().Contain("ease-in-out");
    }

    [Fact]
    public void Shorthand_Should_Pick_Longest_Duration_When_Same_Property_In_Multiple_Triggers()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Opacity(0.5f, t => t.Duration = TimeSpan.FromMilliseconds(100))
            .And()
            .OnFocus().Opacity(0.7f, t => t.Duration = TimeSpan.FromMilliseconds(500))
            .Build();

        string shorthand = transitions.GetCssVariables()["--bob-t-transition"];

        shorthand.Should().Contain("500ms");
        shorthand.Should().NotContain("100ms");
    }

    // ─────────── CSS properties ───────────

    [Fact]
    public void Border_Should_Emit_BorderColor_And_BorderRadius_Entries()
    {
        BorderStyle border = BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, "red")
            .Radius(8);

        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Border(border)
            .Build();

        Dictionary<string, string> vars = transitions.GetCssVariables();
        vars["--bob-t-hover-border-color"].Should().Be("red");
        vars["--bob-t-hover-border-radius"].Should().Be("8px");
    }

    [Fact]
    public void Property_Escape_Hatch_Should_Emit_Arbitrary_Property()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Property("letter-spacing", "0.1em")
            .Build();

        transitions.GetCssVariables()["--bob-t-hover-letter-spacing"].Should().Be("0.1em");
    }

    [Fact]
    public void Color_And_Background_Should_Emit_Separate_Variables()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover()
                .Color("red")
                .BackgroundColor("blue")
            .Build();

        Dictionary<string, string> vars = transitions.GetCssVariables();
        vars["--bob-t-hover-color"].Should().Be("red");
        vars["--bob-t-hover-background-color"].Should().Be("blue");
    }

    // ─────────── Data attribute ───────────

    [Fact]
    public void GetDataAttributeValue_Should_Emit_Trigger_And_Property_Pairs()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale()
            .And()
            .OnFocus().Opacity()
            .Build();

        string dataAttr = transitions.GetDataAttributeValue();

        dataAttr.Should().Contain("hover:scale");
        dataAttr.Should().Contain("focus:opacity");
    }

    [Fact]
    public void GetDataAttributeValue_Should_Not_Duplicate_Identical_Pairs()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale()
            .And()
            .OnHover().Scale()
            .Build();

        string[] parts = transitions.GetDataAttributeValue().Split(' ');
        parts.Should().OnlyHaveUniqueItems();
    }

    // ─────────── Merge ───────────

    [Fact]
    public void MergeWith_Should_Add_Entries_From_Both_Sources()
    {
        BOBTransitions baseTrans = new BOBTransitionsBuilder()
            .OnHover().Scale(1.05f)
            .Build();

        BOBTransitions overrides = new BOBTransitionsBuilder()
            .OnFocus().Opacity(0.5f)
            .Build();

        BOBTransitions merged = baseTrans.MergeWith(overrides);

        Dictionary<string, string> vars = merged.GetCssVariables();
        vars.Should().ContainKey("--bob-t-hover-scale");
        vars.Should().ContainKey("--bob-t-focus-opacity");
    }

    [Fact]
    public void MergeWith_Should_Override_Existing_Property_For_Same_Trigger()
    {
        BOBTransitions baseTrans = new BOBTransitionsBuilder()
            .OnHover().Scale(1.05f)
            .Build();

        BOBTransitions overrides = new BOBTransitionsBuilder()
            .OnHover().Scale(1.5f)
            .Build();

        BOBTransitions merged = baseTrans.MergeWith(overrides);

        merged.GetCssVariables()["--bob-t-hover-scale"].Should().Be("1.5");
    }

    // ─────────── EasingBuilder ───────────

    [Fact]
    public void EasingBuilder_Default_Should_Be_Ease()
    {
        EasingBuilder builder = Easing.Create();

        builder.Build().Should().Be("ease");
    }

    [Theory]
    [InlineData("Linear", "linear")]
    [InlineData("Ease", "ease")]
    [InlineData("EaseIn", "ease-in")]
    [InlineData("EaseOut", "ease-out")]
    [InlineData("EaseInOut", "ease-in-out")]
    public void EasingBuilder_Predefined_Easings_Should_Produce_Matching_String(string method, string expected)
    {
        EasingBuilder builder = Easing.Create();

        EasingBuilder result = method switch
        {
            "Linear" => builder.Linear(),
            "Ease" => builder.Ease(),
            "EaseIn" => builder.EaseIn(),
            "EaseOut" => builder.EaseOut(),
            "EaseInOut" => builder.EaseInOut(),
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        result.Build().Should().Be(expected);
    }

    [Fact]
    public void EasingBuilder_Custom_Should_Pass_Through_Value()
    {
        EasingBuilder builder = Easing.Create();

        builder.Custom("cubic-bezier(1, 1, 1, 1)").Build().Should().Be("cubic-bezier(1, 1, 1, 1)");
    }

    [Fact]
    public void EasingBuilder_Should_Implicitly_Convert_To_String()
    {
        EasingBuilder builder = Easing.Create().Linear();

        string easing = builder;

        easing.Should().Be("linear");
    }

    [Fact]
    public void CubicBezier_WithControlPoints_Build_Should_Emit_Invariant_Culture_Formatted_Values()
    {
        string result = Easing.Create()
            .CubicBezier().WithControlPoints(0.1, 0.2, 0.3, 0.4).Build()
            .Build();

        result.Should().Be("cubic-bezier(0.100, 0.200, 0.300, 0.400)");
    }

    [Theory]
    [InlineData("MaterialStandard", "cubic-bezier(0.400, 0.000, 0.200, 1.000)")]
    [InlineData("MaterialDecelerate", "cubic-bezier(0.000, 0.000, 0.200, 1.000)")]
    [InlineData("MaterialAccelerate", "cubic-bezier(0.400, 0.000, 1.000, 1.000)")]
    [InlineData("MaterialSharp", "cubic-bezier(0.400, 0.000, 0.600, 1.000)")]
    [InlineData("BackOut", "cubic-bezier(0.175, 0.885, 0.320, 1.275)")]
    [InlineData("Bounce", "cubic-bezier(0.680, -0.550, 0.265, 1.550)")]
    [InlineData("Elastic", "cubic-bezier(0.680, -0.600, 0.320, 1.600)")]
    public void CubicBezier_Predefined_Curves_Should_Emit_Expected_Shape(string curve, string expected)
    {
        CubicBezierBuilder cb = Easing.Create().CubicBezier();

        EasingBuilder result = curve switch
        {
            "MaterialStandard" => cb.MaterialStandard(),
            "MaterialDecelerate" => cb.MaterialDecelerate(),
            "MaterialAccelerate" => cb.MaterialAccelerate(),
            "MaterialSharp" => cb.MaterialSharp(),
            "BackOut" => cb.BackOut(),
            "Bounce" => cb.Bounce(),
            "Elastic" => cb.Elastic(),
            _ => throw new ArgumentOutOfRangeException(nameof(curve))
        };

        result.Build().Should().Be(expected);
    }

    [Fact]
    public void Steps_Default_Position_Should_Be_End()
    {
        string result = Easing.Create().Steps(4).Build().Build();

        result.Should().Be("steps(4, end)");
    }

    [Theory]
    [InlineData("Start", "start")]
    [InlineData("End", "end")]
    [InlineData("JumpStart", "jump-start")]
    [InlineData("JumpEnd", "jump-end")]
    [InlineData("JumpBoth", "jump-both")]
    [InlineData("JumpNone", "jump-none")]
    public void Steps_Position_Modifiers_Should_Render_Expected_Keyword(string position, string expected)
    {
        StepsBuilder steps = Easing.Create().Steps(3);

        StepsBuilder modified = position switch
        {
            "Start" => steps.Start(),
            "End" => steps.End(),
            "JumpStart" => steps.JumpStart(),
            "JumpEnd" => steps.JumpEnd(),
            "JumpBoth" => steps.JumpBoth(),
            "JumpNone" => steps.JumpNone(),
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };

        modified.Build().Build().Should().Be($"steps(3, {expected})");
    }

    [Fact]
    public void Easing_In_Transition_Should_Appear_In_Shorthand()
    {
        BOBTransitions transitions = new BOBTransitionsBuilder()
            .OnHover().Scale(1.1f, t => t.Easing = e => e.CubicBezier().MaterialStandard())
            .Build();

        string shorthand = transitions.GetCssVariables()["--bob-t-transition"];

        shorthand.Should().Contain("cubic-bezier(0.400, 0.000, 0.200, 1.000)");
    }
}
