using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component State", "BOBTreeMenu")]
public class BOBTreeMenuStateTests
{
    private sealed record MenuItem(string Key, string Label, IEnumerable<MenuItem>? Children = null);

    private static IEnumerable<MenuItem> NestedItems
    => [
        new MenuItem("parent", "Parent", [
            new MenuItem("child1", "Child 1"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expand_Node_After_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Collapse_Node_On_Second_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        cut.Find("[role='menuitem']").Click();
        cut.Find("[role='menuitem']").GetAttribute("aria-expanded").Should().Be("true");

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Children_When_Expanded(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert — submenu rendered with child items
        cut.Find("[role='menu']").Should().NotBeNull();
        cut.FindAll("[role='menuitem']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ExpandedKeysChanged_On_Expand(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        HashSet<string>? capturedKeys = null;
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children)
            .Add(c => c.ExpandedKeysChanged, keys => capturedKeys = keys));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        capturedKeys.Should().NotBeNull();
        capturedKeys!.Should().Contain("parent");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expand_All_When_ExpandAll_Is_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children)
            .Add(c => c.ExpandAll, true));

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-expanded").Should().Be("true");
        cut.Find("[role='menu']").Should().NotBeNull();
    }
}
