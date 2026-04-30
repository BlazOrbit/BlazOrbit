using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component State", "BOBInputCheckbox")]
public class BOBInputCheckboxStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Active_On_Value_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-active").Should().Be("false");

        cut.Render(p => p.Add(c => c.Value, true));

        root.GetAttribute("data-bob-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Cycle_Indeterminate_State_For_Nullable_Bool(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool?>> cut = ctx.Render<BOBInputCheckbox<bool?>>(p => p
            .Add(c => c.Value, (bool?)null));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-indeterminate").Should().Be("true");
        root.GetAttribute("data-bob-active").Should().Be("false");

        cut.Render(p => p.Add(c => c.Value, (bool?)true));

        root.GetAttribute("data-bob-active").Should().Be("true");
        root.GetAttribute("data-bob-indeterminate").Should().BeNull();

        cut.Render(p => p.Add(c => c.Value, (bool?)false));

        root.GetAttribute("data-bob-active").Should().Be("false");
        root.GetAttribute("data-bob-indeterminate").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().Be("false");

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.ReadOnly, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-readonly").Should().Be("false");

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        root.GetAttribute("data-bob-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Error, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().Be("false");

        cut.Render(p => p.Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Required_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Terms")
            .Add(c => c.Required, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-required").Should().Be("false");
        cut.FindAll(".bob-field__required").Should().BeEmpty();

        cut.Render(p => p
            .Add(c => c.Label, "Terms")
            .Add(c => c.Required, true));

        root.GetAttribute("data-bob-required").Should().Be("true");
        cut.Find(".bob-field__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Old")
            .Add(c => c.HelperText, "Old help"));

        cut.Find(".bob-checkbox__label").TextContent.Should().Contain("Old");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Old help");

        cut.Render(p => p
            .Add(c => c.Label, "New")
            .Add(c => c.HelperText, "New help"));

        cut.Find(".bob-checkbox__label").TextContent.Should().Contain("New");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("New help");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Dictionary<string, object> extra = new()
        {
            { "data-testid", "accept-cb" },
            { "class", "my-class" },
            { "style", "margin: 8px;" }
        };

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.AdditionalAttributes, extra));

        cut.Render(p => p
            .Add(c => c.AdditionalAttributes, extra)
            .Add(c => c.Disabled, true)
            .Add(c => c.Size, BOBSize.Small));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-testid").Should().Be("accept-cb");
        root.ClassList.Should().Contain("my-class");
        root.GetAttribute("style").Should().Contain("margin: 8px;");
    }
}
