using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BOBConfirmDialog")]
public class BOBConfirmDialogRenderingTests
{
    private static ModalReference DummyReference => new("test-ref", _ => Task.CompletedTask);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "Delete file?")
            .Add(c => c.Message, "This will be permanent."));

        cut.Find(".bob-confirm__title").TextContent.Should().Be("Delete file?");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Message(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "This will be permanent."));

        cut.Find(".bob-confirm__message").TextContent.Should().Be("This will be permanent.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Yes_And_No_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M"));

        var buttons = cut.FindAll(".bob-confirm__actions button");
        buttons.Should().HaveCount(2);
        buttons[0].TextContent.Should().Be("Cancel");
        buttons[1].TextContent.Should().Be("Yes");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M")
            .Add(c => c.YesLabel, "Delete")
            .Add(c => c.NoLabel, "Keep"));

        var buttons = cut.FindAll(".bob-confirm__actions button");
        buttons[0].TextContent.Should().Be("Keep");
        buttons[1].TextContent.Should().Be("Delete");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Severity_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M"));

        cut.Find(".bob-confirm__icon bob-component").Should().NotBeNull();
    }
}
