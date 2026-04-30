using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Loading;

[Trait("Component Accessibility", "BOBLoadingIndicator")]
public class BOBLoadingIndicatorAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Role_Status_On_Spinner(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert
        cut.Find("bob-component").GetAttribute("role").Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Custom_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>(p => p
            .Add(c => c.AriaLabel, "Processing your request"));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("Processing your request");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Role_Progressbar_On_Linear_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>(p => p
            .Add(c => c.Variant, BOBLoadingIndicatorVariant.LinearIndeterminate));

        // Assert
        cut.Find("bob-component").GetAttribute("role").Should().Be("progressbar");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_ValueMin_Max_On_Linear_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>(p => p
            .Add(c => c.Variant, BOBLoadingIndicatorVariant.LinearIndeterminate));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-valuemin").Should().Be("0");
        cut.Find("bob-component").GetAttribute("aria-valuemax").Should().Be("100");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Live_Polite_On_Spinner(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert
        cut.Find("bob-component").GetAttribute("aria-live").Should().Be("polite");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Svg_Be_Aria_Hidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLoadingIndicator> cut = ctx.Render<BOBLoadingIndicator>();

        // Assert — svg is decorative, hidden from screen readers
        cut.Find("svg").GetAttribute("aria-hidden").Should().Be("true");
    }
}
