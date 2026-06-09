using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DateRange;

[Trait("Component Accessibility", "BOBInputDateRange")]
public class BOBInputDateRangeAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Separator_Glyph(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Separator, "-")
            .Add(c => c.Value, new BlazOrbit.Components.DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31))));

        cut.Find(".bob-daterange__separator").TextContent.Should().Be("-");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Resolve_Placeholders_From_Parameters(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.StartPlaceholder, "Desde")
            .Add(c => c.EndPlaceholder, "Hasta")
            .Add(c => c.Value, default));

        // Resolved placeholders propagate down to the composed inputs via Label.
        // The inner BOBInputDateTime uses Label for its outline/placeholder text.
        string markup = cut.Markup;
        markup.Should().Contain("Desde");
        markup.Should().Contain("Hasta");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Two_Composed_Inputs_With_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Value, new BlazOrbit.Components.DateRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31))));

        cut.FindAll("[data-bob-component='input-date-time']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Required_To_Composed_Inputs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Label, "Range")
            .Add(c => c.Required, true)
            .Add(c => c.Value, default));

        // Required marker shown on the outer label.
        cut.Find(".bob-field__required").TextContent.Should().Be("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Disabled_To_Composed_Inputs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.Value, default));

        // Disabled state surfaces as data-bob-disabled on the inner inputs.
        IReadOnlyList<AngleSharp.Dom.IElement> inputs = cut.FindAll("[data-bob-component='input-date-time']");
        inputs.Should().AllSatisfy(el =>
            el.HasAttribute("data-bob-disabled").Should().BeTrue());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Prefix_And_Suffix_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.PrefixText, "from")
            .Add(c => c.SuffixText, "to")
            .Add(c => c.Value, default));

        string markup = cut.Markup;
        markup.Should().Contain("from");
        markup.Should().Contain("to");
    }
}
