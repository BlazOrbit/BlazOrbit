using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Password;

[Trait("Component Rendering", "BOBInputPassword")]
public class BOBInputPasswordRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.Label, "Password"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-password");
        root.GetAttribute("data-bob-variant").Should().Be("outlined");
        root.GetAttribute("data-bob-size").Should().Be("medium");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Password_Type_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.Find("input.bob-input__field").GetAttribute("type").Should().Be("password");
        cut.Find("input.bob-input__field").GetAttribute("autocomplete").Should().Be("current-password");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Visibility_Toggle_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.FindAll(".bob-password__toggle").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Visibility_Toggle_When_Disabled_Via_Param(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowVisibilityToggle, false));

        cut.FindAll(".bob-password__toggle").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Strength_Meter_When_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowStrengthMeter, true));

        cut.FindAll(".bob-password__strength").Should().HaveCount(1);
        cut.Find(".bob-password__strength").GetAttribute("data-bob-strength").Should().Be("none");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Strength_Meter_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.FindAll(".bob-password__strength").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_HelperText_And_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.Label, "Password")
            .Add(c => c.HelperText, "Min 8 chars.")
            .Add(c => c.Required, true));

        cut.Find(".bob-input__label").TextContent.Should().Contain("Password");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Min 8 chars.");
        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }
}