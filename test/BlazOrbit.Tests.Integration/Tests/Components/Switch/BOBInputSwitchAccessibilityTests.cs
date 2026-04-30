using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Switch;

[Trait("Component Accessibility", "BOBInputSwitch")]
public class BOBInputSwitchAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Switch_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>();

        cut.Find("input.bob-switch__input").GetAttribute("role").Should().Be("switch");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_Matching_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Value, false));

        cut.Find("input.bob-switch__input").GetAttribute("aria-checked").Should().Be("false");

        cut.Render(p => p.Add(c => c.Value, true));

        cut.Find("input.bob-switch__input").GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaLabel_Toggle_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>();

        cut.Find("input.bob-switch__input").GetAttribute("aria-label").Should().Be("Toggle");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_AriaLabel_When_Label_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Label, "Dark mode"));

        // Label acts as accessible name via `for` association — aria-label should be null
        cut.Find("input.bob-switch__input").GetAttribute("aria-label").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Associate_Label_With_Input_Via_For(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Label, "Feature"));

        IElement input = cut.Find("input.bob-switch__input");
        string? inputId = input.GetAttribute("id");
        inputId.Should().NotBeNullOrWhiteSpace();

        IElement label = cut.Find("label.bob-switch");
        label.GetAttribute("for").Should().Be(inputId);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.HelperText, "Enable notifications."));

        string? describedBy = cut.Find("input.bob-switch__input").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find(".bob-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Input_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Disabled, true));

        cut.Find("input.bob-switch__input").HasAttribute("disabled").Should().BeTrue();
    }
}
