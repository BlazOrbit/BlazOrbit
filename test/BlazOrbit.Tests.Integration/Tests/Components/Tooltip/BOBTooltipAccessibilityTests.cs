using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tooltip;

[Trait("Component Accessibility", "BOBTooltip")]
public class BOBTooltipAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pair_AriaDescribedBy_With_Tooltip_Id_When_Visible(BlazorScenario scenario)
    {
        // WCAG 4.1.2: the focusable trigger must reference the tooltip's id via
        // aria-describedby so screen readers announce the description when focus
        // lands on the trigger.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — Permanent trigger keeps the tooltip mounted from first render
        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Permanent)
            .Add(c => c.Text, "Helpful hint")
            .Add(c => c.ChildContent, b => b.AddContent(0, "Trigger")));

        // Act — capture the rendered ids
        IElement wrapper = cut.Find("[data-bob-component=\"tooltip\"]");
        IElement tooltip = cut.Find("[role='tooltip']");

        // Assert — the wrapper's aria-describedby points at the tooltip's id
        string? descId = wrapper.GetAttribute("aria-describedby");
        string? tooltipId = tooltip.GetAttribute("id");

        descId.Should().NotBeNullOrEmpty();
        tooltipId.Should().NotBeNullOrEmpty();
        descId.Should().Be(tooltipId);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Omit_AriaDescribedBy_When_Tooltip_Hidden(BlazorScenario scenario)
    {
        // When the tooltip is not rendered, the wrapper must not declare a
        // dangling aria-describedby pointing at a non-existent id.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — Hover trigger, no interaction yet → tooltip hidden
        IRenderedComponent<BOBTooltip> cut = ctx.Render<BOBTooltip>(p => p
            .Add(c => c.Trigger, TooltipTrigger.Hover)
            .Add(c => c.Text, "Helpful hint")
            .Add(c => c.ChildContent, b => b.AddContent(0, "Trigger")));

        // Assert — no tooltip in DOM, no aria-describedby on wrapper
        cut.FindAll("[role='tooltip']").Should().BeEmpty();
        cut.Find("[data-bob-component=\"tooltip\"]").HasAttribute("aria-describedby").Should().BeFalse();
    }
}
