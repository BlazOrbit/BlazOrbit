using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component State", "BOBInputLoading")]
public class BOBInputLoadingStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Indicator_When_Loading_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(p => p
            .Add(c => c.Loading, false));

        cut.Markup.Trim().Should().BeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.Loading, true));

        // Assert
        cut.FindAll("div.bob-addon").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Indicator_When_Loading_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(p => p
            .Add(c => c.Loading, true));

        cut.FindAll("div.bob-addon").Should().NotBeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.Loading, false));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }
}
