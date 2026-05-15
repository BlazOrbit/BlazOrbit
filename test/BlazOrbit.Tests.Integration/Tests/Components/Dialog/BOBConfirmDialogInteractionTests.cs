using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Interaction", "BOBConfirmDialog")]
public class BOBConfirmDialogInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Invoke_CloseAsync_True_When_Yes_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool closed = false;
        object? closedResult = null;
        ModalReference reference = new("test-yes", async r =>
        {
            closed = true;
            closedResult = await r.Result;
        });

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, reference)
            .Add(c => c.Title, "Delete?")
            .Add(c => c.Message, "Sure?")
            .Add(c => c.YesLabel, "Yes")
            .Add(c => c.NoLabel, "No"));

        // Act - the Yes (accept) button is the second button in the actions row
        cut.FindAll(".bob-confirm__actions button").Last().Click();

        // Assert
        closed.Should().BeTrue();
        closedResult.Should().Be(true);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Invoke_CloseAsync_False_When_No_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool closed = false;
        object? closedResult = null;
        ModalReference reference = new("test-no", async r =>
        {
            closed = true;
            closedResult = await r.Result;
        });

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, reference)
            .Add(c => c.Title, "Delete?")
            .Add(c => c.Message, "Sure?")
            .Add(c => c.YesLabel, "Yes")
            .Add(c => c.NoLabel, "No"));

        // Act - the No (cancel) button is the first button in the actions row
        cut.FindAll(".bob-confirm__actions button").First().Click();

        // Assert
        closed.Should().BeTrue();
        closedResult.Should().Be(false);
    }
}
