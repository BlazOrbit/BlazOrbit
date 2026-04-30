using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Snapshots", "BOBModalContainer")]
public class BOBModalContainerSnapshotTests
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

    private static ModalState DialogState(string title, bool closable = false, bool isVisible = true)
        => new()
        {
            Id = "snap-dialog",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("snap-dialog", _ => Task.CompletedTask),
            Options = new DialogOptions { Title = title, Closable = closable },
            IsVisible = isVisible,
        };

    private static ModalState DrawerState(DrawerPosition position)
        => new()
        {
            Id = "snap-drawer",
            Type = ModalType.Drawer,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("snap-drawer", _ => Task.CompletedTask),
            Options = new DrawerOptions { Position = position },
            IsVisible = true,
        };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Dialog_Default",
                Html = ctx.Render<BOBModalContainer>(p => p
                    .Add(c => c.Modal, DialogState("Plain Dialog"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Dialog_Closable",
                Html = ctx.Render<BOBModalContainer>(p => p
                    .Add(c => c.Modal, DialogState("Closable Dialog", closable: true))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Drawer_Right",
                Html = ctx.Render<BOBModalContainer>(p => p
                    .Add(c => c.Modal, DrawerState(DrawerPosition.Right))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Drawer_Left",
                Html = ctx.Render<BOBModalContainer>(p => p
                    .Add(c => c.Modal, DrawerState(DrawerPosition.Left))).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
