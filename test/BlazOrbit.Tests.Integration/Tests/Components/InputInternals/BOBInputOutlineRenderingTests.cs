using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BOBInputOutline")]
public class BOBInputOutlineRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>();

        // Assert
        cut.FindAll("label.bob-input__label").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_Inside_Notch(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>(p => p
            .Add(c => c.Label, "Full name")
            .Add(c => c.For, "inp-42"));

        // Assert
        IElement notch = cut.Find("span.bob-input__outline-notch");
        IElement label = notch.QuerySelector("label.bob-input__label")!;
        label.Should().NotBeNull();
        label.GetAttribute("for").Should().Be("inp-42");
        label.TextContent.Should().Contain("Full name");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_Without_Inner_Required_Span(BlazorScenario scenario)
    {
        // CSS-OPT-02 block B.5: the required asterisk is no longer a Razor-emitted
        // <span class="bob-input__required">; it is rendered by the input-family CSS
        // via [data-bob-required="true"] .bob-input__label::after on the host. The
        // BOBInputOutline partial therefore renders only the label text, regardless
        // of the Required parameter (kept for binary compat).
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>(p => p
            .Add(c => c.Label, "Email")
            .Add(c => c.Required, true));

        // Assert
        IElement label = cut.Find("label.bob-input__label");
        label.TextContent.Should().Be("Email");
        cut.FindAll("span.bob-input__required").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Required_Marker_When_Required_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>(p => p
            .Add(c => c.Label, "Email"));

        // Assert
        cut.FindAll("span.bob-input__required").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Leave_Markup_Empty_When_Label_Whitespace(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>(p => p
            .Add(c => c.Label, "   "));

        // Assert
        cut.Markup.Should().BeEmpty();
    }
}
