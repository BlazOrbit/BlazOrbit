using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Accessibility", "BOBDrawer")]
public class BOBDrawerAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Dialog_Role_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true));

        // Assert
        IElement dialog = cut.Find("[role='dialog']");
        dialog.GetAttribute("aria-modal").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Tabindex_On_Host_For_Focus_Management(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true));

        // Assert — host receives tabindex=-1 so JS focus trap can target it
        cut.Find("bob-component").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Close_Button_With_AriaLabel_When_Closable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Closable, true));

        // Assert
        IElement closeButton = cut.Find("button[aria-label='Close']");
        closeButton.Should().NotBeNull();
    }
}
