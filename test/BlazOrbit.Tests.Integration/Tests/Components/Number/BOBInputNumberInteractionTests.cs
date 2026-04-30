using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Number;

[Trait("Component Interaction", "BOBInputNumber")]
public class BOBInputNumberInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Value_On_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>();

        // Act
        cut.Find("input.bob-input__field").Input("42");

        // Assert
        cut.Find(".current-value").TextContent.Should().Be("42");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Increment_On_ArrowUp(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 5));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowUp");

        // Assert
        cut.Find(".current-value").TextContent.Should().Be("6");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Decrement_On_ArrowDown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 5));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowDown");

        // Assert
        cut.Find(".current-value").TextContent.Should().Be("4");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_At_Max(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 10)
            .Add(c => c.Max, 10));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowUp");

        // Assert - stays at 10
        cut.Find(".current-value").TextContent.Should().Be("10");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_At_Min(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 0)
            .Add(c => c.Min, 0));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowDown");

        // Assert
        cut.Find(".current-value").TextContent.Should().Be("0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Custom_Step(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 10)
            .Add(c => c.Step, 5m));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowUp");

        // Assert
        cut.Find(".current-value").TextContent.Should().Be("15");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_On_Focus(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty"));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");

        // Act
        cut.Find("input.bob-input__field").Focus();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnIncrement_Callback(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 3));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowUp");

        // Assert
        cut.Find(".last-increment").TextContent.Should().Be("4");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnDecrement_Callback(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 3));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowDown");

        // Assert
        cut.Find(".last-decrement").TextContent.Should().Be("2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Increment_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 5)
            .Add(c => c.Disabled, true));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowUp");

        // Assert - unchanged
        cut.Find(".current-value").TextContent.Should().Be("5");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Increment_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputNumberConsumer> cut = ctx.Render<TestBOBInputNumberConsumer>(p => p
            .Add(c => c.Value, 5)
            .Add(c => c.ReadOnly, true));

        // Act
        cut.Find("input.bob-input__field").KeyDown(key: "ArrowUp");

        // Assert
        cut.Find(".current-value").TextContent.Should().Be("5");
    }
}
