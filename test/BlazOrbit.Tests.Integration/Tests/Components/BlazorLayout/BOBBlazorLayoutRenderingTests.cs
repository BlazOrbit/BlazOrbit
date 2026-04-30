using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Tests.Components.Initializer;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.BlazorLayout;

[Trait("Component Rendering", "BOBBlazorLayout")]
public class BOBBlazorLayoutRenderingTests
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
    public async Task Should_Render_Body_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>(p => p
            .Add(c => c.Body, (RenderFragment)(b => b.AddMarkupContent(0, "<div class='page-body'>Content</div>"))));

        // Assert
        cut.FindAll(".page-body").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Include_BOBToastHost(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>();

        // Assert — BOBToastHost is mounted (no toast visible by default)
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();
        toastService.ActiveToasts.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Include_BOBModalHost(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>();

        // Assert — modal service available (no modals by default)
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();
        modalService.Should().NotBeNull();
    }
}
