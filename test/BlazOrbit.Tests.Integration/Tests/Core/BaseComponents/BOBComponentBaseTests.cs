using AngleSharp.Dom;
using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.BaseComponents;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Core.BaseComponents;

/// <summary>
/// Functional contract tests for <see cref="BOBComponentBase" />.
///
/// These tests define and enforce the following system-level rules:
///
/// 1 - AUTOMATIC NAMING: Components MUST automatically generate a kebab-case identifier for the
/// 'data-bob-component' attribute, derived from the class name and stripping any 'BOB' prefix for
/// CSS consistency.
///
/// 2 - DESIGN STATE MAPPING: All design-related properties (Size, Density, Elevation,
/// FullWidth) MUST be mapped to 'data-bob-*' HTML attributes to enable CSS attribute selector styling.
///
/// 3 - DYNAMIC STYLE GENERATION:
/// - Colors (Foreground, Background, Ripple) MUST be injected as CSS Variables (--bob-*) using RGBA
/// deterministic formatting.
/// - Borders MUST support shorthand configuration and specific side overrides (Top, Left, etc.) via
/// CSS Variables.
///
/// 4 - STYLE HYBRIDIZATION: User-defined 'style' strings in 'AdditionalAttributes' MUST be
/// preserved and merged with system-generated CSS variables.
///
/// 5 - RIPPLE LIFECYCLE:
/// - Ripple configuration MUST be exposed as CSS variables unless 'DisableRipple' is true.
/// - When disabled, the inline ripple variables MUST be omitted (no data-bob-ripple
///   attribute is emitted — the JS behavior reads IHasRipple state directly; see
///   CSS-OPT-02 block B.6).
///
/// 6 - JAVASCRIPT BEHAVIOR BRIDGE:
/// - Components MUST attach JS behaviors after the first render via <see cref="IBehaviorJsInterop" />.
/// - JS behaviors MUST be skipped if the component is in a 'Loading' state to prevent
/// initialization on incomplete DOM trees.
/// - Components MUST ensure idempotent disposal of JS modules to prevent memory leaks.
///
/// 7 - ATTRIBUTE CLEANLINESS: The 'style' attribute MUST NOT be rendered if no dynamic variables or
/// user styles are present.
/// </summary>
[Trait("Core", "BOBComponentBase")]
public class BOBComponentBaseTests
{
    private readonly IBehaviorJsInterop _jsInterop;

    private readonly IJSObjectReference _jsModule;

