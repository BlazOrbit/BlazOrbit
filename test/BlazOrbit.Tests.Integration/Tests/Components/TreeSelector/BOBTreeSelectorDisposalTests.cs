using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeSelector;

[Trait("Component Disposal", "BOBTreeSelector")]
public class BOBTreeSelectorDisposalTests
{
    private sealed record SelectItem(string Key, string Label, IEnumerable<SelectItem>? Children = null);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, [new SelectItem("a", "Alpha")])
            .Add(c => c.KeySelector, m => m.Key));

        // Act + Assert
        Func<Task> act = () => { cut.Instance.Dispose(); return Task.CompletedTask; };
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_After_Selection_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeSelector<SelectItem>> cut = ctx.Render<BOBTreeSelector<SelectItem>>(p => p
            .Add(c => c.Items, [
                new SelectItem("parent", "Parent", [new SelectItem("child", "Child")])
            ])
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children)
            .Add(c => c.SelectionMode, TreeSelectionMode.Multiple));

        // Act — select item then dispose
        cut.FindAll(".bob-tree-selector__node-content")[0].Click();
        Func<Task> act = () => { cut.Instance.Dispose(); return Task.CompletedTask; };
        await act.Should().NotThrowAsync();
    }
}
