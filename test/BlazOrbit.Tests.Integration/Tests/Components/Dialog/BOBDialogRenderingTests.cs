using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BOBDialog")]
public class BOBDialogRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, false));

        // Assert
        cut.FindAll("[role='dialog']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Content, b => b.AddContent(0, "Dialog body")));

        // Assert
        cut.Find(".bob-dialog__content").TextContent.Should().Contain("Dialog body");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Title_In_Header(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Title, "My Dialog"));

        // Assert
        cut.Find(".bob-dialog__title").TextContent.Should().Be("My Dialog");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Footer_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Footer, b => b.AddContent(0, "Cancel")));

        // Assert
        cut.Find(".bob-dialog__footer").TextContent.Should().Contain("Cancel");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Close_Button_When_Closable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDialog> cut = ctx.Render<BOBDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Title, "Title")
            .Add(c => c.Closable, true));

        // Assert
        cut.Find("[aria-label='Close']").Should().NotBeNull();
    }
}
