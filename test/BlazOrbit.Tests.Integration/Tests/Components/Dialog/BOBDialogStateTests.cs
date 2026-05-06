using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component State", "BOBDialog")]
public class BOBDialogStateTests
{
    private sealed class ControllableModalInterop : IModalJsInterop
    {
        public TaskCompletionSource<bool> AnimationGate { get; } = new();

        public ValueTask LockScrollAsync() => ValueTask.CompletedTask;
        public ValueTask UnlockScrollAsync() => ValueTask.CompletedTask;
        public ValueTask TrapFocusAsync(ElementReference element, string trapId) => ValueTask.CompletedTask;
        public ValueTask ReleaseFocusAsync(string trapId) => ValueTask.CompletedTask;

        public async ValueTask WaitForAnimationEndAsync(ElementReference element, int fallbackMs)
            => await AnimationGate.Task;
    }

    private static ControllableModalInterop RegisterControllable(BlazorTestContextBase ctx)
    {
        ControllableModalInterop interop = new();
        ctx.Services.AddScoped<IModalJsInterop>(_ => interop);
        return interop;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Dialog_When_Open_Becomes_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, false));

        cut.FindAll("[role='dialog']").Should().BeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Dialog_When_Open_Becomes_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true));

        cut.Find("[role='dialog']").Should().NotBeNull();

        // Act
        cut.Render(p => p.Add(c => c.Open, false));

        // Assert
        cut.FindAll("[role='dialog']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Overlay_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find(".bob-dialog-overlay").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Closing_Modifier_During_Animation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ControllableModalInterop interop = RegisterControllable(ctx);

        // Arrange
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true));

        cut.Find(".bob-dialog").GetAttribute("data-bob-closing").Should().BeNull();

        // Act — initiate close; animation gate is still open
        _ = cut.InvokeAsync(async () => await ClickOverlayAsync(cut));
        cut.WaitForState(
            () => cut.FindAll(".bob-dialog[data-bob-closing='true']").Count == 1,
            TimeSpan.FromSeconds(1));

        // Assert — during the animation, the --closing modifier is present
        cut.Find(".bob-dialog").GetAttribute("data-bob-closing").Should().Be("true");
        cut.Find(".bob-dialog-overlay").GetAttribute("data-bob-closing").Should().Be("true");

        // Finish — release animation, dialog eventually unmounts
        interop.AnimationGate.SetResult(true);
        cut.WaitForState(
            () => cut.FindAll("[role='dialog']").Count == 0,
            TimeSpan.FromSeconds(1));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OpenChanged_False_After_Close_Animation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ControllableModalInterop interop = RegisterControllable(ctx);

        // Arrange
        bool? emitted = null;
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, v => emitted = v));

        // Act
        _ = cut.InvokeAsync(async () => await ClickOverlayAsync(cut));
        cut.WaitForState(
            () => cut.FindAll(".bob-dialog[data-bob-closing='true']").Count == 1,
            TimeSpan.FromSeconds(1));

        // Still animating — OpenChanged not yet emitted
        emitted.Should().BeNull();

        // Release
        interop.AnimationGate.SetResult(true);
        cut.WaitForState(() => emitted is not null, TimeSpan.FromSeconds(1));

        // Assert
        emitted.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_FullScreen_Styles_When_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.FullScreen, true));

        // Assert
        string style = cut.Find(".bob-dialog").GetAttribute("style") ?? "";
        style.Should().Contain("width: 100vw");
        style.Should().Contain("height: 100vh");
        style.Should().Contain("border-radius: 0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Size_Constraints(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.MinWidth, "400px")
            .Add(c => c.MaxWidth, "800px")
            .Add(c => c.MinHeight, "200px")
            .Add(c => c.MaxHeight, "600px"));

        // Assert
        string style = cut.Find(".bob-dialog").GetAttribute("style") ?? "";
        style.Should().Contain("min-width: 400px");
        style.Should().Contain("max-width: 800px");
        style.Should().Contain("min-height: 200px");
        style.Should().Contain("max-height: 600px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Elevation_DataAttribute_And_Tint_And_Derived_Shadow(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterControllable(ctx);

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Elevation, 3));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-elevation").Should().Be("3");
        root.GetAttribute("data-bob-shadow").Should().Be("true");
        root.GetAttribute("style").Should().Contain("--bob-inline-elevation-tint: 11%");
    }

    private static Task ClickOverlayAsync(IRenderedComponent<BOBDialog> cut)
    {
        cut.Find(".bob-dialog-overlay").Click();
        return Task.CompletedTask;
    }
}
