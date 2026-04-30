using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Accessibility", "BOBBadge")]
public class BOBBadgeAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Aria_Label_Via_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "3 new messages"))
            .AddUnmatched("aria-label", "3 new messages"));

        // Assert — aria-label applied via unmatched attributes to root
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("3 new messages");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Role_Via_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "New"))
            .AddUnmatched("role", "status"));

        // Assert
        cut.Find("bob-component").GetAttribute("role").Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Span_Badge_Inside_Root(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "5")));

        // Assert — content is in a span, not an interactive element
        cut.Find("span.bob-badge").Should().NotBeNull();
        cut.FindAll("button").Should().BeEmpty();
        cut.FindAll("a").Should().BeEmpty();
    }
}
