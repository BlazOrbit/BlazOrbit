using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Interaction", "BOBThemeSelector")]
public class BOBThemeSelectorInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_ToggleThemeAsync_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        FakeTimeProvider timeProvider = new();
        ctx.Services.AddSingleton<TimeProvider>(timeProvider);

        // Arrange
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("light"));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);

        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Act
        cut.Find("button").Click();

        // Assert — advance past the internal delays (50 ms + 300 ms)
        timeProvider.Advance(TimeSpan.FromMilliseconds(400));
        await cut.InvokeAsync(() => Task.CompletedTask);
        await fake.Received(1).ToggleThemeAsync(Arg.Any<string[]>());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnThemeChanged_After_Toggle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        FakeTimeProvider timeProvider = new();
        ctx.Services.AddSingleton<TimeProvider>(timeProvider);

        // Arrange
        string? capturedTheme = null;
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("light"));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);

        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>(p => p
            .Add(c => c.OnThemeChanged, t => capturedTheme = t));

        // Act
        cut.Find("button").Click();
        timeProvider.Advance(TimeSpan.FromMilliseconds(400));
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        capturedTheme.Should().Be("dark");
    }
}
