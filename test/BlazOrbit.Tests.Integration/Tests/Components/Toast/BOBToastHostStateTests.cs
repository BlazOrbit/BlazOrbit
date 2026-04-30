using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component State", "BOBToastHost")]
public class BOBToastHostStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Position_Group_For_Each_Active_Position(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        cut.FindAll(".bob-toast-host__position").Should().BeEmpty();

        // Act
        await toastService.ShowAsync(
            b => b.AddContent(0, "top"),
            new ToastOptions { AutoDismiss = false, Position = ToastPosition.TopRight });
        await toastService.ShowAsync(
            b => b.AddContent(0, "bottom"),
            new ToastOptions { AutoDismiss = false, Position = ToastPosition.BottomLeft });

        // Assert — one position group per active position
        cut.FindAll(".bob-toast-host__position").Should().HaveCount(2);
        cut.FindAll("[data-bob-component='toast'][data-bob-position='top-right']").Should().HaveCount(1);
        cut.FindAll("[data-bob-component='toast'][data-bob-position='bottom-left']").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Limit_Visible_Toasts_Per_Position(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>(p => p
            .Add(c => c.MaxVisiblePerPosition, 2));
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act — push 4 toasts to the same position
        for (int i = 0; i < 4; i++)
        {
            await toastService.ShowAsync(
                b => b.AddContent(0, $"toast-{i}"),
                new ToastOptions { AutoDismiss = false, Position = ToastPosition.TopRight });
        }

        // Assert — only MaxVisiblePerPosition rendered
        cut.FindAll(".bob-toast-host__position[data-bob-position='top-right'] [data-bob-component='toast']").Should().HaveCount(2);
        toastService.ActiveToasts.Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Render_When_Toast_Added_After_Mount(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        await toastService.ShowAsync(
            b => b.AddContent(0, "Late toast"),
            new ToastOptions { AutoDismiss = false });

        // Assert
        cut.FindAll(".bob-toast-host__position").Should().HaveCount(1);
    }
}
