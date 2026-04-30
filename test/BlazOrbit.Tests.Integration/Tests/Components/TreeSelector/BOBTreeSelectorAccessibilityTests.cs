using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeSelector;

[Trait("Component Accessibility", "BOBTreeSelector")]
public class BOBTreeSelectorAccessibilityTests
{
    private sealed record SelectItem(string Key, string Label, IEnumerable<SelectItem>? Children = null, bool Disabled = false);

    private static IEnumerable<SelectItem> FlatItems
    => [
        new SelectItem("a", "Alpha"),
        new SelectItem("b", "Beta"),
    ];

    private static IEnumerable<SelectItem> NestedItems
    => [
        new SelectItem("parent", "Parent", [
            new SelectItem("child1", "Child 1"),
            new SelectItem("child2", "Child 2"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Container_Have_Role_Tree(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find(".bob-tree-selector__container").GetAttribute("role").Should().Be("tree");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Container_Advertise_Aria_Multiselectable_In_Multiple_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — Blazor renders bool-true attrs as present-with-empty-value.
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.SelectionMode, TreeSelectionMode.Multiple));

        // Assert
        cut.Find("[role='tree']").HasAttribute("aria-multiselectable").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Container_Omit_Aria_Multiselectable_In_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — Blazor omits bool-false attrs entirely.
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("[role='tree']").HasAttribute("aria-multiselectable").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Each_Node_Have_Role_Treeitem(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert — two root items, both treeitem.
        cut.FindAll("[role='treeitem']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Parent_Node_Have_Aria_Expanded_False_When_Collapsed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Assert
        cut.Find("[data-key='parent']").GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Parent_Node_Have_Aria_Expanded_True_After_ExpandAll(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children)
            .Add(c => c.ExpandAll, true));

        // Assert
        cut.Find("[data-key='parent']").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Leaf_Node_Not_Have_Aria_Expanded(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — leaves shouldn't advertise an expand state (it's null in the source).
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("[data-key='a']").GetAttribute("aria-expanded").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Aria_Selected_After_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        cut.Find("[data-key='a']").GetAttribute("aria-selected").Should().Be("false");

        // Act
        cut.Find("[data-key='a'] .bob-tree-selector__node-content").Click();

        // Assert
        cut.Find("[data-key='a']").GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expand_Parent_With_ArrowRight(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        cut.Find("[data-key='parent']").GetAttribute("aria-expanded").Should().Be("false");

        // Act — ArrowRight on a collapsed parent expands it.
        cut.Find("[data-key='parent'] .bob-tree-selector__node-content")
            .KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        cut.Find("[data-key='parent']").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Collapse_Parent_With_ArrowLeft(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children)
            .Add(c => c.ExpandAll, true));

        cut.Find("[data-key='parent']").GetAttribute("aria-expanded").Should().Be("true");

        // Act
        cut.Find("[data-key='parent'] .bob-tree-selector__node-content")
            .KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        cut.Find("[data-key='parent']").GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Selection_With_Enter_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Act
        cut.Find("[data-key='a'] .bob-tree-selector__node-content")
            .KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        cut.Find("[data-key='a']").GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Selection_With_Space_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Act
        cut.Find("[data-key='a'] .bob-tree-selector__node-content")
            .KeyDown(new KeyboardEventArgs { Key = " " });

        // Assert
        cut.Find("[data-key='a']").GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Enabled_Node_Content_Focusable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert — each enabled node content row exposes tabindex=0.
        IReadOnlyList<IElement> contents = cut.FindAll(".bob-tree-selector__node-content");
        contents.Should().OnlyContain(c => c.GetAttribute("tabindex") == "0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expander_Button_Carry_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — collapsed parent => "Expand".
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Assert
        cut.Find(".bob-tree-selector__expander").GetAttribute("aria-label").Should().Be("Expand");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Children_Group_Have_Role_Group(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children)
            .Add(c => c.ExpandAll, true));

        // Assert
        cut.Find(".bob-tree-selector__children").GetAttribute("role").Should().Be("group");
    }
}
