using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Globalization;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Rendering", "BOBDatePicker")]
public class BOBDatePickerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_DatePicker_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("date-picker");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Picker_Family_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-picker-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Navigation_Buttons(BlazorScenario scenario)
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
    public async Task Should_Render_42_Day_Cell_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — 42 day buttons plus 7 week header spans = 49 .bob-picker__cell elements
        IReadOnlyList<IElement> dayCells = cut.FindAll(".bob-picker__grid button.bob-picker__cell");
        dayCells.Should().HaveCount(42);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Week_Day_Headers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — 7 abbreviated weekday header spans
        IReadOnlyList<IElement> headers = cut.FindAll(".bob-picker__grid span.bob-picker__cell");
        headers.Should().HaveCount(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Month_Year_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Assert — title shows current month/year
        string expected = DateTime.Today.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        cut.Find(".bob-picker__title").TextContent.Should().Contain(expected);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Selected_Day_As_Active(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        DateOnly selectedDate = new(2024, 6, 15);
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.Value, selectedDate));

        // Assert — selected day has bob-btn--active class
        IReadOnlyList<IElement> activeCells = cut.FindAll(".bob-picker__cell.bob-btn--active");
        activeCells.Should().HaveCount(1);
        activeCells[0].TextContent.Trim().Should().Be("15");
    }
}
