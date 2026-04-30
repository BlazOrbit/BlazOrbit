using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Interaction", "BOBDialog")]
public class BOBDialogInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OpenChanged_False_When_Overlay_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnOverlayClick, true)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find(".bob-dialog-overlay").Click();

        // Assert
        openChangedValue.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Close_On_Overlay_Click_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnOverlayClick, false)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find(".bob-dialog-overlay").Click();

        // Assert
        openChangedValue.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OpenChanged_False_On_Escape_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnEscape, true)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find("[data-bob-component=\"dialog\"]").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        openChangedValue.Should().BeFalse();
    }
}
