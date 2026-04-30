using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Svg;

[Trait("Component Accessibility", "BOBSvgIcon")]
public class BOBSvgIconAccessibilityTests
{
    private static readonly IconKey SimpleIcon = new("SimpleIcon") { SvgContent = "<path d=\"M12 2L2 22h20L12 2z\"/>" };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Svg_Aria_Hidden_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon));

        // Assert — decorative by default
        cut.Find("svg").GetAttribute("aria-hidden").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Svg_Non_Focusable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon));

        // Assert
        cut.Find("svg").GetAttribute("focusable").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Title_Element_When_Title_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon)
            .Add(c => c.Title, "Decorative star"));

        // Assert
        IElement title = cut.Find("svg > title");
        title.TextContent.Should().Be("Decorative star");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Title_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon));

        // Assert
        cut.FindAll("svg > title").Should().BeEmpty();
    }
}
