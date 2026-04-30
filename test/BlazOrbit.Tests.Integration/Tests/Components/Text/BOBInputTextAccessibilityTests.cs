using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Text;

[Trait("Component Accessibility", "BOBInputText")]
public class BOBInputTextAccessibilityTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Associate_Label_With_Input_Via_For_Id(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Label, "Full name")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement input = cut.Find("input.bob-input__field");
        IElement label = cut.Find("label.bob-input__label");

        string? inputId = input.GetAttribute("id");
        inputId.Should().NotBeNullOrWhiteSpace();
        label.GetAttribute("for").Should().Be(inputId);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_AriaLabel_From_Placeholder_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Placeholder, "Search")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("aria-label").Should().Be("Search");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_AriaLabel_When_Label_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.Placeholder, "Type")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("aria-label").Should().BeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaRequired_Matching_Required_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Label, "Email")
            .Add(c => c.Required, true)
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("aria-required").Should().Be("true");

        cut.Render(p => p
            .Add(c => c.Label, "Email")
            .Add(c => c.Required, false)
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bob-input__field").GetAttribute("aria-required").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaInvalid_Matching_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, false));

        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, true));

        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper_Id(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.HelperText, "Your legal name")
            .Add(c => c.ValueExpression, () => model.Value));

        string? describedBy = cut.Find("input.bob-input__field").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find(".bob-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_Required_State_On_Host(BlazorScenario scenario)
    {
        // CSS-OPT-02 block B.5: the asterisk is rendered by CSS via
        // [data-bob-required="true"] .bob-input__label::after, so we assert
        // the host attribute rather than a DOM marker element.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Label, "Email")
            .Add(c => c.Required, true)
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }
}
