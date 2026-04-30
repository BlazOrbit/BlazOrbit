using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Interaction", "BOBTreeMenu")]
public class BOBTreeMenuInteractionTests
{
    private sealed record MenuItem(string Key, string Label, IEnumerable<MenuItem>? Children = null, bool Disabled = false);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnNodeClick_When_Item_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TreeNodeEventArgs<TreeMenuNode<MenuItem>>? captured = null;
        IEnumerable<MenuItem> items = [new MenuItem("a", "Alpha")];

        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.OnNodeClick, args => captured = args));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Key.Should().Be("a");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_OnNodeClick_When_Disabled_Item_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — declarative mode required; imperative mode has no DisabledSelector
        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BOBTreeMenuItem>(0);
                b.AddAttribute(1, "Key", "a");
                b.AddAttribute(2, "Text", "Alpha");
                b.AddAttribute(3, "Disabled", true);
                b.CloseComponent();
            }));

        // Assert — button rendered with disabled attribute, bUnit blocks click
        cut.Find("[role='menuitem']").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnNavigate_When_Leaf_Nav_Item_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — declarative mode to set Href
        string? navigatedHref = null;
        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BOBTreeMenuItem>(0);
                b.AddAttribute(1, "Key", "home");
                b.AddAttribute(2, "Text", "Home");
                b.AddAttribute(3, "Href", "/home");
                b.CloseComponent();
            })
            .Add(c => c.OnNavigate, href => navigatedHref = href));

        // bUnit renders; declarative mode triggers StateHasChanged after firstRender
        cut.Render();

        // Act
        cut.Find("a[role='menuitem']").Click();

        // Assert
        navigatedHref.Should().Be("/home");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expand_Parent_And_Show_Children_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IEnumerable<MenuItem> items = [
            new MenuItem("parent", "Parent", [
                new MenuItem("child", "Child"),
            ]),
        ];

        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        cut.FindAll("[role='menuitem']").Should().HaveCount(2);
        cut.Find(".bob-tree-menu__submenu").Should().NotBeNull();
    }
}
