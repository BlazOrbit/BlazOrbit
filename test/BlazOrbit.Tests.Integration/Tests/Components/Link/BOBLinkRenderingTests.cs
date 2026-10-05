using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Link;

[Trait("Component Rendering", "BOBLink")]
public class BOBLinkRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/docs")
            .Add(c => c.Text, "Docs")
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Shadow, BOBShadowPresets.Elevation(4)));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("link");
        root.GetAttribute("data-bob-variant").Should().Be("text");
        root.GetAttribute("data-bob-size").Should().Be("large");
        root.GetAttribute("data-bob-shadow").Should().Be("true");
        root.GetAttribute("style").Should().Contain("--bob-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Anchor_With_Href_And_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/contact")
            .Add(c => c.Text, "Contact"));

        // Assert
        IElement anchor = cut.Find("a");
        anchor.GetAttribute("href").Should().Be("/contact");
        anchor.QuerySelector(".bob-link__text")!.TextContent.Should().Be("Contact");
        cut.FindAll("button").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_Instead_Of_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/")
            .Add(c => c.Text, "Ignored")
            .AddChildContent("<strong class='custom'>Home</strong>"));

        // Assert
        cut.Find("a .custom").TextContent.Should().Be("Home");
        cut.FindAll(".bob-link__text").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Leading_And_Trailing_Icons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/next")
            .Add(c => c.Text, "Next")
            .Add(c => c.LeadingIcon, BOBIconKeys.MaterialIconsOutlined.i_check)
            .Add(c => c.TrailingIcon, BOBIconKeys.MaterialIconsOutlined.i_open_in_new));

        // Assert
        cut.FindAll("a .bob-link__icon--leading").Should().HaveCount(1);
        cut.FindAll("a .bob-link__icon--trailing").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Anchor_As_Transition_Target(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/")
            .Add(c => c.Text, "Home")
            .Add(c => c.Transitions, BOBTransitionPresets.HoverLift));

        // Assert
        cut.Find("bob-component").HasAttribute("data-bob-transitions").Should().BeTrue();
        cut.Find("a").ClassList.Should().Contain("transition-target");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_FullWidth_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/")
            .Add(c => c.Text, "Home")
            .Add(c => c.FullWidth, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-fullwidth").Should().Be("true");
    }
}
