using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Accessibility", "BOBThemePreview")]
public class BOBThemePreviewAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Section_Headings_For_Each_Block(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Assert — every section opens with an h5 heading so the preview is screen-reader-navigable
        IReadOnlyList<IElement> sections = cut.FindAll(".bob-theme-preview__section");
        sections.Should().NotBeEmpty();
        foreach (IElement section in sections)
        {
            IElement? heading = section.QuerySelector("h5");
            heading.Should().NotBeNull();
            heading!.TextContent.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Decorative_Icons_As_Aria_Hidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Assert — every svg in the preview is decorative
        IReadOnlyList<IElement> svgs = cut.FindAll("svg");
        svgs.Should().NotBeEmpty();
        foreach (IElement svg in svgs)
        {
            svg.GetAttribute("aria-hidden").Should().Be("true");
        }
    }
}
