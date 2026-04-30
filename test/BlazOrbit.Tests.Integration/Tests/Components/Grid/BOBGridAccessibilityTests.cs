using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Grid;

[Trait("Component Accessibility", "BOBGrid")]
public class BOBGridAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_Semantic_Role_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — BOBGrid is a pure layout primitive.
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert — no role/aria automatically applied
        IElement root = cut.Find("bob-component");
        root.HasAttribute("role").Should().BeFalse();
        root.HasAttribute("aria-label").Should().BeFalse();
        root.HasAttribute("tabindex").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Role_From_AdditionalAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — consumer opts in to role="list" / role="grid" via attributes.
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .AddUnmatched("role", "list")
            .AddUnmatched("aria-label", "Products")
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("role").Should().Be("list");
        root.GetAttribute("aria-label").Should().Be("Products");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Role_From_AdditionalAttributes_On_GridItem(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .AddUnmatched("role", "listitem")
            .Add(c => c.ChildContent, b => b.AddContent(0, "Item")));

        // Assert
        cut.Find("bob-component").GetAttribute("role").Should().Be("listitem");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Block_Semantic_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — heading inside grid cell preserves document outline.
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0, "<h2 id='section'>Heading</h2>")));

        // Assert
        cut.Find("h2#section").TextContent.Should().Be("Heading");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Leak_Breakpoint_Hide_Flags_As_AriaHidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — HideXs is visual-only (CSS media query driven).
        // SR should still reach the content; aria-hidden must not be emitted.
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.HideXs, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "Hidden on mobile")));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-hide-xs").Should().Be("true");
        root.HasAttribute("aria-hidden").Should().BeFalse();
    }
}
