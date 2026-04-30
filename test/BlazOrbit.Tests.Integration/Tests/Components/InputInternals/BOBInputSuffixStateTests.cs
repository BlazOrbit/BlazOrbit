using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component State", "BOBInputSuffix")]
public class BOBInputSuffixStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Text_On_Reparameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(p => p
            .Add(c => c.SuffixText, "kg"));

        cut.Find("span").TextContent.Should().Be("kg");

        // Act
        cut.Render(p => p.Add(c => c.SuffixText, "g"));

        // Assert
        cut.Find("span").TextContent.Should().Be("g");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Icon_On_Reparameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(p => p
            .Add(c => c.SuffixIcon, BOBIconKeys.MaterialIconsOutlined.i_check));

        cut.FindAll("svg").Should().NotBeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.SuffixIcon, (IconKey?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Both_Text_And_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(p => p
            .Add(c => c.SuffixText, "EUR")
            .Add(c => c.SuffixIcon, BOBIconKeys.MaterialIconsOutlined.i_check));

        // Assert
        cut.Find("span").TextContent.Should().Be("EUR");
        cut.FindAll("svg").Should().NotBeEmpty();
    }
}
