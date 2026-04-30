using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.StackedLayout;

[Trait("Component Interaction", "BOBStackedLayout")]
public class BOBStackedLayoutInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_NavOpen_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n")));

        // Act
        cut.Find(".bob-stacked-layout__toggle").Click();

        // Assert
        cut.Find(".bob-stacked-layout__toggle").GetAttribute("aria-expanded").Should().Be("true");

        // Act — second click closes
        cut.Find(".bob-stacked-layout__toggle").Click();

        // Assert
        cut.Find(".bob-stacked-layout__toggle").GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_NavOpenChanged_With_New_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<bool> emitted = [];

        // Arrange
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n"))
            .Add(c => c.NavOpenChanged, v => emitted.Add(v)));

        // Act
        cut.Find(".bob-stacked-layout__toggle").Click();
        cut.Find(".bob-stacked-layout__toggle").Click();

        // Assert
        emitted.Should().Equal(true, false);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Toggle_When_Nav_Absent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — ShowToggle default is true, but Nav == null gates it.
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.ShowToggle, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "m")));

        // Assert — toggle must not exist if there is no nav to reveal
        cut.FindAll(".bob-stacked-layout__toggle").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Toggle_When_ShowToggle_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n"))
            .Add(c => c.ShowToggle, false));

        // Assert
        cut.FindAll(".bob-stacked-layout__toggle").Should().BeEmpty();
    }
}