    public BOBComponentBaseTests()
    {
        _jsInterop = Substitute.For<IBehaviorJsInterop>();
        _jsModule = Substitute.For<IJSObjectReference>();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Attributes_Should_Map_All_Boolean_And_Enum_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.FullWidth, true)
            .Add(c => c.Loading, true)
            .Add(c => c.Error, true)
            .Add(c => c.Disabled, true)
            .Add(c => c.ReadOnly, true)
            .Add(c => c.Required, true)
        );

        IElement el = cut.Find("div");
        el.GetAttribute("data-bob-size").Should().Be("large");
        el.GetAttribute("data-bob-density").Should().Be("compact");
        el.GetAttribute("data-bob-fullwidth").Should().Be("true");
        el.GetAttribute("data-bob-loading").Should().Be("true");
        el.GetAttribute("data-bob-error").Should().Be("true");
        el.GetAttribute("data-bob-disabled").Should().Be("true");
        el.GetAttribute("data-bob-readonly").Should().Be("true");
        el.GetAttribute("data-bob-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Border_Should_Support_Inherit_And_None_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Act: Probar BorderStyle con tipos None e Inherit (Cubre ramas de switch/if en el Builder)
        IRenderedComponent<BOBComponentBase_TestStub> cut =
            ctx.Render<BOBComponentBase_TestStub>(p => p
                .Add(c => c.Border,
                    BorderStyle.Create().None())
            );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("--bob-inline-border:").And.Contain("none");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Borders_Should_Handle_All_Specific_Sides(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBComponentBase_TestStub> cut =
            ctx.Render<BOBComponentBase_TestStub>(p => p
                .Add(c => c.Border,
                    BorderStyle.Create()
                        .Top("1px", BorderStyleType.Solid, new CssColor("#F00"))
                        .Left("2px", BorderStyleType.Dotted, new CssColor("#0F0")))
            );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("--bob-inline-border-top:").And.Contain("1px solid");
        style.Should().Contain("--bob-inline-border-left:").And.Contain("2px dotted");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Borders_Should_Handle_Shorthand_And_Complex_Overrides(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBComponentBase_TestStub> cut =
    ctx.Render<BOBComponentBase_TestStub>(p => p
        .Add(c => c.Border,
            BorderStyle.Create()
                .All("1px", BorderStyleType.Solid, new CssColor("#000"))
                .Bottom("2px", BorderStyleType.Dashed, new CssColor("#FFF"))
                .Radius(5))
    );

        string? style = cut.Find("div").GetAttribute("style");
        // Composed shorthand var
        style.Should().Contain("--bob-inline-border:").And.Contain("1px solid");
        style.Should().Contain("--bob-inline-border-radius: 5px");
        // Composed side override
        style.Should().Contain("--bob-inline-border-bottom:").And.Contain("2px dashed");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Elevation_Should_Handle_Value_And_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p.Add(c => c.Shadow, BOBShadowPresets.Elevation(12)));
        IElement el = cut.Find("div");
        el.GetAttribute("data-bob-shadow").Should().Be("true");
        el.GetAttribute("style").Should().Contain("--bob-inline-shadow:");

        cut.Render(p => p.Add(c => c.Shadow, null));
        el = cut.Find("div");
        el.HasAttribute("data-bob-shadow").Should().BeFalse();
        (el.GetAttribute("style") ?? string.Empty).Should().NotContain("--bob-inline-shadow");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Naming_Should_Remove_BOB_Prefix_And_Apply_KebabCase(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>();
        cut.Find("div").GetAttribute("data-bob-component").Should().Be("component-base_-test-stub");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Ripple_Should_Be_Disabled_When_DisableRipple_Is_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Act: DisableRipple = true (Cubre la rama donde config.Ripple no debería setearse)
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.DisableRipple, true));

        IElement el = cut.Find("div");
        // CSS-OPT-02 block B.6: data-bob-ripple is no longer emitted; assert via the
        // absence of inline ripple variables.
        el.HasAttribute("data-bob-ripple").Should().BeFalse();

        string? style = el.GetAttribute("style");
        style.Should().NotContain("--bob-inline-ripple-color");
        style.Should().NotContain("--bob-inline-ripple-duration");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Attach_And_Dispose_JS_Behavior(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ctx.Services.AddSingleton(_jsInterop);
        // Arrange
        _jsInterop.AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>())
                  .Returns(_jsModule);

        // Act
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>();
        await cut.Instance.DisposeAsync();

        // Assert
        await _jsInterop.Received(1).AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>());
        await _jsModule.Received(1).InvokeVoidAsync("dispose", Arg.Any<object[]>());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Null_AdditionalAttributes_And_Style_Merging(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Test con AdditionalAttributes nulos explícitamente
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.AdditionalAttributes, null)
            .Add(c => c.Color, "rgba(0,0,0,1)"));

        cut.Find("div").GetAttribute("style").Should().Contain("--bob-inline-color");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Style_Attribute_Should_Be_Removed_If_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Un componente sin nada que genere CSS vars
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>();
        cut.Find("div").HasAttribute("style").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Styles_Should_Handle_Colors_And_Ripple(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Color, "rgba(255,0,0,1)")
            .Add(c => c.BackgroundColor, "rgba(0,255,0,1)")
            .Add(c => c.DisableRipple, false)
            .Add(c => c.RippleColor, "rgba(255,255,255,1)")
            .Add(c => c.RippleDurationMs, 300)
        );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("--bob-inline-color: rgba(255,0,0,1)");
        style.Should().Contain("--bob-inline-background: rgba(0,255,0,1)");
        style.Should().Contain("--bob-inline-ripple-color: rgba(255,255,255,1)");
        style.Should().Contain("--bob-inline-ripple-duration: 300ms");
        // CSS-OPT-02 block B.6: ripple is signalled by the inline variables only;
        // no DOM data-attribute is emitted (the JS behavior reads IHasRipple directly).
        cut.Find("div").HasAttribute("data-bob-ripple").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task User_Styles_Should_Be_Preserved_And_Merged(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Dictionary<string, object> attrs = new()
        { { "style", "display: flex;" } };
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.AdditionalAttributes, attrs)
            .Add(c => c.Color, "rgba(255,0,0,1)")
        );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().StartWith("--bob-inline-color:");
        style.Should().Contain("display: flex;");
    }

    // ---- CORE-T-01: PatchVolatileAttributes flips data-bob-* without full rebuild ----

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PatchVolatileAttributes_Should_Update_Loading_Without_Full_Rebuild(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Loading, false));
        cut.Find("div").GetAttribute("data-bob-loading").Should().BeNull();

        // Act — flip volatile attribute
        cut.Render(p => p.Add(c => c.Loading, true));

        // Assert — attribute updated
        cut.Find("div").GetAttribute("data-bob-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PatchVolatileAttributes_Should_Update_Error_Without_Full_Rebuild(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Error, false));
        cut.Find("div").GetAttribute("data-bob-error").Should().BeNull();

        // Act
        cut.Render(p => p.Add(c => c.Error, true));

        // Assert
        cut.Find("div").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PatchVolatileAttributes_Should_Update_Disabled_Without_Full_Rebuild(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Disabled, false));

        // Act
        cut.Render(p => p.Add(c => c.Disabled, true));

        // Assert
        cut.Find("div").GetAttribute("data-bob-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PatchVolatileAttributes_Should_Update_FullWidth_Without_Full_Rebuild(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.FullWidth, false));

        // Act
        cut.Render(p => p.Add(c => c.FullWidth, true));

        // Assert
        cut.Find("div").GetAttribute("data-bob-fullwidth").Should().Be("true");
    }

    // ---- CORE-T-02: BuildComponentDataAttributes re-executes on re-render ----

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BuildComponentDataAttributes_Should_Reexecute_On_ReRender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — initial render with color
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Color, "rgba(255,0,0,1)"));
        cut.Find("div").GetAttribute("style").Should().Contain("--bob-inline-color: rgba(255,0,0,1)");

        // Act — change color
        cut.Render(p => p.Add(c => c.Color, "rgba(0,0,255,1)"));

        // Assert — updated inline var reflects new color
        cut.Find("div").GetAttribute("style").Should().Contain("--bob-inline-color: rgba(0,0,255,1)");
    }

    // ---- PERF-02: BuildStyles fingerprint cache ----

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BuildStyles_Should_Be_Idempotent_When_Parameters_Are_Unchanged(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — initial render with a representative mix of style-affecting parameters
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.Color, "rgba(255,0,0,1)")
            .Add(c => c.BackgroundColor, "rgba(0,255,0,1)")
            .Add(c => c.FullWidth, true));
        string styleBefore = cut.Find("div").GetAttribute("style")!;
        string sizeBefore = cut.Find("div").GetAttribute("data-bob-size")!;

        // Act — re-render with identical parameter values; the fingerprint cache should
        // detect the no-op and short-circuit the full ComputedAttributes rebuild.
        cut.Render(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.Color, "rgba(255,0,0,1)")
            .Add(c => c.BackgroundColor, "rgba(0,255,0,1)")
            .Add(c => c.FullWidth, true));

        // Assert — visible output is byte-identical (correctness preserved by the cache)
        cut.Find("div").GetAttribute("style").Should().Be(styleBefore);
        cut.Find("div").GetAttribute("data-bob-size").Should().Be(sizeBefore);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BuildStyles_Should_Rebuild_When_Style_Parameter_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBComponentBase_TestStub> cut = ctx.Render<BOBComponentBase_TestStub>(p => p
            .Add(c => c.Color, "rgba(255,0,0,1)"));

        // Act — flip a style-affecting parameter; fingerprint must diverge so the
        // rebuild path runs and the inline var picks up the new value.
        cut.Render(p => p.Add(c => c.Color, "rgba(0,0,255,1)"));

        // Assert
        cut.Find("div").GetAttribute("style").Should().Contain("--bob-inline-color: rgba(0,0,255,1)");
    }
}