using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Rendering", "BOBBadge")]
public class BOBBadgeRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "New")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("badge");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Bob_Badge_Span(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "3")));

        // Assert
        cut.Find("span.bob-badge").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "42")));

        // Assert
        cut.Find("span.bob-badge").TextContent.Should().Contain("42");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dot_Attribute_When_No_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — no ChildContent = dot mode
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>();

        // Assert — data-bob-dot emitted when no content
        cut.Find("bob-component").GetAttribute("data-bob-dot").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Dot_Attribute_When_Content_Present(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "5")));

        // Assert — no dot when content is present
        cut.Find("bob-component").GetAttribute("data-bob-dot").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Circular_Attribute_When_Circular_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.Circular, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "1")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-circular").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Background_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.BackgroundColor, "red")
            .Add(c => c.ChildContent, b => b.AddContent(0, "!")));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-background");
    }
}
