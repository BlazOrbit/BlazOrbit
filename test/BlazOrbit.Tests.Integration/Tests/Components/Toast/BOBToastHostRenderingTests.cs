using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Rendering", "BOBToastHost")]
public class BOBToastHostRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_No_Position_Wrappers_When_No_Toasts(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();

        // Assert — single root always present; no position wrappers when empty
        cut.FindAll("[data-bob-component='toast-host']").Should().HaveCount(1);
        cut.FindAll(".bob-toast-host__position").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toast_When_Show_Called(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        await toastService.ShowAsync(b => b.AddContent(0, "Hello"), new ToastOptions { AutoDismiss = false });

        // Assert — single root, one position wrapper populated
        cut.FindAll("[data-bob-component='toast-host']").Should().HaveCount(1);
        cut.FindAll(".bob-toast-host__position").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Single_Root_Regardless_Of_Toast_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act — show toasts in multiple positions
        await toastService.ShowAsync(b => b.AddContent(0, "tl"), new ToastOptions { AutoDismiss = false, Position = ToastPosition.TopLeft });
        await toastService.ShowAsync(b => b.AddContent(0, "br"), new ToastOptions { AutoDismiss = false, Position = ToastPosition.BottomRight });

        // Assert — only one <bob-component> root (not one per position)
        cut.FindAll("[data-bob-component='toast-host']").Should().HaveCount(1);
        cut.FindAll(".bob-toast-host__position").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toasts_Queued_Before_Mount(BlazorScenario scenario)
    {
        // COMP-TOASTHOST-03: SSR boot path can fire toasts during DI/init,
        // before the host mounts. The host must surface those preexisting
        // toasts on its first render — not wait for the next OnChange tick.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — queue a toast before the host renders
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();
        await toastService.ShowAsync(b => b.AddContent(0, "Preexisting"),
            new ToastOptions { AutoDismiss = false, Position = ToastPosition.TopRight });

        // Act
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();

        // Assert — the toast is visible on the very first render
        cut.FindAll(".bob-toast-host__position").Should().HaveCount(1);
        cut.Find("[data-bob-position='top-right']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toast_In_Correct_Position(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        await toastService.ShowAsync(b => b.AddContent(0, "Bottom Left Toast"),
            new ToastOptions { AutoDismiss = false, Position = ToastPosition.BottomLeft });

        // Assert
        cut.Find("[data-bob-position='bottom-left']").Should().NotBeNull();
    }
}
