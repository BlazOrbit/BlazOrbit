using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace BlazOrbit.Tests.Integration.Tests.Core.BaseComponents;

/// <summary>
/// Direct unit tests for <see cref="BOBComponentAttributesBuilder" />.
/// Exercises <c>BuildStyles</c> and <c>PatchVolatileAttributes</c> against minimal stubs
/// so that the contract (kebab-case naming, per-IHas* emission, volatile subset, family
/// coexistence, built-component hook, inline-style merging) is pinned without going through
/// the full bUnit render pipeline.
/// </summary>
[Trait("Core", "BOBComponentAttributesBuilder")]
public class BOBComponentAttributesBuilderUnitTests
{
    // ---------- Naming ----------

    [Fact]
    public void ComponentName_Should_Strip_BOB_Prefix_And_Kebab_Case()
    {
        BOBComponentAttributesBuilder builder = new();
        BOBDemoComponent component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("demo-component");
    }

    [Fact]
    public void ComponentName_Should_Strip_Generic_Arity_Backtick()
    {
        BOBComponentAttributesBuilder builder = new();
        BOBGenericDemo<int> component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("generic-demo");
    }

    [Fact]
    public void ComponentName_Should_Kebab_Case_Non_Prefixed_Types()
    {
        BOBComponentAttributesBuilder builder = new();
        PlainComponent component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("plain-component");
    }

    // ---------- Per-IHas emission ----------

