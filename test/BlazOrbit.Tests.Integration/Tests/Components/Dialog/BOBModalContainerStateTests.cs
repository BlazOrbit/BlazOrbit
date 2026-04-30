using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component State", "BOBModalContainer")]
public class BOBModalContainerStateTests
{
    private static ModalState CreateDialogState(bool isVisible = true, bool isAnimatingOut = false, int? elevation = null)
        => new()
        {
            Id = "test-state-modal",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("test-state-modal", _ => Task.CompletedTask),
            Options = new DialogOptions { Title = "State Test", Elevation = elevation },
            IsVisible = isVisible,
            IsAnimatingOut = isAnimatingOut,
        };

    private static ModalState CreateDrawerState(bool isVisible = true, bool isAnimatingOut = false, int? elevation = null)
        => new()
        {
            Id = "test-state-drawer",
            Type = ModalType.Drawer,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("test-state-drawer", _ => Task.CompletedTask),
            Options = new DrawerOptions { Position = DrawerPosition.Right, Elevation = elevation },
            IsVisible = isVisible,
            IsAnimatingOut = isAnimatingOut,
        };

    private sealed class DummyModalContent : Microsoft.AspNetCore.Components.ComponentBase, IModalContent
    {
        [Microsoft.AspNetCore.Components.Parameter]
        public ModalReference ModalRef { get; set; } = default!;

        ModalReference IModalContent.ModalReference
        {
            get => ModalRef;
            set => ModalRef = value;
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Hidden_When_Not_Visible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState(isVisible: false)));

        // Assert
        cut.Find(".bob-modal-container--hidden").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Closing_Animation_When_AnimatingOut(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState(isAnimatingOut: true)));

        // Assert
        cut.Find(".bob-modal-container--closing").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Re_Render_When_Modal_State_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState(isVisible: true)));
        cut.Find(".bob-modal-container--visible").Should().NotBeNull();

        // Act — flip to hidden
        cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState(isVisible: false)));

        // Assert
        cut.Find(".bob-modal-container--hidden").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Elevation_Styles_To_Dialog(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState(elevation: 3)));

        // Assert
        IElement dialog = cut.Find(".bob-modal-dialog");
        string style = dialog.GetAttribute("style") ?? "";
        style.Should().Contain("--bob-inline-elevation-tint: 11%");
        style.Should().Contain("--bob-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Elevation_Styles_To_Drawer(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, CreateDrawerState(elevation: 2)));

        // Assert
        IElement drawer = cut.Find(".bob-modal-drawer");
        string style = drawer.GetAttribute("style") ?? "";
        style.Should().Contain("--bob-inline-elevation-tint: 8%");
        style.Should().Contain("--bob-inline-shadow:");
    }
}
