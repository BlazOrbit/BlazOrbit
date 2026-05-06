using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Globalization;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component State", "BOBDatePicker")]
public class BOBDatePickerStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Size_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Density_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.Density, BOBDensity.Compact));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-density").Should().Be("compact");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sync_CurrentMonth_To_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(2020, 1, 10)));

        // Assert
        string title = cut.Find(".bob-picker__title").TextContent;
        title.Should().Contain(new DateTime(2020, 1, 1).ToString("MMMM yyyy", CultureInfo.CurrentCulture));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Title_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(2020, 1, 10)));

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateOnly(2022, 7, 4)));

        // Assert
        string title = cut.Find(".bob-picker__title").TextContent;
        title.Should().Contain(new DateTime(2022, 7, 1).ToString("MMMM yyyy", CultureInfo.CurrentCulture));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Shift_Active_Day_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(2024, 6, 15)));
        cut.FindAll(".bob-picker__cell[data-bob-active='true']")
            .Single().TextContent.Trim().Should().Be("15");

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateOnly(2024, 6, 22)));

        // Assert
        cut.FindAll(".bob-picker__cell[data-bob-active='true']")
            .Single().TextContent.Trim().Should().Be("22");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Class_And_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(p => p
            .AddUnmatched("class", "custom-picker")
            .AddUnmatched("style", "margin: 1rem;"));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("class").Should().Contain("custom-picker");
        root.GetAttribute("style").Should().Contain("margin: 1rem;");
    }
}
