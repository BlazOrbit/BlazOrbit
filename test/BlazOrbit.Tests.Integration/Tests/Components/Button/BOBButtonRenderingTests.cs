using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Button;

[Trait("Component Rendering", "BOBButton")]
public class BOBButtonRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Test Button")
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.BackgroundColor, PaletteColor.Background)
            .Add(c => c.Color, PaletteColor.BackgroundContrast)

            .Add(c => c.Shadow, BOBShadowPresets.Elevation(4)));

        // Assert
        cut.Find("bob-component").Should().NotBeNull();
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("button");
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
        cut.Find("bob-component").GetAttribute("data-bob-shadow").Should().Be("true");
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-shadow:");
    }
}