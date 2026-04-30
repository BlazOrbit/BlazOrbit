using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeSelector;

[Trait("Component Rendering", "BOBTreeSelector")]
public class BOBTreeSelectorRenderingTests
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
            new SelectItem("child2", "Child 2"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("tree-selector");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Container_With_Tree_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("[role='tree']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Items_With_Treeitem_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.FindAll("[role='treeitem']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Checkboxes_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.FindAll("[role='checkbox']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Checkboxes_When_ShowCheckboxes_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ShowCheckboxes, false));

        // Assert
        cut.FindAll("[role='checkbox']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_SelectionMode_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.SelectionMode, TreeSelectionMode.Multiple));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-selection-mode").Should().Be("multiple");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Parent_With_Expander_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Assert — parent node has expander button
        cut.Find(".bob-tree-selector__expander").Should().NotBeNull();
    }
}
