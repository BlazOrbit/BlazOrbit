using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Card;

[Trait("Component State", "BOBCard")]
public class BOBCardStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Content_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "First")));

        cut.Find(".bob-card__content").TextContent.Should().Contain("First");

        // Act
        cut.Render(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Second")));

        // Assert
        cut.Find(".bob-card__content").TextContent.Should().Contain("Second");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Shadow_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Shadow, ShadowStyle.Create(4, 8))
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-shadow").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Elevation_DataAttribute_And_Tint(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Elevation, 3)
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-elevation").Should().Be("3");
        root.GetAttribute("data-bob-shadow").Should().Be("true");
        root.GetAttribute("style").Should().Contain("--bob-inline-elevation-tint: 11%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Give_Shadow_Precedence_Over_Elevation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        ShadowStyle explicitShadow = ShadowStyle.Create(4, 8);
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Shadow, explicitShadow)
            .Add(c => c.Elevation, 3)
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-elevation").Should().Be("3");
        root.GetAttribute("style").Should().Contain("--bob-inline-elevation-tint: 11%");
        root.GetAttribute("style").Should().Contain(explicitShadow.ToCss());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Border_InlineVar(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Border, BorderStyle.Create().All("1px", BorderStyleType.Solid, "red"))
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-border");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnClick_When_Clickable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool clicked = false;
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, _ => clicked = true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Act
        cut.Find(".bob-card").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_UserAttributes_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .AddUnmatched("data-testid", "my-card")
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Act
        cut.Render(p => p
            .AddUnmatched("data-testid", "my-card")
            .Add(c => c.ChildContent, b => b.AddContent(0, "updated")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-testid").Should().Be("my-card");
    }
}
