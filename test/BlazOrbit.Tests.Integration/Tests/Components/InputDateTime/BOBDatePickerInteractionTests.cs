using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Globalization;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Interaction", "BOBDatePicker")]
public class BOBDatePickerInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Previous_Month(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();
        string initialTitle = cut.Find(".bob-picker__title").TextContent;

        // Act
        cut.Find("button[aria-label='Previous month']").Click();

        // Assert
        string updatedTitle = cut.Find(".bob-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddMonths(-1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
        updatedTitle.Should().NotBe(initialTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Next_Month(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();
        string initialTitle = cut.Find(".bob-picker__title").TextContent;

        // Act
        cut.Find("button[aria-label='Next month']").Click();

        // Assert
        string updatedTitle = cut.Find(".bob-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddMonths(1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
        updatedTitle.Should().NotBe(initialTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Previous_Year(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Act
        cut.Find("button[aria-label='Previous year']").Click();

        // Assert
        string updatedTitle = cut.Find(".bob-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddYears(-1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Next_Year(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>();

        // Act
        cut.Find("button[aria-label='Next year']").Click();

        // Assert
        string updatedTitle = cut.Find(".bob-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddYears(1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Day_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateOnly? captured = null;
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.ValueChanged, v => captured = v));

        // Act — click first non-muted day cell
        IReadOnlyList<IElement> dayCells = cut.FindAll(".bob-picker__grid button.bob-picker__cell");
        dayCells.First().Click();

        // Assert
        captured.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Selected_Day_After_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateOnly? selected = null;
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1))
            .Add(c => c.ValueChanged, v => selected = v));

        // Act — click a day that contains "15"
        IElement day15 = cut.FindAll(".bob-picker__grid button.bob-picker__cell")
            .First(b => b.TextContent.Trim() == "15");
        day15.Click();

        // Assert
        selected.Should().NotBeNull();
        selected!.Value.Day.Should().Be(15);
    }
}
