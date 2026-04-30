using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Interaction", "BOBToastHost")]
public class BOBToastHostInteractionTests
{
    private static int CountOnChangeSubscribers(IToastService toastService)
    {
        // ToastService exposes `event Func<Task>? OnChangeAsync`; the compiler-generated
        // backing field carries the multicast delegate. Reading the invocation
        // list is the only host-agnostic way to assert the host actually
        // unsubscribed on dispose (vs. just not crashing).
        FieldInfo? field = toastService.GetType()
            .GetField("OnChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        Func<Task>? handler = (Func<Task>?)field?.GetValue(toastService);
        return handler?.GetInvocationList().Length ?? 0;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Unsubscribe_From_ToastService_On_Dispose(BlazorScenario scenario)
    {
        // COMP-TOASTHOST-02: previously the host implemented IDisposable while
        // its base class declared IAsyncDisposable, so Dispose() was dead code
        // and the OnChange subscription leaked across host re-mounts.
        BlazorTestContextBase ctx = scenario.CreateContext();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();
        int baseline = CountOnChangeSubscribers(toastService);

        // Arrange — render adds exactly one subscriber
        ctx.Render<BOBToastHost>();
        CountOnChangeSubscribers(toastService).Should().Be(baseline + 1);

        // Act — disposing the bUnit context disposes the host
        await ctx.DisposeAsync();

        // Assert — the host removed its handler from OnChange
        CountOnChangeSubscribers(toastService).Should().Be(baseline);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Toast_After_CloseAll(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();
        await toastService.ShowAsync(b => b.AddContent(0, "msg"), new ToastOptions { AutoDismiss = false });
        cut.FindAll("[data-bob-component='toast-host']").Should().HaveCount(1);

        // Act
        await toastService.CloseAllAsync();

        // Assert — toasts marked IsClosing, host still renders but toast has closing attr
        // (actual removal happens after animation completes)
        toastService.ActiveToasts.All(t => t.IsClosing).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Multiple_Toasts(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        await toastService.ShowAsync(b => b.AddContent(0, "Toast 1"), new ToastOptions { AutoDismiss = false });
        await toastService.ShowAsync(b => b.AddContent(0, "Toast 2"), new ToastOptions { AutoDismiss = false });

        // Assert
        toastService.ActiveToasts.Should().HaveCount(2);
    }
}
