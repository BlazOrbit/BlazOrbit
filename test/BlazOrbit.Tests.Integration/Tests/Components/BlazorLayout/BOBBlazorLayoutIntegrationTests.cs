using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Tests.Components.Initializer;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.BlazorLayout;

[Trait("Component Integration", "BOBBlazorLayout")]
public class BOBBlazorLayoutIntegrationTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BOBInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Toast_Via_Layout(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        await toastService.ShowAsync(b => b.AddContent(0, "Layout toast"), new ToastOptions { AutoDismiss = false });

        // Assert
        toastService.ActiveToasts.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Support_Simultaneous_Modal_And_Toast(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act — show toast
        await toastService.ShowAsync(b => b.AddContent(0, "Toast 1"), new ToastOptions { AutoDismiss = false });
        await toastService.ShowAsync(b => b.AddContent(0, "Toast 2"), new ToastOptions { AutoDismiss = false });

        // Assert — both coexist
        toastService.ActiveToasts.Should().HaveCount(2);
    }
}
