using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component State", "BOBInputDateTime")]
public class BOBInputDateTimeStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Act
        cut.Render(p => p.Add(c => c.Disabled, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Readonly_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Act
        cut.Render(p => p.Add(c => c.ReadOnly, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Loading_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();

        // Act
        cut.Render(p => p.Add(c => c.Loading, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_When_DateOnly_Value_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>();
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateOnly(2024, 1, 1)));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_When_TimeOnly_Value_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>();
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.Value, new TimeOnly(12, 0)));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_When_DateTime_Value_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>();
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateTime(2024, 6, 15, 14, 30, 0)));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Picker_Button_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("button[aria-label='Open picker']").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .AddUnmatched("data-testid", "dt-picker"));

        // Assert
        cut.Find("bob-component").GetAttribute("data-testid").Should().Be("dt-picker");
    }
}
