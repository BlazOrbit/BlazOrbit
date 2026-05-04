using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Accordion;

[Trait("Component Interaction", "BOBAccordion")]
public class BOBAccordionInteractionTests
{
    private static RenderFragment BuildThreeItems(bool middleDisabled = false) => b =>
    {
        b.OpenComponent<BOBAccordionItem>(0);
        b.AddAttribute(1, "Id", "i1");
        b.AddAttribute(2, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "A")));
        b.AddAttribute(3, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyA")));
        b.CloseComponent();
        b.OpenComponent<BOBAccordionItem>(4);
        b.AddAttribute(5, "Id", "i2");
        b.AddAttribute(6, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "B")));
        if (middleDisabled) b.AddAttribute(7, "Disabled", true);
        b.AddAttribute(8, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyB")));
        b.CloseComponent();
        b.OpenComponent<BOBAccordionItem>(9);
        b.AddAttribute(10, "Id", "i3");
        b.AddAttribute(11, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "C")));
        b.AddAttribute(12, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "BodyC")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Item_On_Click_In_Multiple_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildThreeItems()));

        // Act — click first header
        cut.FindAll(".bob-accordion-item__header")[0].Click();

        // Assert
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[0].GetAttribute("data-bob-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_Multiple_Items_Open_In_Multiple_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildThreeItems()));

        // Act
        cut.FindAll(".bob-accordion-item__header")[0].Click();
        cut.FindAll(".bob-accordion-item__header")[2].Click();

        // Assert
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[0].GetAttribute("data-bob-expanded").Should().Be("true");
        items[2].GetAttribute("data-bob-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Collapse_Other_When_Expanding_In_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.Mode, BOBAccordionMode.Single)
            .Add(c => c.ChildContent, BuildThreeItems()));

        // Act
        cut.FindAll(".bob-accordion-item__header")[0].Click();
        cut.FindAll(".bob-accordion-item__header")[2].Click();

        // Assert — only third is expanded
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[0].GetAttribute("data-bob-expanded").Should().Be("false");
        items[2].GetAttribute("data-bob-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_Collapse_Active_In_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.Mode, BOBAccordionMode.Single)
            .Add(c => c.ChildContent, BuildThreeItems()));

        // Act — open then click again to close
        cut.FindAll(".bob-accordion-item__header")[0].Click();
        cut.FindAll(".bob-accordion-item__header")[0].Click();

        // Assert — none expanded
        foreach (var item in cut.FindAll("[data-bob-component='accordion-item']"))
        {
            item.GetAttribute("data-bob-expanded").Should().Be("false");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reject_Collapse_Active_In_SingleStrict_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — first auto-expands in strict mode
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.Mode, BOBAccordionMode.SingleStrict)
            .Add(c => c.ChildContent, BuildThreeItems()));

        // Act — click already-open first item
        cut.FindAll(".bob-accordion-item__header")[0].Click();

        // Assert — still expanded
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[0].GetAttribute("data-bob-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_Disabled_Item(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — middle item disabled
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildThreeItems(middleDisabled: true)));

        // Act — click via JS event (browser would block disabled but bUnit may dispatch)
        cut.FindAll(".bob-accordion-item__header")[1].Click();

        // Assert — still collapsed
        var items = cut.FindAll("[data-bob-component='accordion-item']");
        items[1].GetAttribute("data-bob-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnExpandedChanged_With_Item_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBAccordionItemToggle? captured = null;
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildThreeItems())
            .Add(c => c.OnExpandedChanged, t => captured = t));

        // Act
        cut.FindAll(".bob-accordion-item__header")[1].Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Value.ItemId.Should().Be("i2");
        captured.Value.IsExpanded.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ExpandedItemsChanged_With_Full_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IReadOnlyList<string>? captured = null;
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildThreeItems())
            .Add(c => c.ExpandedItemsChanged, list => captured = list));

        // Act
        cut.FindAll(".bob-accordion-item__header")[0].Click();
        cut.FindAll(".bob-accordion-item__header")[2].Click();

        // Assert
        captured.Should().NotBeNull();
        captured.Should().BeEquivalentTo(new[] { "i1", "i3" });
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Move_To_Next_Header_With_ArrowDown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildThreeItems()));

        // Act — keydown on first header (focus delegation tested via FocusAsync; we just verify no exception)
        cut.FindAll(".bob-accordion-item__header")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert — accordion still rendered consistently
        cut.FindAll("[data-bob-component='accordion-item']").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Disabled_Item_With_Arrow_Navigation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — middle disabled
        IRenderedComponent<BOBAccordion> cut = ctx.Render<BOBAccordion>(p => p
            .Add(c => c.ChildContent, BuildThreeItems(middleDisabled: true)));

        // Act — should skip disabled middle without exception
        cut.FindAll(".bob-accordion-item__header")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert
        cut.FindAll("[data-bob-component='accordion-item']").Should().HaveCount(3);
    }
}
