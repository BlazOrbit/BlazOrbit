using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component State", "BOBDrawer")]
public class BOBDrawerStateTests
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
    public async Task Should_Show_Drawer_When_Open_Becomes_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, false));
        cut.FindAll("[role='dialog']").Should().BeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Drawer_When_Open_Becomes_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true));
        cut.Find("[role='dialog']").Should().NotBeNull();

        // Act
        cut.Render(p => p.Add(c => c.Open, false));

        // Assert
        cut.FindAll("[role='dialog']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Position_In_Modifier_Class(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        foreach (DrawerPosition position in Enum.GetValues<DrawerPosition>())
        {
            // Arrange & Act
            IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
                .Add(c => c.Open, true)
                .Add(c => c.Position, position));

            // Assert
            cut.Find(".bob-drawer").ClassList.Should()
                .Contain($"bob-drawer--{position.ToString().ToLowerInvariant()}");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Size_As_Width_For_Left_Right(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Position, DrawerPosition.Left)
            .Add(c => c.Size, "280px"));

        // Assert
        cut.Find(".bob-drawer").GetAttribute("style").Should().Contain("width: 280px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Size_As_Height_For_Top_Bottom(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Position, DrawerPosition.Top)
            .Add(c => c.Size, "180px"));

        // Assert
        cut.Find(".bob-drawer").GetAttribute("style").Should().Contain("height: 180px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Closing_Modifier_During_Animation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ControllableModalInterop interop = RegisterControllable(ctx);

        // Arrange
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true));
        cut.Find(".bob-drawer").ClassList.Contains("bob-drawer--closing").Should().BeFalse();

        // Act
        cut.Find(".bob-drawer-overlay").Click();
        cut.WaitForState(
            () => cut.FindAll(".bob-drawer.bob-drawer--closing").Count == 1,
            TimeSpan.FromSeconds(1));

        // Assert
        cut.Find(".bob-drawer").ClassList.Contains("bob-drawer--closing").Should().BeTrue();
        cut.Find(".bob-drawer-overlay").ClassList.Contains("bob-drawer-overlay--closing").Should().BeTrue();

        // Finish
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
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, v => emitted = v));

        // Act
        cut.Find(".bob-drawer-overlay").Click();
        cut.WaitForState(
            () => cut.FindAll(".bob-drawer--closing").Count == 1,
            TimeSpan.FromSeconds(1));
        emitted.Should().BeNull();

        interop.AnimationGate.SetResult(true);
        cut.WaitForState(() => emitted is not null, TimeSpan.FromSeconds(1));

        // Assert
        emitted.Should().BeFalse();
    }
}
