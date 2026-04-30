using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Rendering", "BOBTreeMenu")]
public class BOBTreeMenuRenderingTests
{
    private sealed record MenuItem(string Key, string Label, IEnumerable<MenuItem>? Children = null, bool Disabled = false);

    private static IEnumerable<MenuItem> FlatItems
    => [
        new MenuItem("a", "Alpha"),
        new MenuItem("b", "Beta"),
    ];

    private static IEnumerable<MenuItem> NestedItems
    => [
        new MenuItem("parent", "Parent", [
            new MenuItem("child1", "Child 1"),
            new MenuItem("child2", "Child 2"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("tree-menu");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nav_With_Menubar_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("nav").GetAttribute("role").Should().Be("menubar");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Items_With_Menuitem_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.FindAll("[role='menuitem']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Disabled_Item_With_DataBuiDisabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — declarative mode required; imperative mode has no DisabledSelector
        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BOBTreeMenuItem>(0);
                b.AddAttribute(1, "Key", "x");
                b.AddAttribute(2, "Text", "X");
                b.AddAttribute(3, "Disabled", true);
                b.CloseComponent();
            }));

        // Assert
        cut.Find(".bob-tree-menu__item").GetAttribute("data-bob-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Items_With_Expanded_False_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Orientation_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.Orientation, TreeMenuOrientation.Horizontal));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("horizontal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Parent_Node_With_HasPopup(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-haspopup").Should().Be("true");
    }
}
