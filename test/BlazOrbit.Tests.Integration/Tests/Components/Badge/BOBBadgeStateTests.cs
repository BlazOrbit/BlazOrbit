using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component State", "BOBBadge")]
public class BOBBadgeStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Content_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "1")));

        cut.Find("span.bob-badge").TextContent.Should().Contain("1");

        // Act
        cut.Render(p => p.Add(c => c.ChildContent, b => b.AddContent(0, "99")));

        // Assert
        cut.Find("span.bob-badge").TextContent.Should().Contain("99");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Circular_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "5")));

        cut.Find("bob-component").GetAttribute("data-bob-circular").Should().BeNull();

        // Act
        cut.Render(p => p
            .Add(c => c.Circular, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "5")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-circular").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.Color, "#fff")
            .Add(c => c.ChildContent, b => b.AddContent(0, "N")));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-color");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.ChildContent, b => b.AddContent(0, "X")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }
}
