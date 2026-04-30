using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BOBInputLoading")]
public class BOBInputLoadingRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Loading_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(p => p
            .Add(c => c.Loading, false));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Addon_With_Loading_Indicator_When_Loading_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(p => p
            .Add(c => c.Loading, true));

        // Assert
        IElement addon = cut.Find(".bob-addon");
        addon.Should().NotBeNull();
        cut.FindComponents<BOBLoadingIndicator>().Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_LoadingIndicatorVariant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(p => p
            .Add(c => c.Loading, true)
            .Add(c => c.LoadingIndicatorVariant, BOBLoadingIndicatorVariant.Dots));

        // Assert
        IRenderedComponent<BOBLoadingIndicator> indicator = cut.FindComponent<BOBLoadingIndicator>();
        indicator.Instance.Variant.Name.Should().Be("Dots");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Size_To_Indicator(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(p => p
            .Add(c => c.Loading, true)
            .Add(c => c.Size, BOBSize.Large));

        // Assert
        IRenderedComponent<BOBLoadingIndicator> indicator = cut.FindComponent<BOBLoadingIndicator>();
        indicator.Instance.Size.Should().Be(BOBSize.Large);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Re_Render_When_Loading_Toggled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(p => p
            .Add(c => c.Loading, false));
        cut.Markup.Trim().Should().BeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.Loading, true));

        // Assert
        cut.FindAll(".bob-addon").Should().HaveCount(1);

        // Act
        cut.Render(p => p.Add(c => c.Loading, false));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }
}
