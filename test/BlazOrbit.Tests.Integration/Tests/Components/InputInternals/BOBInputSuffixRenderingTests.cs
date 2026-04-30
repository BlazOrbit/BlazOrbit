using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BOBInputSuffix")]
public class BOBInputSuffixRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Neither_Text_Nor_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>();

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Text_Inside_Suffix_Addon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(p => p
            .Add(c => c.SuffixText, ".com"));

        // Assert
        IElement addon = cut.Find(".bob-input__addon--suffix");
        addon.ClassList.Should().Contain("bob-addon");
        addon.QuerySelector("span")!.TextContent.Should().Be(".com");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Icon_When_Only_Icon_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(p => p
            .Add(c => c.SuffixIcon, BOBIconKeys.UI.Close));

        // Assert
        IElement addon = cut.Find(".bob-input__addon--suffix");
        addon.Should().NotBeNull();
        addon.QuerySelectorAll("span").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_Whitespace_Only_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(p => p
            .Add(c => c.SuffixText, "   ")
            .Add(c => c.SuffixIcon, (IconKey?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_When_Suffix_Parameters_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(p => p
            .Add(c => c.SuffixText, "before"));
        cut.Find(".bob-input__addon--suffix span").TextContent.Should().Be("before");

        // Act
        cut.Render(p => p.Add(c => c.SuffixText, "after"));

        // Assert
        cut.Find(".bob-input__addon--suffix span").TextContent.Should().Be("after");

        // Act
        cut.Render(p => p.Add(c => c.SuffixText, (string?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }
}
