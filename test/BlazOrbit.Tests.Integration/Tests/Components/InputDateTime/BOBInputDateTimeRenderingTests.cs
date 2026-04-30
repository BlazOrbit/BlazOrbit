using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Rendering", "BOBInputDateTime")]
public class BOBInputDateTimeRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_InputDateTime_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-date-time");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Input_Family_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-input-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Open_Picker_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Assert
        cut.Find("button[aria-label='Open picker']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Select Date"));

        // Assert
        cut.Markup.Should().Contain("Select Date");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Helper_Text_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.HelperText, "Pick a date"));

        // Assert
        cut.Markup.Should().Contain("Pick a date");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Float_Label_Initially_Without_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Date"));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_When_Value_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Date")
            .Add(c => c.Value, new DateOnly(2024, 6, 15)));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Pattern_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Assert
        cut.Find(".bob-pattern").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Outlined_Variant_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("outlined");
    }
}
