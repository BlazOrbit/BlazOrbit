using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Disposal", "BOBTreeMenu")]
public class BOBTreeMenuDisposalTests
{
    private sealed record MenuItem(string Key, string Label, IEnumerable<MenuItem>? Children = null);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, [new MenuItem("a", "Alpha")])
            .Add(c => c.KeySelector, m => m.Key));

        // Act + Assert
        Func<Task> act = () => { cut.Instance.Dispose(); return Task.CompletedTask; };
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_After_Navigation_Subscription_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — render with nested items so nav subscription + expand are tested
        IRenderedComponent<BOBTreeMenu<MenuItem>> cut = ctx.Render<BOBTreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, [
                new MenuItem("parent", "Parent", [new MenuItem("child", "Child")])
            ])
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        cut.Find("[role='menuitem']").Click(); // expand

        // Act + Assert — dispose unsubscribes LocationChanged, no exception
        Func<Task> act = () => { cut.Instance.Dispose(); return Task.CompletedTask; };
        await act.Should().NotThrowAsync();
    }
}
