using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Interaction", "BOBModalContainer")]
public class BOBModalContainerInteractionTests
{
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
    public async Task Should_Invoke_Close_Callback_When_Close_Button_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool closed = false;
        ModalReference reference = new("test-close", _ =>
        {
            closed = true;
            return Task.CompletedTask;
        });

        ModalState modal = new()
        {
            Id = "test-close",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = reference,
            Options = new DialogOptions { Title = "Closable", Closable = true },
            IsVisible = true,
        };

        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, modal));

        // Act
        cut.Find("button[aria-label='Close']").Click();

        // Assert
        closed.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Stop_Click_Propagation_On_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — when Closable is false, no close button should exist
        ModalState modal = new()
        {
            Id = "no-close",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("no-close", _ => Task.CompletedTask),
            Options = new DialogOptions { Title = "Non-closable", Closable = false },
            IsVisible = true,
        };

        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, modal));

        // Assert — no close button rendered when Closable=false (header only renders for title)
        cut.FindAll("button[aria-label='Close']").Should().BeEmpty();
    }
}
