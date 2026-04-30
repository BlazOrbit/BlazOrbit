using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Loading;

[Trait("Component Rendering", "BOBLoadingIndicator")]
public class BOBLoadingIndicatorRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("loading-indicator");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Spinner_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("spinner");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Status_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert — spinner has role="status"
        cut.Find("bob-component").GetAttribute("role").Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("Loading");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Svg_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert
        cut.Find("svg").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>(p => p
            .Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Linear_Variant_With_Progressbar_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>(p => p
            .Add(c => c.Variant, BOBLoadingIndicatorVariant.LinearIndeterminate));

        // Assert — linear has role="progressbar" instead of "status"
        cut.Find("bob-component").GetAttribute("role").Should().Be("progressbar");
    }
}
