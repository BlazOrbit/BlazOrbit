using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Core.BaseComponents;

/// <summary>
/// Direct unit tests for <see cref="BOBComponentJsBehaviorBuilder" />.
/// Pins the dispatch contract (IJsBehavior gate, IHasRipple → RippleConfiguration,
/// DisableRipple/HasAnyBehavior short-circuits, IBehaviorJsInterop wiring)
/// without going through <c>OnAfterRenderAsync</c>.
/// </summary>
[Trait("Core", "BOBComponentJsBehaviorBuilder")]
public class BOBComponentJsBehaviorBuilderTests
{
    [Fact]
    public async Task BuildAndAttachAsync_Should_Return_Null_When_Component_Is_Not_IJsBehavior()
    {
        IBehaviorJsInterop interop = Substitute.For<IBehaviorJsInterop>();
        PlainComponent component = new();

        BOBComponentJsBehaviorBuilder builder = BOBComponentJsBehaviorBuilder.For(component, interop);

        IJSObjectReference? result = await builder.BuildAndAttachAsync();

        result.Should().BeNull();
        await interop.DidNotReceive().AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>());
    }

    [Fact]
    public async Task BuildAndAttachAsync_Should_Return_Null_When_IJsBehavior_Has_No_Configured_Behaviors()
    {
        IBehaviorJsInterop interop = Substitute.For<IBehaviorJsInterop>();
        EmptyJsBehavior component = new();

        BOBComponentJsBehaviorBuilder builder = BOBComponentJsBehaviorBuilder.For(component, interop);

        IJSObjectReference? result = await builder.BuildAndAttachAsync();

        result.Should().BeNull();
        await interop.DidNotReceive().AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>());
    }

    [Fact]
    public async Task BuildAndAttachAsync_Should_Skip_Ripple_When_DisableRipple_Is_True()
    {
        IBehaviorJsInterop interop = Substitute.For<IBehaviorJsInterop>();
        RippleComponent component = new() { DisableRipple = true };

        BOBComponentJsBehaviorBuilder builder = BOBComponentJsBehaviorBuilder.For(component, interop);

        IJSObjectReference? result = await builder.BuildAndAttachAsync();

        result.Should().BeNull();
        await interop.DidNotReceive().AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>());
    }

    [Fact]
    public async Task BuildAndAttachAsync_Should_Populate_RippleConfiguration_When_Enabled()
    {
        IBehaviorJsInterop interop = Substitute.For<IBehaviorJsInterop>();
        IJSObjectReference jsRef = Substitute.For<IJSObjectReference>();
        BehaviorConfiguration? captured = null;
        interop
            .AttachBehaviorsAsync(Arg.Do<BehaviorConfiguration>(c => captured = c))
            .Returns(new ValueTask<IJSObjectReference>(jsRef));

        RippleComponent component = new()
        {
            DisableRipple = false,
            RippleColor = "#abcdef",
            RippleDurationMs = 250
        };

        BOBComponentJsBehaviorBuilder builder = BOBComponentJsBehaviorBuilder.For(component, interop);

        IJSObjectReference? result = await builder.BuildAndAttachAsync();

        result.Should().BeSameAs(jsRef);
        captured.Should().NotBeNull();
        captured!.HasAnyBehavior.Should().BeTrue();
        captured.Ripple.Should().NotBeNull();
        captured.Ripple!.Color.Should().Be("#abcdef");
        captured.Ripple.Duration.Should().Be(250);
    }

    [Fact]
    public async Task BuildAndAttachAsync_Should_Return_Null_When_Interop_Is_Null()
    {
        RippleComponent component = new();

        BOBComponentJsBehaviorBuilder builder = BOBComponentJsBehaviorBuilder.For(component, null!);

        IJSObjectReference? result = await builder.BuildAndAttachAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task BuildAndAttachAsync_Should_Accept_Null_RippleColor_And_Duration()
    {
        IBehaviorJsInterop interop = Substitute.For<IBehaviorJsInterop>();
        IJSObjectReference jsRef = Substitute.For<IJSObjectReference>();
        BehaviorConfiguration? captured = null;
        interop
            .AttachBehaviorsAsync(Arg.Do<BehaviorConfiguration>(c => captured = c))
            .Returns(new ValueTask<IJSObjectReference>(jsRef));

        RippleComponent component = new()
        {
            RippleColor = null,
            RippleDurationMs = null
        };

        BOBComponentJsBehaviorBuilder builder = BOBComponentJsBehaviorBuilder.For(component, interop);

        await builder.BuildAndAttachAsync();

        captured.Should().NotBeNull();
        captured!.Ripple.Should().NotBeNull();
        captured.Ripple!.Color.Should().BeNull();
        captured.Ripple.Duration.Should().BeNull();
    }

    [Fact]
    public void For_Factory_Should_Return_Fresh_Instance_Each_Call()
    {
        IBehaviorJsInterop interop = Substitute.For<IBehaviorJsInterop>();
        RippleComponent component = new();

        BOBComponentJsBehaviorBuilder a = BOBComponentJsBehaviorBuilder.For(component, interop);
        BOBComponentJsBehaviorBuilder b = BOBComponentJsBehaviorBuilder.For(component, interop);

        a.Should().NotBeSameAs(b);
    }

    // ─────────── Stubs ───────────

    private sealed class PlainComponent : ComponentBase;

    private sealed class EmptyJsBehavior : ComponentBase, IJsBehavior;

    private sealed class RippleComponent : ComponentBase, IHasRipple
    {
        public bool DisableRipple { get; set; }
        public string? RippleColor { get; set; }
        public int? RippleDurationMs { get; set; }

        public ElementReference GetRippleContainer() => default;
    }
}
