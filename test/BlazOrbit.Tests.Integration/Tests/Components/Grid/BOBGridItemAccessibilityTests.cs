using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Grid;

[Trait("Component Accessibility", "BOBGridItem")]
public class BOBGridItemAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Role_Via_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .AddUnmatched("role", "gridcell"));

        // Assert
        cut.Find("bob-component").GetAttribute("role").Should().Be("gridcell");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Aria_Label_Via_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .AddUnmatched("aria-label", "Header cell"));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("Header cell");
    }
}
