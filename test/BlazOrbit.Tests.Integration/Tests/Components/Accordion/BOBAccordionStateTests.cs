using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Accordion;

[Trait("Component State", "BOBAccordion")]
public class BOBAccordionStateTests
{
    private static RenderFragment BuildItems() => b =>
    {
        b.OpenComponent<BOBAccordionItem>(0);
        b.AddAttribute(1, "Id", "a");
        b.AddAttribute(2, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "A")));
        b.AddAttribute(3, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyA")));
        b.CloseComponent();
        b.OpenComponent<BOBAccordionItem>(4);
        b.AddAttribute(5, "Id", "b");
        b.AddAttribute(6, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "B")));
        b.AddAttribute(7, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyB")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sync_From_Controlled_ExpandedItems(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ExpandedItems, new[] { "b" })
            .Add(c => c.ChildContent, BuildItems()));

        // Assert
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[0].GetAttribute("data-bob-expanded").Should().Be("false");
        items[1].GetAttribute("data-bob-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_When_ExpandedItems_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ExpandedItems, new[] { "a" })
            .Add(c => c.ChildContent, BuildItems()));

        // Act
        cut.Render(p => p
            .Add(c => c.ExpandedItems, new[] { "b" })
            .Add(c => c.ChildContent, BuildItems()));

        // Assert
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[0].GetAttribute("data-bob-expanded").Should().Be("false");
        items[1].GetAttribute("data-bob-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_InitiallyExpanded_When_Controlled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — controlled (empty list) + InitiallyExpanded should be ignored
        RenderFragment frag = b =>
        {
            b.OpenComponent<BOBAccordionItem>(0);
            b.AddAttribute(1, "Id", "a");
            b.AddAttribute(2, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "A")));
            b.AddAttribute(3, "InitiallyExpanded", true);
            b.AddAttribute(4, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyA")));
            b.CloseComponent();
        };

        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ExpandedItems, Array.Empty<string>())
            .Add(c => c.ChildContent, frag));

        // Assert
        cut.Find("[data-bob-component='accordion-item']")
            .GetAttribute("data-bob-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_IsItemExpanded_Public_Query(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ExpandedItems, new[] { "a" })
            .Add(c => c.ChildContent, BuildItems()));

        // Act + Assert
        cut.Instance.IsItemExpanded("a").Should().BeTrue();
        cut.Instance.IsItemExpanded("b").Should().BeFalse();
        cut.Instance.IsItemExpanded("nonexistent").Should().BeFalse();
    }
}
