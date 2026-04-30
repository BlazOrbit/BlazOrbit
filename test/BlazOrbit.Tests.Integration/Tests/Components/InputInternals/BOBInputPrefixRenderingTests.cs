using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BOBInputPrefix")]
public class BOBInputPrefixRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Neither_Text_Nor_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>();

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Text_Inside_Prefix_Addon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(p => p
            .Add(c => c.PrefixText, "https://"));

        // Assert
        IElement addon = cut.Find(".bob-input__addon--prefix");
        addon.ClassList.Should().Contain("bob-addon");
        addon.QuerySelector("span")!.TextContent.Should().Be("https://");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Icon_When_Only_Icon_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(p => p
            .Add(c => c.PrefixIcon, BOBIconKeys.UI.Search));

        // Assert
        IElement addon = cut.Find(".bob-input__addon--prefix");
        addon.Should().NotBeNull();
        addon.QuerySelectorAll("span").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_Whitespace_Only_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(p => p
            .Add(c => c.PrefixText, "   ")
            .Add(c => c.PrefixIcon, (IconKey?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_When_Prefix_Parameters_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(p => p
            .Add(c => c.PrefixText, "before"));
        cut.Find(".bob-input__addon--prefix span").TextContent.Should().Be("before");

        // Act
        cut.Render(p => p.Add(c => c.PrefixText, "after"));

        // Assert
        cut.Find(".bob-input__addon--prefix span").TextContent.Should().Be("after");

        // Act
        cut.Render(p => p.Add(c => c.PrefixText, (string?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }
}
