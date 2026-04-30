using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Interaction", "BOBDrawer")]
public class BOBDrawerInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OpenChanged_False_On_Overlay_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnOverlayClick, true)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find(".bob-drawer-overlay").Click();

        // Assert
        openChangedValue.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OpenChanged_False_On_Escape_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnEscape, true)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find("""[data-bob-component="drawer"]""").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        openChangedValue.Should().BeFalse();
    }
}
