using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Rendering", "BOBToast")]
public class BOBToastRenderingTests
{
    private static ToastState CreateState(
        string content = "Toast message",
        bool closable = true,
        bool autoDismiss = false,
        ToastPosition position = ToastPosition.TopRight)
        => new()
        {
            Content = b => b.AddContent(0, content),
            Options = new ToastOptions
            {
                Closable = closable,
                AutoDismiss = autoDismiss,
                Position = position,
            }
        };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState()));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("toast");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState("Hello Toast")));

        // Assert
        cut.Find(".bob-toast__content").TextContent.Should().Contain("Hello Toast");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Close_Button_When_Closable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState(closable: true)));

        // Assert
        cut.Find("[aria-label='Close']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Close_Button_When_Not_Closable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState(closable: false)));

        // Assert
        cut.FindAll("[aria-label='Close']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Position_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState(position: ToastPosition.BottomLeft)));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-position").Should().Be("bottom-left");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Progress_Bar_When_AutoDismiss(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState(autoDismiss: true)));

        // Assert
        cut.Find(".bob-toast__progress").Should().NotBeNull();
    }
}
