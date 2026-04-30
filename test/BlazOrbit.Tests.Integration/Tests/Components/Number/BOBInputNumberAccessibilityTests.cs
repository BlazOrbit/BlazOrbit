using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Number;

[Trait("Component Accessibility", "BOBInputNumber")]
public class BOBInputNumberAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Associate_Label_With_Input_Via_For(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty"));

        IElement input = cut.Find("input.bob-input__field");
        IElement label = cut.Find("label.bob-input__label");
        string? inputId = input.GetAttribute("id");
        inputId.Should().NotBeNullOrWhiteSpace();
        label.GetAttribute("for").Should().Be(inputId);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaLabel_From_Placeholder_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Placeholder, "Enter qty"));

        cut.Find("input.bob-input__field").GetAttribute("aria-label").Should().Be("Enter qty");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaInvalid_Matching_Error(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Error, true));

        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaRequired(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Required, true));

        cut.Find("input.bob-input__field").GetAttribute("aria-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.HelperText, "Enter a number."));

        string? describedBy = cut.Find("input.bob-input__field").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find(".bob-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_InputMode_Decimal(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<decimal>> cut = ctx.Render<BOBInputNumber<decimal>>();

        cut.Find("input.bob-input__field").GetAttribute("inputmode").Should().Be("decimal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Increment_Decrement_Aria_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.ShowStepButtons, true));

        cut.Markup.Should().Contain("Increment");
        cut.Markup.Should().Contain("Decrement");
    }
}
