using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Link;

[Trait("Component Accessibility", "BOBLink")]
public class BOBLinkAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Rel_Or_Target_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/about")
            .Add(c => c.Text, "About"));

        // Assert
        cut.Find("a").HasAttribute("target").Should().BeFalse();
        cut.Find("a").HasAttribute("rel").Should().BeFalse();
        cut.FindAll("a .sr-only").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Add_Noopener_Noreferrer_When_Target_Is_Blank(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "https://example.com")
            .Add(c => c.Text, "External")
            .Add(c => c.Target, "_blank"));

        // Assert - reverse tabnabbing protection
        cut.Find("a").GetAttribute("target").Should().Be("_blank");
        cut.Find("a").GetAttribute("rel")!.Split(' ').Should().BeEquivalentTo("noopener", "noreferrer");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Merge_Rel_Without_Duplicates_When_Target_Is_Blank(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "https://example.com")
            .Add(c => c.Text, "Sponsor")
            .Add(c => c.Target, "_BLANK")
            .Add(c => c.Rel, "sponsored  NOOPENER"));

        // Assert
        cut.Find("a").GetAttribute("rel")!.Split(' ').Should().BeEquivalentTo("sponsored", "NOOPENER", "noreferrer");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Rel_As_Is_When_Target_Is_Not_Blank(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/terms")
            .Add(c => c.Text, "Terms")
            .Add(c => c.Rel, "license"));

        // Assert
        cut.Find("a").GetAttribute("rel").Should().Be("license");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Announce_New_Tab_To_Screen_Readers_When_Target_Is_Blank(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "https://example.com")
            .Add(c => c.Text, "External")
            .Add(c => c.Target, "_blank"));

        // Assert - WCAG G201: warn users in advance that a new window opens
        cut.Find("a .sr-only").TextContent.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_AriaLabel_To_Anchor(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/settings")
            .Add(c => c.LeadingIcon, BOBIconKeys.MaterialIconsOutlined.i_check)
            .Add(c => c.AriaLabel, "Settings"));

        // Assert - WCAG 4.1.2 Name, Role, Value for icon-only links
        cut.Find("a").GetAttribute("aria-label").Should().Be("Settings");
    }
}
