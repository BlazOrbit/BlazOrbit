using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component State", "BOBTimePicker")]
public class BOBTimePickerStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Size_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
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
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Density, BOBDensity.Compact));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-density").Should().Be("compact");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Minute_Value_From_Value_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(14, 35)));

        // Assert — minute input carries the two-digit minute
        IReadOnlyList<IElement> inputs = cut.FindAll("input");
        inputs.Should().Contain(i => i.GetAttribute("value") == "35");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Minute_Input_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(10, 15)));
        cut.FindAll("input").Should().Contain(i => i.GetAttribute("value") == "15");

        // Act
        cut.Render(p => p.Add(c => c.Value, new TimeOnly(10, 45)));

        // Assert
        cut.FindAll("input").Should().Contain(i => i.GetAttribute("value") == "45");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.OnlyServer), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_12_Hour_Format_Under_EnUs_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — VerifyConfig locks culture to en-US which uses a 12h ShortTimePattern
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>();

        // Assert — first button is the format toggle; en-US → "12h"
        cut.FindAll("button").First().TextContent.Trim().Should().Be("12h");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_AmPm_Toggle_In_12h_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .Add(c => c.Value, new TimeOnly(14, 0)));

        // Assert — last action button displays AM or PM label
        cut.Markup.Should().MatchRegex(">\\s*(AM|PM)\\s*<");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Class_And_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(p => p
            .AddUnmatched("class", "custom-time")
            .AddUnmatched("style", "padding: 2px;"));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("class").Should().Contain("custom-time");
        root.GetAttribute("style").Should().Contain("padding: 2px;");
    }
}
