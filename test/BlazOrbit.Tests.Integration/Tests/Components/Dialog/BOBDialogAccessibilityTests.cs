using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Accessibility", "BOBDialog")]
public class BOBDialogAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Role_Dialog(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Modal_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").GetAttribute("aria-modal").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Labelledby_Pointing_To_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Title, "Dialog Title"));

        // Assert
        string? labelledBy = cut.Find("[role='dialog']").GetAttribute("aria-labelledby");
        labelledBy.Should().NotBeNullOrEmpty();
        cut.Find($"#{labelledBy}").TextContent.Should().Be("Dialog Title");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Button_Have_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Title, "Title")
            .Add(c => c.Closable, true));

        // Assert
        cut.Find("[aria-label='Close']").Should().NotBeNull();
    }
}
