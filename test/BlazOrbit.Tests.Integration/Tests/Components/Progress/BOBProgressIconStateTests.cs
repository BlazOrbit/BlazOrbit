using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Progress;

[Trait("Component State", "BOBProgressIcon")]
public class BOBProgressIconStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Aria_Label_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBProgressIcon> cut = ctx.Render<BOBProgressIcon>();

        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("Loading");

        // Act
        cut.Render(p => p.Add(c => c.AriaLabel, "Saving data"));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("Saving data");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBProgressIcon> cut = ctx.Render<BOBProgressIcon>(p => p
            .Add(c => c.Size, BOBSize.Small));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("small");

        // Act
        cut.Render(p => p.Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBProgressIcon> cut = ctx.Render<BOBProgressIcon>(p => p
            .Add(c => c.Color, "#ff0000"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-color");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Variant_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBProgressIcon> cut = ctx.Render<BOBProgressIcon>(p => p
            .Add(c => c.Variant, BOBProgressIconVariant.Spinner));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("spinner");

        // Act
        cut.Render(p => p.Add(c => c.Variant, BOBProgressIconVariant.Dots));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("dots");
    }
}