using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Svg;

[Trait("Component State", "BOBSvgIcon")]
public class BOBSvgIconStateTests
{
    private static readonly IconKey IconA = new("IconA") { SvgContent = "<path d=\"M1 1h22v22H1z\"/>" };
    private static readonly IconKey IconB = new("IconB") { SvgContent = "<circle cx=\"12\" cy=\"12\" r=\"10\"/>" };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Icon_Content_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, IconA));

        cut.Find("svg").InnerHtml.Should().Contain("M1 1h22v22H1z");

        // Act
        cut.Render(p => p.Add(c => c.Icon, IconB));

        // Assert
        cut.Find("svg").InnerHtml.Should().Contain("cx=\"12\"");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, IconA)
            .Add(c => c.Size, BOBSize.Small));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("small");

        // Act
        cut.Render(p => p
            .Add(c => c.Icon, IconA)
            .Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, IconA)
            .Add(c => c.Color, "#ff0000"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-color");
    }
}
