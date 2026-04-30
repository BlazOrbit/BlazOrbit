using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Stubs;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Interaction", "BOBModalHost")]
public class BOBModalHostInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Host_After_Close(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        cut.FindAll(".bob-modal-host").Should().HaveCount(1);

        // Act
        await modalService.CloseAllAsync();

        // Assert
        cut.FindAll(".bob-modal-host").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Stack_Multiple_Modals(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        // Act
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();

        // Assert
        cut.FindAll(".bob-modal-container").Should().HaveCount(2);
    }
}
