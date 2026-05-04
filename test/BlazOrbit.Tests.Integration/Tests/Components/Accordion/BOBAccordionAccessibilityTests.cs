using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Accordion;

[Trait("Component Accessibility", "BOBAccordion")]
public class BOBAccordionAccessibilityTests
{
    private static RenderFragment TwoItems() => b =>
    {
        b.OpenComponent<BOBAccordionItem>(0);
        b.AddAttribute(1, "Id", "i1");
        b.AddAttribute(2, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "Header A")));
        b.AddAttribute(3, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Body A")));
        b.CloseComponent();
        b.OpenComponent<BOBAccordionItem>(4);
        b.AddAttribute(5, "Id", "i2");
        b.AddAttribute(6, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "Header B")));
        b.AddAttribute(7, "Disabled", true);
        b.AddAttribute(8, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Body B")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Header_Be_Button_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, TwoItems()));

        // Assert
        cut.FindAll("button.bob-accordion-item__header").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Header_Have_Aria_Expanded_Reflecting_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — first expanded via controlled
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ExpandedItems, new[] { "i1" })
            .Add(c => c.ChildContent, TwoItems()));

        // Assert
        var headers = cut.FindAll(".bob-accordion-item__header");
        headers[0].GetAttribute("aria-expanded").Should().Be("true");
        headers[1].GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Header_Aria_Controls_Match_Body_Id(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, TwoItems()));

        // Assert
        cut.FindAll(".bob-accordion-item__header")[0]
            .GetAttribute("aria-controls").Should().Be("bob-accordion-body-i1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Body_Have_Region_Role_And_Labelledby(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, TwoItems()));

        // Assert
        var body = cut.Find("#bob-accordion-body-i1");
        body.GetAttribute("role").Should().Be("region");
        body.GetAttribute("aria-labelledby").Should().Be("bob-accordion-header-i1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disabled_Header_Have_Disabled_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, TwoItems()));

        // Assert
        var headers = cut.FindAll(".bob-accordion-item__header");
        headers[1].HasAttribute("disabled").Should().BeTrue();
        headers[1].GetAttribute("aria-disabled").Should().Be("true");
    }
}
