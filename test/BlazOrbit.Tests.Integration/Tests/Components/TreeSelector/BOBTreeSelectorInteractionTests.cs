using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeSelector;

[Trait("Component Interaction", "BOBTreeSelector")]
public class BOBTreeSelectorInteractionTests
{
    private sealed record SelectItem(string Key, string Label, IEnumerable<SelectItem>? Children = null);

    private static IEnumerable<SelectItem> FlatItems
    => [
        new SelectItem("a", "Alpha"),
        new SelectItem("b", "Beta"),
    ];

    private static IEnumerable<SelectItem> NestedItems
    => [
        new SelectItem("parent", "Parent", [
            new SelectItem("child", "Child"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnNodeClick_When_Node_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TreeNodeEventArgs<TreeSelectionNode<SelectItem>>? captured = null;
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.OnNodeClick, args => captured = args));

        // Act
        cut.FindAll(".bob-tree-selector__node-content")[0].Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Key.Should().Be("a");
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
        cut.FindAll(".bob-tree-selector__node-content")[0]
            .KeyDown(new KeyboardEventArgs { Key = " " });

        // Assert
        cut.FindAll("[role='treeitem']")[0].GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expand_With_ArrowRight_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Act
        cut.FindAll(".bob-tree-selector__node-content")[0]
            .KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        cut.Find("[role='treeitem']").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Collapse_With_ArrowLeft_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children)
            .Add(c => c.ExpandAll, true));

        // Act
        cut.FindAll(".bob-tree-selector__node-content")[0]
            .KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        cut.Find("[role='treeitem']").GetAttribute("aria-expanded").Should().Be("false");
    }
}
