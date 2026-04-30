using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Stubs;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BOBModalHost")]
public class BOBModalHostRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_No_Modals(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();

        // Assert
        cut.FindAll(".bob-modal-host").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Host_When_Modal_Shown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        // Act
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();

        // Assert
        cut.FindAll(".bob-modal-host").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Modal_Container_When_Modal_Shown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        // Act
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();

        // Assert
        cut.FindAll(".bob-modal-container").Should().HaveCount(1);
    }
}
