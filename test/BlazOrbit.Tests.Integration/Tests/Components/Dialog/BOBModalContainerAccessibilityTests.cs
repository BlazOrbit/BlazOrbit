using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Accessibility", "BOBModalContainer")]
public class BOBModalContainerAccessibilityTests
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
    public async Task Should_Render_Dialog_Title_As_Heading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        ModalState modal = new()
        {
            Id = "a11y-1",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("a11y-1", _ => Task.CompletedTask),
            Options = new DialogOptions { Title = "My Dialog" },
            IsVisible = true,
        };
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, modal));

        // Assert — h2 carries the title for screen readers
        IElement h2 = cut.Find("h2.bob-modal-dialog__title");
        h2.TextContent.Should().Be("My Dialog");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Provide_AriaLabel_On_Close_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        ModalState modal = new()
        {
            Id = "a11y-close",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("a11y-close", _ => Task.CompletedTask),
            Options = new DialogOptions { Title = "Closable", Closable = true },
            IsVisible = true,
        };
        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, modal));

        // Assert
        cut.Find("button[aria-label='Close']").Should().NotBeNull();
    }
}
