using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BOBModalContainer")]
public class BOBModalContainerRenderingTests
{
    private static ModalState CreateDialogState(string title = "Test Dialog")
        => new()
        {
            Id = "test-modal-1",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("test-modal-1", _ => Task.CompletedTask),
            Options = new DialogOptions { Title = title },
            IsVisible = true,
        };

    private static ModalState CreateDrawerState(DrawerPosition position = DrawerPosition.Right)
        => new()
        {
            Id = "test-drawer-1",
            Type = ModalType.Drawer,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("test-drawer-1", _ => Task.CompletedTask),
            Options = new DrawerOptions { Position = position },
            IsVisible = true,
        };

    private sealed class DummyModalContent : Microsoft.AspNetCore.Components.ComponentBase, IModalContent
    {
        [Microsoft.AspNetCore.Components.Parameter]
        public ModalReference ModalRef { get; set; } = default!;

        // IModalContent requires this property — delegated to ModalRef
        ModalReference IModalContent.ModalReference
        {
            get => ModalRef;
            set => ModalRef = value;
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState()));

        // Assert
        cut.Find(".bob-modal-dialog").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Drawer_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDrawerState()));

        // Assert
        cut.Find(".bob-modal-drawer").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState("My Title")));

        // Assert
        cut.Find(".bob-modal-dialog__title").TextContent.Should().Be("My Title");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Drawer_Position_Class(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDrawerState(DrawerPosition.Left)));

        // Assert
        cut.Find("[data-bob-position='left']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Visible_Class_When_Visible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState()));

        // Assert
        cut.Find("[data-bob-visible='true']").Should().NotBeNull();
    }
}
