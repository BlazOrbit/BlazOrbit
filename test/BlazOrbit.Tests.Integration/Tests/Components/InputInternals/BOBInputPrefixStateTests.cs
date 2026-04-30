using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component State", "BOBInputPrefix")]
public class BOBInputPrefixStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Text_On_Reparameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(p => p
            .Add(c => c.PrefixText, "USD"));

        cut.Find("span").TextContent.Should().Be("USD");

        // Act
        cut.Render(p => p.Add(c => c.PrefixText, "EUR"));

        // Assert
        cut.Find("span").TextContent.Should().Be("EUR");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Icon_On_Reparameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(p => p
            .Add(c => c.PrefixIcon, BOBIconKeys.MaterialIconsOutlined.i_check));

        cut.FindAll("svg").Should().NotBeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.PrefixIcon, (IconKey?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Both_Text_And_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(p => p
            .Add(c => c.PrefixText, "$")
            .Add(c => c.PrefixIcon, BOBIconKeys.MaterialIconsOutlined.i_check));

        // Assert
        cut.Find("span").TextContent.Should().Be("$");
        cut.FindAll("svg").Should().NotBeEmpty();
    }
}
