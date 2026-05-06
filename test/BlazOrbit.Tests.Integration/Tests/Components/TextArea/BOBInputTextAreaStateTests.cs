using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.TextArea;

[Trait("Component State", "BOBInputTextArea")]
public class BOBInputTextAreaStateTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Value, "initial")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bob-input__field").GetAttribute("value").Should().Be("initial");

        cut.Render(p => p
            .Add(c => c.Value, "updated")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bob-input__field").GetAttribute("value").Should().Be("updated");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().BeNull();
        cut.Find("textarea").HasAttribute("disabled").Should().BeFalse();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("textarea").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, true));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-readonly").Should().Be("true");
        cut.Find("textarea").HasAttribute("readonly").Should().BeTrue();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, false));

        root.GetAttribute("data-bob-readonly").Should().BeNull();
        cut.Find("textarea").HasAttribute("readonly").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_Via_Explicit_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().BeNull();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
        cut.Find("textarea").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-loading").Should().BeNull();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, true));

        root.GetAttribute("data-bob-loading").Should().Be("true");
        cut.Find("textarea").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Flip_AutoResize_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.AutoResize, false));

        cut.Find("bob-component").GetAttribute("data-bob-autoresize").Should().BeNull();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.AutoResize, true));

        cut.Find("bob-component").GetAttribute("data-bob-autoresize").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Resize_Direction_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Resize, TextAreaResize.None));

        cut.Find("bob-component").GetAttribute("data-bob-resize").Should().Be("none");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Resize, TextAreaResize.Horizontal));
        cut.Find("bob-component").GetAttribute("data-bob-resize").Should().Be("horizontal");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Resize, TextAreaResize.Both));
        cut.Find("bob-component").GetAttribute("data-bob-resize").Should().Be("both");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Rows_And_MaxLength(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Rows, 3)
            .Add(c => c.MaxLength, 50));

        IElement textarea = cut.Find("textarea.bob-input__field");
        textarea.GetAttribute("rows").Should().Be("3");
        textarea.GetAttribute("maxlength").Should().Be("50");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Rows, 10)
            .Add(c => c.MaxLength, 500));

        textarea = cut.Find("textarea.bob-input__field");
        textarea.GetAttribute("rows").Should().Be("10");
        textarea.GetAttribute("maxlength").Should().Be("500");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        Dictionary<string, object> extra = new()
        {
            { "data-testid", "bio-field" },
            { "class", "my-class" },
            { "style", "margin: 4px;" }
        };

        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.AdditionalAttributes, extra));

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.AdditionalAttributes, extra)
            .Add(c => c.Disabled, true)
            .Add(c => c.Size, BOBSize.Small));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-testid").Should().Be("bio-field");
        root.ClassList.Should().Contain("my-class");
        root.GetAttribute("style").Should().Contain("margin: 4px;");
    }
}
