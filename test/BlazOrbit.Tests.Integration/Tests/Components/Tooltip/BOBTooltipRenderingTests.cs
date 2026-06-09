using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tooltip;

[Trait("Component Rendering", "BOBTooltip")]
public class BOBTooltipRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Wrapper_With_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Permanent)
            .Add(c => c.Text, "hint")
            .Add(c => c.ChildContent, b => b.AddContent(0, "trigger")));

        cut.Find("[data-bob-component=\"tooltip\"]").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Tooltip_When_Trigger_Is_Permanent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Permanent)
            .Add(c => c.Text, "always visible")
            .Add(c => c.ChildContent, b => b.AddContent(0, "trigger")));

        IElement tooltip = cut.Find("[role='tooltip']");
        tooltip.TextContent.Should().Contain("always visible");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Tooltip_Initially_For_Hover_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Hover)
            .Add(c => c.Text, "hint")
            .Add(c => c.ChildContent, b => b.AddContent(0, "trigger")));

        cut.FindAll("[role='tooltip']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Map_Placement_To_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Permanent)
            .Add(c => c.Placement, TooltipPlacement.Bottom)
            .Add(c => c.Text, "hint")
            .Add(c => c.ChildContent, b => b.AddContent(0, "trigger")));

        cut.Find("[role='tooltip']").GetAttribute("data-bob-placement").Should().Be("bottom");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Arrow_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Permanent)
            .Add(c => c.Arrow, false)
            .Add(c => c.Text, "hint")
            .Add(c => c.ChildContent, b => b.AddContent(0, "trigger")));

        cut.FindAll(".bob-tooltip__arrow").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_TooltipContent_Over_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Permanent)
            .Add(c => c.Text, "plain text")
            .Add(c => c.TooltipContent, b => b.AddMarkupContent(0, "<strong>rich</strong>"))
            .Add(c => c.ChildContent, b => b.AddContent(0, "trigger")));

        IElement tooltip = cut.Find("[role='tooltip']");
        // ChildContent slot supersedes the Text shorthand when both are set.
        tooltip.InnerHtml.Should().Contain("<strong>rich</strong>");
    }
}
