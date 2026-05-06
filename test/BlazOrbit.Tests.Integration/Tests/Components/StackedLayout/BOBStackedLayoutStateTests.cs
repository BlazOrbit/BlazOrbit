using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.StackedLayout;

[Trait("Component State", "BOBStackedLayout")]
public class BOBStackedLayoutStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_StickyHeader_In_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.StickyHeader, true));
        cut.Find("bob-component").GetAttribute("data-bob-sticky-header").Should().Be("true");

        // Act
        cut.Render(p => p.Add(c => c.StickyHeader, false));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sticky-header").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_StickyNav_In_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.StickyNav, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sticky-nav").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_NavOpen_In_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n"))
            .Add(c => c.ShowToggle, true));
        cut.Find("bob-component").GetAttribute("data-bob-nav-open").Should().BeNull();

        // Act
        cut.Find(".bob-stacked-layout__toggle").Click();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-nav-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Accept_NavOpen_As_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — parameter two-way binding: pre-open the nav.
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n"))
            .Add(c => c.NavOpen, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-nav-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_HeaderHeight_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.HeaderHeight, "72px"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-header-height: 72px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_NavColumns_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.NavColumns, 3));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-nav-columns: 3");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_ContentMaxWidth_And_NavGap_CssVariables(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.ContentMaxWidth, "1200px")
            .Add(c => c.NavGap, "1rem")
            .Add(c => c.NavMinColumnWidth, "180px"));

        // Assert
        string style = cut.Find("bob-component").GetAttribute("style") ?? "";
        style.Should().Contain("--bob-inline-content-max-width: 1200px");
        style.Should().Contain("--bob-inline-nav-gap: 1rem");
        style.Should().Contain("--bob-inline-nav-col-min: 180px");
    }
}
