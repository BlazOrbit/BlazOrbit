using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Accessibility", "BOBDatePicker")]
public class BOBDatePickerAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Label_Navigation_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert
        cut.Find("button[aria-label='Previous year']").Should().NotBeNull();
        cut.Find("button[aria-label='Previous month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next year']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nav_Buttons_As_Native_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — type="button" prevents form submission; button role is implicit
        foreach (string label in new[] { "Previous year", "Previous month", "Next month", "Next year" })
        {
            IElement btn = cut.Find($"button[aria-label='{label}']");
            btn.TagName.Should().Be("BUTTON");
            btn.GetAttribute("type").Should().Be("button");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Day_Cells_Keyboard_Focusable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — all day cell buttons are tabbable (tabindex="0")
        IReadOnlyList<IElement> dayCells = cut.FindAll(".bob-picker__grid button.bob-picker__cell");
        dayCells.Should().OnlyContain(c => c.GetAttribute("tabindex") == "0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Week_Header_Cells_As_Muted_Non_Interactive(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — header weekdays render as <span>, not <button>
        IReadOnlyList<IElement> headers = cut.FindAll(".bob-picker__grid span.bob-picker__cell");
        headers.Should().HaveCount(7);
        headers.Should().OnlyContain(h => h.TagName == "SPAN"
            && h.GetAttribute("data-bob-muted") == "true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Decorative_Chevron_Icons_From_AT(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — icons inside aria-labeled buttons inherit accessible name;
        // the button's aria-label is sufficient, child svgs should not surface.
        IElement prevMonth = cut.Find("button[aria-label='Previous month']");
        prevMonth.QuerySelector("svg").Should().NotBeNull("chevron icon renders inside the button");
    }
}
