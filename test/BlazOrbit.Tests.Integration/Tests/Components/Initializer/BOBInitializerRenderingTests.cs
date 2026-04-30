using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.Initializer;

[Trait("Component Rendering", "BOBInitializer")]
public class BOBInitializerRenderingTests
{
    internal static readonly Dictionary<string, string> FullPalette = new()
    {
        ["--palette-background"] = "#121212",
        ["--palette-background-contrast"] = "#FFFFFF",
        ["--palette-error"] = "#CF6679",
        ["--palette-error-contrast"] = "#000000",
        ["--palette-info"] = "#64B5F6",
        ["--palette-info-contrast"] = "#000000",
        ["--palette-primary"] = "#8AB4F8",
        ["--palette-primary-contrast"] = "#000000",
        ["--palette-secondary"] = "#B39DDB",
        ["--palette-secondary-contrast"] = "#000000",
        ["--palette-shadow"] = "#000000",
        ["--palette-success"] = "#81C995",
        ["--palette-success-contrast"] = "#000000",
        ["--palette-surface"] = "#1E1E1E",
        ["--palette-surface-contrast"] = "#FFFFFF",
        ["--palette-warning"] = "#FFD54F",
        ["--palette-warning-contrast"] = "#000000",
    };

    internal static IThemeJsInterop RegisterFakeTheme(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(new ValueTask<Dictionary<string, string>>(FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
        return fake;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_After_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .AddChildContent("<div class='test-child'>Hello</div>"));

        // Assert
        cut.FindAll(".test-child").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_For_Null_ChildContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>();

        // Assert — no child divs (only HeadContent + CascadingValue shell)
        cut.FindAll("div").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_InitializeAsync_On_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .Add(c => c.DefaultTheme, "light"));

        // Assert
        await fake.Received(1).InitializeAsync("light");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_GetPaletteAsync_On_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>();

        // Assert
        await fake.Received(1).GetPaletteAsync();
    }
}
