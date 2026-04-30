using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Accessibility", "BOBInputCheckbox")]
public class BOBInputCheckboxAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Checkbox_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Accept"));

        cut.Find(".bob-checkbox").GetAttribute("role").Should().Be("checkbox");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_False_When_Unchecked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false));

        cut.Find(".bob-checkbox").GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_True_When_Checked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Value, true));

        cut.Find(".bob-checkbox").GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_Mixed_When_Indeterminate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool?>> cut = ctx.Render<BOBInputCheckbox<bool?>>(p => p
            .Add(c => c.Value, (bool?)null));

        cut.Find(".bob-checkbox").GetAttribute("aria-checked").Should().Be("mixed");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaDisabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".bob-checkbox").GetAttribute("aria-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaLabelledBy_To_Label_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Terms"));

        IElement checkbox = cut.Find(".bob-checkbox");
        string? labelledBy = checkbox.GetAttribute("aria-labelledby");
        labelledBy.Should().NotBeNullOrWhiteSpace();

        IElement label = cut.Find(".bob-checkbox__label");
        label.GetAttribute("id").Should().Be(labelledBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_AriaLabelledBy_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>();

        cut.Find(".bob-checkbox").GetAttribute("aria-labelledby").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_TabIndex_0_When_Enabled_And_Minus1_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Disabled, false));

        cut.Find(".bob-checkbox").GetAttribute("tabindex").Should().Be("0");

        cut.Render(p => p.Add(c => c.Disabled, true));

        cut.Find(".bob-checkbox").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.HelperText, "Select to continue"));

        string? describedBy = cut.Find(".bob-checkbox").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find(".bob-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }
}
