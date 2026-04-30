using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Rendering", "BOBInputCheckbox")]
public class BOBInputCheckboxRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Accept"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-checkbox");
        root.GetAttribute("data-bob-variant").Should().Be("default");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-disabled").Should().Be("false");
        root.GetAttribute("data-bob-error").Should().Be("false");
        root.GetAttribute("data-bob-active").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Active_When_Checked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Value, true)
            .Add(c => c.Label, "Checked"));

        cut.Find("bob-component").GetAttribute("data-bob-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Indeterminate_For_Null_Nullable_Bool(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool?>> cut = ctx.Render<BOBInputCheckbox<bool?>>(p => p
            .Add(c => c.Label, "Indeterminate"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-indeterminate").Should().Be("true");
        root.GetAttribute("data-bob-active").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Terms")
            .Add(c => c.HelperText, "You must accept."));

        cut.Find(".bob-checkbox__label").TextContent.Should().Contain("Terms");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("You must accept.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>();

        cut.FindAll(".bob-checkbox__label").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Accept")
            .Add(c => c.Required, true));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-required").Should().Be("true");
        cut.Find(".bob-field__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_InlineColor_When_Color_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Colored")
            .Add(c => c.Color, "rgba(255,0,0,1)"));

        string style = cut.Find("bob-component").GetAttribute("style") ?? string.Empty;
        style.Should().Contain("--bob-inline-color: rgba(255,0,0,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Size_And_Density_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-size").Should().Be("large");
        root.GetAttribute("data-bob-density").Should().Be("compact");
    }
}
