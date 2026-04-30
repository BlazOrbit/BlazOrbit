using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Accessibility", "BOBToastHost")]
public class BOBToastHostAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_AriaLive_Region_For_Each_Toast(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        await toastService.ShowAsync(
            b => b.AddContent(0, "Hello"),
            new ToastOptions { AutoDismiss = false });

        // Assert — toasts surface aria-live for screen readers
        IReadOnlyList<IElement> liveRegions = cut.FindAll("[aria-live]");
        liveRegions.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Toasts_As_Atomic(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        await toastService.ShowAsync(
            b => b.AddContent(0, "Atomic msg"),
            new ToastOptions { AutoDismiss = false });

        // Assert — aria-atomic ensures the entire toast text is announced together
        IReadOnlyList<IElement> toasts = cut.FindAll("[aria-atomic='true']");
        toasts.Should().NotBeEmpty();
    }
}
