using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.Initializer;

[Trait("Component Interaction", "BOBInitializer")]
public class BOBInitializerInteractionTests
{
    private static IThemeJsInterop BuildFake(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BOBInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
        return fake;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Subscribe_To_OnThemeChanged_On_Init(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = BuildFake(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>();

        // Assert — component subscribed: verify via InitializeAsync being called (lifecycle completed)
        await fake.Received(1).InitializeAsync(Arg.Any<string?>());
        fake.Received(1).OnThemeChanged += Arg.Any<Action<string>?>();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Unsubscribe_From_OnThemeChanged_On_Dispose(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = BuildFake(ctx);

        // Arrange
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>();

        // Act
        cut.Instance.Dispose();

        // Assert
        fake.Received(1).OnThemeChanged -= Arg.Any<Action<string>?>();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reload_Palette_When_ThemeChanged_Fires(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Action<string>? registeredHandler = null;
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BOBInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        fake.OnThemeChanged += Arg.Do<Action<string>>(h => registeredHandler = h);
        ctx.Services.AddScoped(_ => fake);

        // Arrange
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .AddChildContent("<span class='child'>ok</span>"));

        int callsBefore = fake.ReceivedCalls()
            .Count(c => c.GetMethodInfo().Name == "GetPaletteAsync");

        // Act — simulate theme change event
        registeredHandler?.Invoke("light");
        cut.WaitForState(() => true, TimeSpan.FromMilliseconds(300));

        // Assert — GetPaletteAsync called again
        int callsAfter = fake.ReceivedCalls()
            .Count(c => c.GetMethodInfo().Name == "GetPaletteAsync");
        callsAfter.Should().BeGreaterThan(callsBefore);
    }
}
