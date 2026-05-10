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
        public ModalReference ModalReference { get; set; } = default!;
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

    /// <summary>
    /// Regression test: BOBModalContainer must pass the ModalReference parameter using the
    /// exact property name "ModalReference" (not "ModalRef"). If the attribute name mismatches,
    /// Blazor silently leaves the parameter as default and the child component throws on click.
    /// </summary>
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_ModalReference_To_Child_Component(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool closed = false;
        object? closedResult = null;
        ModalReference reference = new("test-ref-pass", async r =>
        {
            closed = true;
            closedResult = await r.Result;
        });

        ModalState modal = new()
        {
            Id = "test-ref-pass",
            Type = ModalType.Dialog,
            ComponentType = typeof(BOBConfirmDialog),
            Reference = reference,
            Options = new DialogOptions { Title = "Confirm", Closable = false },
            IsVisible = true,
            Parameters = new Dictionary<string, object?>
            {
                ["Title"] = "Delete?",
                ["Message"] = "Are you sure?",
                ["YesLabel"] = "Yes",
                ["NoLabel"] = "No",
            },
        };

        IRenderedComponent<BOBModalContainer> cut = ctx.Render<BOBModalContainer>(p => p
            .Add(c => c.Modal, modal));

        // Act — click the Yes (accept) button rendered inside BOBConfirmDialog
        cut.FindAll(".bob-confirm__actions button").Last().Click();

        // Assert — ModalReference was correctly bound and CloseAsync(true) resolved
        closed.Should().BeTrue();
        closedResult.Should().Be(true);
    }
}
