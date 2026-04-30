using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Stubs;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component State", "BOBModalHost")]
public class BOBModalHostStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Modal_Count_Through_Renders(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        cut.FindAll(".bob-modal-container").Should().BeEmpty();

        // Act — open one modal
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        cut.FindAll(".bob-modal-container").Should().HaveCount(1);

        // Open second modal
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        cut.FindAll(".bob-modal-container").Should().HaveCount(2);

        // Close all
        await modalService.CloseAllAsync();

        // Assert
        cut.FindAll(".bob-modal-container").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Overlay_When_Active(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        // Act
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();

        // Assert
        cut.Find(".bob-modal-overlay").Should().NotBeNull();
    }
}
