using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Accordion;

[Trait("Component Rendering", "BOBAccordion")]
public class BOBAccordionRenderingTests
{
    private static RenderFragment BuildTwoItems() => b =>
    {
        b.OpenComponent<BOBAccordionItem>(0);
        b.AddAttribute(1, "Id", "item1");
        b.AddAttribute(2, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "Section A")));
        b.AddAttribute(3, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Body A")));
        b.CloseComponent();
        b.OpenComponent<BOBAccordionItem>(4);
        b.AddAttribute(5, "Id", "item2");
        b.AddAttribute(6, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "Section B")));
        b.AddAttribute(7, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Body B")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildTwoItems()));

        // Assert
        cut.Find("bob-component[data-bob-component='accordion']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_All_Items(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildTwoItems()));

        // Assert
        cut.FindAll("[data-bob-component='accordion-item']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Item_Headers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildTwoItems()));

        // Assert
        var headers = cut.FindAll(".bob-accordion-item__header");
        headers.Should().HaveCount(2);
        headers[0].TextContent.Should().Contain("Section A");
        headers[1].TextContent.Should().Contain("Section B");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Item_Bodies_Always(BlazorScenario scenario)
    {
        // The grid 0fr→1fr animation requires the body to be in the DOM at all times;
        // collapse hides it via CSS, not via conditional rendering.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildTwoItems()));

        // Assert
        var bodies = cut.FindAll(".bob-accordion-item__body");
        bodies.Should().HaveCount(2);
        bodies[0].TextContent.Should().Contain("Body A");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_All_Items_Collapsed_In_Multiple_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildTwoItems()));

        // Assert
        foreach (var item in cut.FindAll("[data-bob-component='accordion-item']"))
        {
            item.GetAttribute("data-bob-expanded").Should().BeNull();
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Auto_Expand_First_Item_In_SingleStrict_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.Mode, BOBAccordionMode.SingleStrict)
            .Add(c => c.ChildContent, BuildTwoItems()));

        // Assert
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[0].GetAttribute("data-bob-expanded").Should().Be("true");
        items[1].GetAttribute("data-bob-expanded").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Respect_InitiallyExpanded_When_Uncontrolled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — second item starts open
        RenderFragment frag = b =>
        {
            b.OpenComponent<BOBAccordionItem>(0);
            b.AddAttribute(1, "Id", "i1");
            b.AddAttribute(2, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "A")));
            b.AddAttribute(3, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyA")));
            b.CloseComponent();
            b.OpenComponent<BOBAccordionItem>(4);
            b.AddAttribute(5, "Id", "i2");
            b.AddAttribute(6, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "B")));
            b.AddAttribute(7, "InitiallyExpanded", true);
            b.AddAttribute(8, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyB")));
            b.CloseComponent();
        };

        // Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, frag));

        // Assert
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[1].GetAttribute("data-bob-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Plus_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildTwoItems()));

        // Assert — each item header has an svg-icon child
        cut.FindAll(".bob-accordion-item__icon bob-component[data-bob-component='svg-icon']")
            .Should().HaveCount(2);
    }
}
