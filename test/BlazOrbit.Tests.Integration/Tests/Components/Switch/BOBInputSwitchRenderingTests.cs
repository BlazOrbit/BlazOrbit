using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Switch;

[Trait("Component Rendering", "BOBInputSwitch")]
public class BOBInputSwitchRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Label, "Toggle"));

        // Outer bob-component from BOBInputSwitch
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-switch");
        root.GetAttribute("data-bob-variant").Should().Be("default");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-density").Should().Be("standard");
        root.GetAttribute("data-bob-disabled").Should().BeNull();
        root.GetAttribute("data-bob-error").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Inner_Switch_With_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Label, "Feature"));

        IElement input = cut.Find("input.bob-switch__input");
        input.GetAttribute("role").Should().Be("switch");
        input.GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Track_And_Thumb(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>();

        cut.Find(".bob-switch__track").Should().NotBeNull();
        cut.Find(".bob-switch__thumb").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Label, "Dark mode"));

        cut.Find(".bob-switch__label").TextContent.Should().Contain("Dark mode");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>();

        cut.FindAll(".bob-switch__label").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.HelperText, "Enable to activate."));

        cut.Find(".bob-field-helper").TextContent.Should().Contain("Enable to activate.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Size_And_Density(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-size").Should().Be("large");
        root.GetAttribute("data-bob-density").Should().Be("compact");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Checked_When_Value_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Value, true));

        cut.Find("input.bob-switch__input").GetAttribute("aria-checked").Should().Be("true");
    }
}
