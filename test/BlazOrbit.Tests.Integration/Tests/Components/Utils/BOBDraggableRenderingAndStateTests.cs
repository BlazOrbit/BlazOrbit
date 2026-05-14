using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Utils;

[Trait("Component Rendering", "BOBDraggable")]
public class BOBDraggableRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Child_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDraggable> cut = ctx.Render<BOBDraggable>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Drag me")));

        cut.Find(".bob-draggable").TextContent.Should().Be("Drag me");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDraggable> cut = ctx.Render<BOBDraggable>();

        cut.Find(".bob-draggable").Should().NotBeNull();
    }
}

[Trait("Component Interaction", "BOBDraggable")]
public class BOBDraggableInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnDragStart_On_MouseDown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBDraggable> cut = ctx.Render<BOBDraggable>(p => p
            .Add(c => c.OnDragStart, _ =>
            {
                fired = true;
                return Task.CompletedTask;
            }));

        cut.Find(".bob-draggable").MouseDown(new MouseEventArgs { ClientX = 10, ClientY = 20 });

        fired.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_OnDragStart_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBDraggable> cut = ctx.Render<BOBDraggable>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OnDragStart, _ =>
            {
                fired = true;
                return Task.CompletedTask;
            }));

        cut.Find(".bob-draggable").MouseDown(new MouseEventArgs { ClientX = 0, ClientY = 0 });

        fired.Should().BeFalse();
    }
}

[Trait("Component Disposal", "BOBDraggable")]
public class BOBDraggableDisposalTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_Without_Throwing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDraggable> cut = ctx.Render<BOBDraggable>();

        // Trigger a mouse down so internal _isDragging becomes true
        cut.Find(".bob-draggable").MouseDown(new MouseEventArgs { ClientX = 0, ClientY = 0 });

        // Act & Assert — disposal must not throw even when dragging
        Func<Task> dispose = async () => await cut.Instance.DisposeAsync();
        await dispose.Should().NotThrowAsync();
    }
}