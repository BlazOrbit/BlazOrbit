using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TextArea;

[Trait("Component Interaction", "BOBInputTextArea")]
public class BOBInputTextAreaInteractionTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Value_On_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        string? captured = null;
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("textarea.bob-input__field").Change("hello world");

        captured.Should().Be("hello world");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Commit_Value_On_Input_When_UpdateOnInput_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        string? captured = null;
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v)
            .Add(c => c.UpdateOnInput, false));

        cut.Find("textarea.bob-input__field").Input("typing");

        captured.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Commit_Value_On_Input_When_UpdateOnInput_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        string? captured = null;
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v)
            .Add(c => c.UpdateOnInput, true));

        cut.Find("textarea.bob-input__field").Input("typing");

        captured.Should().Be("typing");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnInput_Callback_With_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        string? captured = null;
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.OnInput, v => captured = v));

        cut.Find("textarea.bob-input__field").Input("abc");

        captured.Should().Be("abc");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_On_Focus_And_Collapse_On_Blur_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-floated").Should().Be("false");

        cut.Find("textarea.bob-input__field").Focus();
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");

        cut.Find("textarea.bob-input__field").Blur();
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Label_Floated_On_Blur_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = "filled" };
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Value, "filled")
            .Add(c => c.Label, "Notes")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement textarea = cut.Find("textarea.bob-input__field");
        textarea.Focus();
        textarea.Blur();

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_Placeholder_While_Floated(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.Placeholder, "type here")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bob-input__field").GetAttribute("placeholder").Should().BeNull();

        cut.Find("textarea.bob-input__field").Focus();

        cut.Find("textarea.bob-input__field").GetAttribute("placeholder").Should().Be("type here");
    }
}
