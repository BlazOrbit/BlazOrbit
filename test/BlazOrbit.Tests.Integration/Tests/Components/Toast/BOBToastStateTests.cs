using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component State", "BOBToast")]
public class BOBToastStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Closing_DataAttribute_When_IsClosing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        ToastState state = new()
        {
            Content = b => b.AddContent(0, "msg"),
            Options = ToastOptions.Default,
            // Act — set IsClosing = true
            IsClosing = true
        };

        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, state));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-closing").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Paused_DataAttribute_When_IsPaused(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        ToastState state = new()
        {
            Content = b => b.AddContent(0, "msg"),
            Options = ToastOptions.Default,
            IsPaused = true
        };

        // Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, state));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-paused").Should().Be("true");
    }
}
