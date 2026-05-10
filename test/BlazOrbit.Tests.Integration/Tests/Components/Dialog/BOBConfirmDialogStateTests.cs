using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component State", "BOBConfirmDialog")]
public class BOBConfirmDialogStateTests
{
    private static ModalReference DummyReference => new("test-ref", _ => Task.CompletedTask);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Info_Severity_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-severity").Should().Be("info");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Warning_Severity(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M")
            .Add(c => c.Severity, ConfirmSeverity.Warning));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-severity").Should().Be("warning");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Danger_Severity(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M")
            .Add(c => c.Severity, ConfirmSeverity.Danger));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-severity").Should().Be("danger");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Accept_Button_With_Error_Color_When_Danger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M")
            .Add(c => c.Severity, ConfirmSeverity.Danger));

        // The accept (Yes) button is the second button and should have an inline style with the error palette
        var acceptButton = cut.FindAll(".bob-confirm__actions bob-component").Last();
        string? style = acceptButton.GetAttribute("style");
        style.Should().Contain("var(--palette-error)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Accept_Button_With_Warning_Color_When_Warning(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M")
            .Add(c => c.Severity, ConfirmSeverity.Warning));

        var acceptButton = cut.FindAll(".bob-confirm__actions bob-component").Last();
        string? style = acceptButton.GetAttribute("style");
        style.Should().Contain("var(--palette-warning)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Accept_Button_Without_Explicit_Color_When_Info(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M")
            .Add(c => c.Severity, ConfirmSeverity.Info));

        var acceptButton = cut.FindAll(".bob-confirm__actions bob-component").Last();
        string? style = acceptButton.GetAttribute("style");
        style.Should().NotContain("var(--palette-error)");
        style.Should().NotContain("var(--palette-warning)");
    }
}
