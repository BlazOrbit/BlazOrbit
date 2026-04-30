using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeSelector;

[Trait("Component State", "BOBTreeSelector")]
public class BOBTreeSelectorStateTests
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
            new SelectItem("child1", "Child 1"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_Item_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Act
        cut.FindAll(".bob-tree-selector__node-content")[0].Click();

        // Assert
        cut.FindAll("[role='treeitem']")[0].GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Deselect_Item_On_Second_Click_In_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.SelectionMode, TreeSelectionMode.Single));

        cut.FindAll(".bob-tree-selector__node-content")[0].Click();
        cut.FindAll("[role='treeitem']")[0].GetAttribute("aria-selected").Should().Be("true");

        // Act
        cut.FindAll(".bob-tree-selector__node-content")[0].Click();

        // Assert
        cut.FindAll("[role='treeitem']")[0].GetAttribute("aria-selected").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Replace_Selection_In_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.SelectionMode, TreeSelectionMode.Single));

        cut.FindAll(".bob-tree-selector__node-content")[0].Click();

        // Act — click second item
        cut.FindAll(".bob-tree-selector__node-content")[1].Click();

        // Assert — first deselected, second selected
        cut.FindAll("[role='treeitem']")[0].GetAttribute("aria-selected").Should().Be("false");
        cut.FindAll("[role='treeitem']")[1].GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_Multi_Select_In_Multiple_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.SelectionMode, TreeSelectionMode.Multiple));

        // Act
        cut.FindAll(".bob-tree-selector__node-content")[0].Click();
        cut.FindAll(".bob-tree-selector__node-content")[1].Click();

        // Assert — both selected
        cut.FindAll("[role='treeitem']")[0].GetAttribute("aria-selected").Should().Be("true");
        cut.FindAll("[role='treeitem']")[1].GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_SelectedKeysChanged_On_Selection(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        HashSet<string>? captured = null;
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.SelectedKeysChanged, keys => captured = keys));

        // Act
        cut.FindAll(".bob-tree-selector__node-content")[0].Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Should().Contain("a");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expand_Node_On_Expander_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Act — click expander button (not the node content)
        cut.Find(".bob-tree-selector__expander").Click();

        // Assert
        cut.Find("[role='treeitem']").GetAttribute("aria-expanded").Should().Be("true");
        cut.Find("[role='group']").Should().NotBeNull();
    }
}