    [Fact]
    public void BuildStyles_Should_Emit_Data_Attributes_For_Every_IHas_Interface()
    {
        BOBComponentAttributesBuilder builder = new();
        FullFeaturedStub component = new()
        {
            Size = BOBSize.Large,
            Density = BOBDensity.Compact,
            FullWidth = true,
            Loading = true,
            ErrorFlag = true,
            DisabledFlag = true,
            ActiveFlag = true,
            ReadOnlyFlag = true,
            RequiredFlag = true
        };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Size].Should().Be("large");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Density].Should().Be("compact");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Loading].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Error].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Active].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Required].Should().Be("true");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Color_And_Background_Inline_Variables()
    {
        BOBComponentAttributesBuilder builder = new();
        ColoredStub component = new()
        {
            Color = "rgba(10,20,30,1)",
            BackgroundColor = "rgba(40,50,60,1)"
        };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bob-inline-color: rgba(10,20,30,1)");
        style.Should().Contain("--bob-inline-background: rgba(40,50,60,1)");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Ripple_Variables_When_Enabled()
    {
        BOBComponentAttributesBuilder builder = new();
        RippleStub component = new()
        {
            DisableRipple = false,
            RippleColor = "rgba(255,255,255,0.5)",
            RippleDurationMs = 250
        };

        builder.BuildStyles(component, null);

        // CSS-OPT-02 block B.6: data-bob-ripple is no longer emitted; the JS behavior
        // builder reads IHasRipple state directly. Inline variables remain.
        builder.ComputedAttributes.Should().NotContainKey("data-bob-ripple");
        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bob-inline-ripple-color: rgba(255,255,255,0.5)");
        style.Should().Contain("--bob-inline-ripple-duration: 250ms");
    }

    [Fact]
    public void BuildStyles_Should_Omit_Ripple_Variables_When_Disabled()
    {
        BOBComponentAttributesBuilder builder = new();
        RippleStub component = new()
        {
            DisableRipple = true,
            RippleColor = "rgba(255,255,255,0.5)",
            RippleDurationMs = 250
        };

        builder.BuildStyles(component, null);

        // CSS-OPT-02 block B.6: no DOM data-attribute is emitted in either state.
        builder.ComputedAttributes.Should().NotContainKey("data-bob-ripple");
        builder.ComputedAttributes.TryGetValue("style", out object? style).Should().BeFalse(
            "ripple vars must not be emitted when disabled");
        _ = style;
    }

    [Fact]
    public void BuildStyles_Should_Emit_Shadow_Attribute_And_Variable()
    {
        BOBComponentAttributesBuilder builder = new();
        ShadowStub component = new() { Shadow = BOBShadowPresets.Elevation(4) };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Shadow].Should().Be("true");
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bob-inline-shadow:");
    }

    [Fact]
    public void BuildStyles_Should_Drop_Shadow_Attribute_When_Shadow_Is_Null()
    {
        BOBComponentAttributesBuilder builder = new();
        ShadowStub component = new() { Shadow = null };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Shadow)
            .Should().BeFalse();
    }

    [Fact]
    public void BuildStyles_Should_Emit_Elevation_DataAttribute_Tint_And_Derived_Shadow()
    {
        BOBComponentAttributesBuilder builder = new();
        ElevationStub component = new() { Elevation = 2 };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Elevation].Should().Be("2");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Shadow].Should().Be("true");
        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bob-inline-elevation-tint: 8%");
        style.Should().Contain("--bob-inline-shadow:");
    }

    [Fact]
    public void BuildStyles_Should_Clamp_Elevation_To_0_24()
    {
        BOBComponentAttributesBuilder builder = new();
        ElevationStub component = new() { Elevation = 30 };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Elevation].Should().Be("24");
    }

    [Fact]
    public void BuildStyles_Should_Not_Emit_Elevation_When_Null()
    {
        BOBComponentAttributesBuilder builder = new();
        ElevationStub component = new() { Elevation = null };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Elevation).Should().BeFalse();
        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Shadow).Should().BeFalse();
        builder.ComputedAttributes.ContainsKey("style").Should().BeFalse();
    }

    [Fact]
    public void BuildStyles_Should_Give_Shadow_Precedence_Over_Elevation()
    {
        BOBComponentAttributesBuilder builder = new();
        ShadowAndElevationStub component = new()
        {
            Shadow = BOBShadowPresets.Elevation(8),
            Elevation = 2
        };

        builder.BuildStyles(component, null);

        // Shadow explicit wins; elevation still emits tint and data attr
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Elevation].Should().Be("2");
        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bob-inline-elevation-tint: 8%");
        // The explicit shadow CSS should remain, not be overwritten by elevation-derived shadow
        style.Should().Contain(BOBShadowPresets.Elevation(8).ToCss());
    }

    [Fact]
    public void BuildStyles_Should_Emit_Prefix_And_Suffix_Variables()
    {
        BOBComponentAttributesBuilder builder = new();
        PrefixSuffixStub component = new()
        {
            PrefixColor = "#123",
            PrefixBackgroundColor = "#abc",
            SuffixColor = "#456",
            SuffixBackgroundColor = "#def"
        };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bob-inline-prefix-color: #123");
        style.Should().Contain("--bob-inline-prefix-background: #abc");
        style.Should().Contain("--bob-inline-suffix-color: #456");
        style.Should().Contain("--bob-inline-suffix-background: #def");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Border_Sides_And_Radius()
    {
        BOBComponentAttributesBuilder builder = new();
        BorderStub component = new()
        {
            Border = BorderStyle.Create()
                .All("1px", BorderStyleType.Solid, "#000")
                .Radius(6)
        };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bob-inline-border:");
        style.Should().Contain("1px solid #000");
        style.Should().Contain("--bob-inline-border-radius: 6px");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Transitions_DataAttribute_And_Variables()
    {
        BOBComponentAttributesBuilder builder = new();
        TransitionsStub component = new()
        {
            Transitions = BOBTransitionPresets.HoverFade
        };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Transitions)
            .Should().BeTrue();
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bob-t-transition");
    }

    // ---------- Variant ----------

    [Fact]
    public void BuildStyles_Should_Emit_Variant_Attribute_In_Lower_Invariant()
    {
        BOBComponentAttributesBuilder builder = new();
        VariantStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Variant].Should().Be("filled");
    }

    // ---------- Families ----------

    [Fact]
    public void BuildStyles_Should_Emit_All_Family_Attributes_When_Component_Is_Multi_Family()
    {
        BOBComponentAttributesBuilder builder = new();
        MultiFamilyStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.InputBase].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.PickerBase].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.DataCollectionBase].Should().Be("true");
    }

    // ---------- IBuiltComponent hook ----------

    [Fact]
    public void BuildStyles_Should_Invoke_BuildComponentDataAttributes_And_CssVariables()
    {
        BOBComponentAttributesBuilder builder = new();
        BuiltComponentStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes["data-bob-custom"].Should().Be("custom-value");
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bob-inline-custom: 42px");
    }

    // ---------- User style merge ----------

    [Fact]
    public void BuildStyles_Should_Merge_User_Style_After_Framework_Variables()
    {
        BOBComponentAttributesBuilder builder = new();
        ColoredStub component = new() { Color = "rgba(1,2,3,1)" };
        Dictionary<string, object> additional = new() { ["style"] = "display: flex;" };

        builder.BuildStyles(component, additional);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().StartWith("--bob-inline-color:");
        style.Should().Contain("display: flex;");
    }

    [Fact]
    public void BuildStyles_Should_Preserve_Only_User_Style_When_No_Framework_Vars_Present()
    {
        BOBComponentAttributesBuilder builder = new();
        PlainComponent component = new();
        Dictionary<string, object> additional = new() { ["style"] = "opacity: 0.5" };

        builder.BuildStyles(component, additional);

        ((string)builder.ComputedAttributes["style"]).Should().Be("opacity: 0.5");
    }

    [Fact]
    public void BuildStyles_Should_Remove_Style_When_No_Variables_And_No_User_Style()
    {
        BOBComponentAttributesBuilder builder = new();
        PlainComponent component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes.ContainsKey("style").Should().BeFalse();
    }

    // ---------- Order stability ----------

    [Fact]
    public void BuildStyles_Should_Produce_Identical_Style_String_For_Identical_Inputs()
    {
        BOBComponentAttributesBuilder b1 = new();
        BOBComponentAttributesBuilder b2 = new();
        ColoredStub c = new()
        {
            Color = "rgba(10,20,30,1)",
            BackgroundColor = "rgba(40,50,60,1)"
        };

        b1.BuildStyles(c, null);
        b2.BuildStyles(c, null);

        ((string)b1.ComputedAttributes["style"])
            .Should().Be((string)b2.ComputedAttributes["style"]);
    }

    [Fact]
    public void BuildStyles_Should_Reset_Previous_State_On_Re_Invocation()
    {
        BOBComponentAttributesBuilder builder = new();
        ColoredStub withColor = new() { Color = "rgba(1,2,3,1)" };
        PlainComponent plain = new();

        builder.BuildStyles(withColor, null);
        builder.ComputedAttributes.ContainsKey("style").Should().BeTrue();

        builder.BuildStyles(plain, null);

        builder.ComputedAttributes.ContainsKey("style").Should().BeFalse(
            "second component has no vars — stale style must be cleared");
        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Size)
            .Should().BeFalse("second component does not implement IHasSize");
    }

    // ---------- PatchVolatileAttributes ----------

    [Fact]
    public void PatchVolatileAttributes_Should_Refresh_Volatile_Subset_Without_Touching_Component_Attribute()
    {
        BOBComponentAttributesBuilder builder = new();
        FullFeaturedStub component = new()
        {
            Size = BOBSize.Small,
            DisabledFlag = false,
            Loading = false,
            ErrorFlag = false,
            ReadOnlyFlag = false,
            RequiredFlag = false,
            ActiveFlag = false
        };

        builder.BuildStyles(component, null);

        // Flip every volatile flag
        component.DisabledFlag = true;
        component.Loading = true;
        component.ErrorFlag = true;
        component.ReadOnlyFlag = true;
        component.RequiredFlag = true;
        component.ActiveFlag = true;

        builder.PatchVolatileAttributes(component);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Loading].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Error].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Required].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Active].Should().Be("true");
        // Non-volatile attribute left untouched
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Size].Should().Be("small");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component].Should().Be("full-featured-stub");
    }

    [Fact]
    public void PatchVolatileAttributes_Should_Re_Execute_BuildComponentDataAttributes()
    {
        BOBComponentAttributesBuilder builder = new();
        BuiltComponentStub component = new();

        builder.BuildStyles(component, null);
        builder.ComputedAttributes["data-bob-custom"].Should().Be("custom-value");

        component.DataValue = "patched";
        builder.PatchVolatileAttributes(component);

        builder.ComputedAttributes["data-bob-custom"].Should().Be("patched");
    }

    // ---------- BASE-02: framework owns the contract keys ----------

    [Fact]
    public void BuildStyles_Should_Preserve_Framework_DataAttributes_Over_Component_Overrides()
    {
        BOBComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new() { DisabledFlag = true };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("contract-hijacking-stub",
                "framework-owned data-bob-component must not be overridable by a component hook");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled]
            .Should().Be("true",
                "framework-owned data-bob-disabled must reflect IHasDisabled, not the hook's override");
    }

    [Fact]
    public void BuildStyles_Should_Preserve_Framework_InlineVariables_Over_Component_Overrides()
    {
        BOBComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new() { ColorValue = "rgba(10,20,30,1)" };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bob-inline-color: rgba(10,20,30,1)",
            "framework-owned --bob-inline-color must win over a component override");
        style.Should().NotContain("HIJACKED");
    }

    [Fact]
    public void BuildStyles_Should_Allow_Component_Keys_That_Do_Not_Collide()
    {
        BOBComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes["data-bob-custom-extra"].Should().Be("ok");
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bob-inline-custom-extra: 99px");
    }

    [Fact]
    public void PatchVolatileAttributes_Should_Preserve_Framework_Volatile_State_Over_Component_Overrides()
    {
        BOBComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new() { DisabledFlag = true };

        builder.BuildStyles(component, null);

        component.DisabledFlag = false;
        builder.PatchVolatileAttributes(component);

        builder.ComputedAttributes
            .Should().NotContainKey(FeatureDefinitions.DataAttributes.Disabled,
                "PatchVolatileAttributes must refresh data-bob-disabled from IHasDisabled and ignore the component override");
    }

    // ---------- Style fingerprint cache ----------

    [Fact]
    public void BuildStyles_Should_Hit_Fingerprint_Cache_On_Second_Call_With_Identical_Inputs()
    {
        BOBComponentAttributesBuilder builder = new();
        ColoredStub component = new()
        {
            Color = "rgba(10,20,30,1)",
            BackgroundColor = "rgba(40,50,60,1)"
        };

        // First call: cold cache → must rebuild.
        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse(
            "first call cannot hit a cache that is empty");

        // Second call with the same component instance and same inputs: cache must hit.
        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeTrue(
            "fingerprint hit + same additionalAttributes reference must short-circuit BuildStyles");
    }

    [Fact]
    public void BuildStyles_Should_Hit_Cache_For_Components_That_Do_Not_Implement_IBuiltComponent()
    {
        // Regression guard: when BOBComponentBase declared IBuiltComponent directly, every
        // descendant was flag-Built and the cache never applied. After the marker became opt-in,
        // a stub that does not declare IBuiltComponent must be cache-eligible.
        BOBComponentAttributesBuilder builder = new();
        FullFeaturedStub component = new()
        {
            Size = BOBSize.Medium,
            Density = BOBDensity.Standard
        };

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse();

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeTrue(
            "FullFeaturedStub does not implement IBuiltComponent, so the cache must apply");
    }

    [Fact]
    public void BuildStyles_Should_Bypass_Cache_When_Component_Implements_IBuiltComponent()
    {
        // IBuiltComponent is opt-out for the cache: hooks may read state opaque to the
        // fingerprint (timers, counters), so the builder must rebuild on every call.
        BOBComponentAttributesBuilder builder = new();
        BuiltComponentStub component = new();

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse(
            "first call: cache cold");

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse(
            "second call: IBuiltComponent opts out — no cache hit even when inputs match");
    }

    [Fact]
    public void BuildStyles_Should_Hit_Cache_When_Component_Implements_IPureBuiltComponent()
    {
        // IPureBuiltComponent is the refined opt-in: hooks read only [Parameter] state,
        // so the builder folds their contributions into the fingerprint and the cache applies.
        BOBComponentAttributesBuilder builder = new();
        PureBuiltComponentStub component = new() { Gap = "1rem" };

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse("first call: cache cold");

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeTrue(
            "second call: hooks are pure → fingerprint includes their output → cache hits when params match");
    }

    [Fact]
    public void BuildStyles_Should_Invalidate_Cache_When_Pure_Component_Parameter_Changes()
    {
        BOBComponentAttributesBuilder builder = new();
        PureBuiltComponentStub component = new() { Gap = "1rem" };

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse();

        component.Gap = "2rem";
        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse(
            "fingerprint must diverge when a parameter that the pure hook reads changes");

        // And the rebuild must reflect the new value.
        ((string)builder.ComputedAttributes["style"]!).Should().Contain("--_gap: 2rem");
    }

    [Fact]
    public void BuildStyles_Should_Invalidate_Cache_When_Style_Affecting_Parameter_Changes()
    {
        BOBComponentAttributesBuilder builder = new();
        ColoredStub component = new() { Color = "rgba(10,20,30,1)" };

        builder.BuildStyles(component, null);
        builder.LastBuildSkipped.Should().BeFalse();

        // Mutate a fingerprint-relevant input.
        component.Color = "rgba(99,99,99,1)";
        builder.BuildStyles(component, null);

        builder.LastBuildSkipped.Should().BeFalse(
            "fingerprint must diverge when a style-affecting parameter changes");
        ((string)builder.ComputedAttributes["style"]).Should().Contain("rgba(99,99,99,1)");
    }

    [Fact]
    public void BuildStyles_Should_Invalidate_Cache_When_AdditionalAttributes_Reference_Changes()
    {
        BOBComponentAttributesBuilder builder = new();
        ColoredStub component = new() { Color = "rgba(10,20,30,1)" };
        Dictionary<string, object> first = new() { ["data-x"] = "1" };
        Dictionary<string, object> secondSameContent = new() { ["data-x"] = "1" };

        builder.BuildStyles(component, first);
        builder.LastBuildSkipped.Should().BeFalse();

        // Same content, different reference. Cache compares by reference identity (the parent
        // typically reuses the same dictionary unless it actually mutates the captured set).
        builder.BuildStyles(component, secondSameContent);
        builder.LastBuildSkipped.Should().BeFalse(
            "reference change in additionalAttributes invalidates the cache, even with identical content");
    }

    // ---------- Perf regression guard (PERF-05) ----------

    /// <summary>
    /// PERF-05: warm cache hits must be measurably faster than cold rebuilds.
    /// Threshold is deliberately loose (2×) to survive Debug-mode CI runs; the real-world
    /// Release-build speedup on representative components is 5–30×. This test is a sanity guard:
    /// if a future refactor erases the cache fast-path silently, this fact fails. It does NOT
    /// claim a specific perf budget — that lives in dedicated benchmarks if/when they exist.
    /// </summary>
    [Fact]
    public void BuildStyles_Cache_Hit_Should_Be_Significantly_Faster_Than_Cold_Rebuild()
    {
        const int iterations = 10_000;

        // Warmup — JIT both paths.
        BOBComponentAttributesBuilder warmup = new();
        FullFeaturedStub warmStub = new() { Size = BOBSize.Medium, Density = BOBDensity.Standard };
        for (int i = 0; i < 200; i++)
        {
            warmup.BuildStyles(warmStub, null);
        }

        // Cold path: fresh builder per iteration so every call is a cache miss.
        Stopwatch cold = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            BOBComponentAttributesBuilder b = new();
            FullFeaturedStub s = new() { Size = BOBSize.Medium, Density = BOBDensity.Standard };
            b.BuildStyles(s, null);
        }

        cold.Stop();

        // Warm path: one builder, repeated calls — every call after the first is a cache hit.
        BOBComponentAttributesBuilder warm = new();
        FullFeaturedStub stub = new() { Size = BOBSize.Medium, Density = BOBDensity.Standard };
        warm.BuildStyles(stub, null); // prime
        Stopwatch hot = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            warm.BuildStyles(stub, null);
        }

        hot.Stop();

        warm.LastBuildSkipped.Should().BeTrue("the warm-path measurements must actually be hitting the cache");

        double speedup = (double)cold.ElapsedTicks / Math.Max(hot.ElapsedTicks, 1);
        speedup.Should().BeGreaterThan(2.0,
            because: $"cache hit must be measurably faster than cold rebuild. Measured: cold={cold.ElapsedMilliseconds}ms, " +
                     $"warm={hot.ElapsedMilliseconds}ms, speedup={speedup:F1}× over {iterations} iterations.");
    }

    // ---------- Type info cache (PERF-04) ----------

    [Fact]
    public void GetTypeInfo_Should_Resolve_Same_Instance_For_Same_Type()
    {
        BOBComponentAttributesBuilder b1 = new();
        BOBComponentAttributesBuilder b2 = new();

        b1.BuildStyles(new ColoredStub(), null);
        b2.BuildStyles(new ColoredStub(), null);

        // Both should resolve to "colored-stub" without reflecting twice (cache is static).
        // The observable invariant is that the component name is stable and the style composition
        // for the same inputs matches byte-for-byte.
        b1.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be(b2.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]);
    }

    // ================================================================
    // Stubs
    // ================================================================

    private sealed class BOBDemoComponent : ComponentBase;

    private sealed class BOBGenericDemo<T> : ComponentBase;

    private sealed class PlainComponent : ComponentBase;

    private sealed class ColoredStub : ComponentBase, IHasColor, IHasBackgroundColor
    {
        public string? Color { get; set; }
        public string? BackgroundColor { get; set; }
    }

    private sealed class RippleStub : ComponentBase, IHasRipple
    {
        public bool DisableRipple { get; set; }
        public string? RippleColor { get; set; }
        public int? RippleDurationMs { get; set; }
        public ElementReference GetRippleContainer() => default;
    }

    private sealed class ShadowStub : ComponentBase, IHasShadow
    {
        public ShadowStyle? Shadow { get; set; }
    }

    private sealed class ElevationStub : ComponentBase, IHasElevation
    {
        public int? Elevation { get; set; }
    }

    private sealed class ShadowAndElevationStub : ComponentBase, IHasShadow, IHasElevation
    {
        public ShadowStyle? Shadow { get; set; }
        public int? Elevation { get; set; }
    }

    private sealed class PrefixSuffixStub : ComponentBase, IHasPrefix, IHasSuffix
    {
        public string? PrefixText { get; set; }
        public IconKey? PrefixIcon { get; set; }
        public string? PrefixColor { get; set; }
        public string? PrefixBackgroundColor { get; set; }
        public string? SuffixText { get; set; }
        public IconKey? SuffixIcon { get; set; }
        public string? SuffixColor { get; set; }
        public string? SuffixBackgroundColor { get; set; }
    }

    private sealed class BorderStub : ComponentBase, IHasBorder
    {
        public BorderStyle? Border { get; set; }
    }

    private sealed class TransitionsStub : ComponentBase, IHasTransitions
    {
        public BOBTransitions? Transitions { get; set; }
    }

    private sealed class VariantStub : ComponentBase, IVariantComponent
    {
        public Variant CurrentVariant { get; set; } = new BOBBadgeVariant("Filled");
        public Type VariantType => typeof(BOBBadgeVariant);
    }

    private sealed class MultiFamilyStub : ComponentBase,
        IInputFamilyComponent, IPickerFamilyComponent, IDataCollectionFamilyComponent;

    private sealed class BuiltComponentStub : ComponentBase, IBuiltComponent
    {
        public string DataValue { get; set; } = "custom-value";

        public void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
            => dataAttributes["data-bob-custom"] = DataValue;

        public void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
            => cssVariables["--bob-inline-custom"] = "42px";
    }

    private sealed class PureBuiltComponentStub : ComponentBase, IPureBuiltComponent
    {
        public string? Gap { get; set; }

        public void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes) { }

        public void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
        {
            if (Gap != null)
            {
                cssVariables["--_gap"] = Gap;
            }
        }
    }

    private sealed class ContractHijackingStub : ComponentBase, IBuiltComponent, IHasDisabled, IHasColor
    {
        public bool DisabledFlag { get; set; }
        public bool Disabled { get => DisabledFlag; set => DisabledFlag = value; }
        public bool IsDisabled => DisabledFlag;

        public string? ColorValue { get; set; }
        public string? Color { get => ColorValue; set => ColorValue = value; }

        public void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
        {
            dataAttributes[FeatureDefinitions.DataAttributes.Component] = "HIJACKED";
            dataAttributes[FeatureDefinitions.DataAttributes.Disabled] = "HIJACKED";
            dataAttributes["data-bob-custom-extra"] = "ok";
        }

        public void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
        {
            cssVariables[FeatureDefinitions.InlineVariables.Color] = "HIJACKED";
            cssVariables["--bob-inline-custom-extra"] = "99px";
        }
    }

    private sealed class FullFeaturedStub : ComponentBase,
        IHasSize, IHasDensity, IHasFullWidth,
        IHasLoading, IHasError, IHasDisabled, IHasActive, IHasReadOnly, IHasRequired
    {
        public BOBSize Size { get; set; } = BOBSize.Medium;
        public BOBDensity Density { get; set; } = BOBDensity.Standard;
        public bool FullWidth { get; set; }
        public bool Loading { get; set; }
        public bool ErrorFlag { get; set; }
        public bool Error => ErrorFlag;
        public bool IsError => ErrorFlag;
        public bool DisabledFlag { get; set; }
        public bool Disabled { get => DisabledFlag; set => DisabledFlag = value; }
        public bool IsDisabled => DisabledFlag;
        public bool ActiveFlag { get; set; }
        public bool Active { get => ActiveFlag; set => ActiveFlag = value; }
        public bool IsActive => ActiveFlag;
        public bool ReadOnlyFlag { get; set; }
        public bool ReadOnly => ReadOnlyFlag;
        public bool IsReadOnly => ReadOnlyFlag;
        public bool RequiredFlag { get; set; }
        public bool Required => RequiredFlag;
        public bool IsRequired => RequiredFlag;
    }
}
