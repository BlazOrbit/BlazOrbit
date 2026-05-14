using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DateRange;

[Trait("Component Rendering", "BOBInputDateRange")]
public class BOBInputDateRangeRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Two_Composed_DateTime_Inputs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // The redesign composes two BOBInputDateTime instances under a single
        // <bob-component data-bob-component="input-date-range"> root, with the
        // separator glyph between them.
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Value,
                new BlazOrbit.Components.DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31))));

        cut.Find("bob-component[data-bob-component='input-date-range']").Should().NotBeNull();
        cut.FindAll("[data-bob-component='input-date-time']").Should().HaveCount(2);
        cut.Find(".bob-daterange__separator").TextContent.Should().Be("→");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_With_Required_Marker_When_Bound_To_Required_Field(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Label, "Range")
            .Add(c => c.Required, true)
            .Add(c => c.Value, default));

        cut.Find("label.bob-daterange__label").TextContent.Should().Contain("Range");
        cut.Find(".bob-field__required").TextContent.Should().Be("*");
    }
}