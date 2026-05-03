using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputNumberSlider;

[Trait("Component Interaction", "BOBInputNumberSlider")]
public class BOBInputNumberSliderInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Increment_Value_On_ArrowUp(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 10;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "ArrowUp");

        value.Should().Be(11);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Decrement_Value_On_ArrowDown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 10;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "ArrowDown");

        value.Should().Be(9);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Increment_By_Custom_Step(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 0;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Step, 5)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "ArrowRight");

        value.Should().Be(5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Jump_To_Min_On_Home(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 50;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "Home");

        value.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Jump_To_Max_On_End(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 50;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "End");

        value.Should().Be(100);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Increment_By_TenSteps_On_PageUp(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 0;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Step, 1)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "PageUp");

        value.Should().Be(10);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Change_Value_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 10;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Value, value)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "ArrowUp");

        value.Should().Be(10);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Change_Value_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int value = 10;
        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Value, value)
            .Add(c => c.ReadOnly, true)
            .Add(c => c.ValueChanged, v => value = v));

        cut.Find("._bob-slider-thumb").KeyDown(key: "ArrowUp");

        value.Should().Be(10);
    }
}
