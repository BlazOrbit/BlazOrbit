using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputRangeSlider;

[Trait("Component Interaction", "BOBInputRangeSlider")]
public class BOBInputRangeSliderInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Increment_Min_Thumb_On_ArrowRight(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBNumericRange<int> value = new(20, 80);
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.FindAll("._bob-slider-thumb")[0].KeyDown(key: "ArrowRight");

        value.Min.Should().Be(21);
        value.Max.Should().Be(80);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Increment_Max_Thumb_On_ArrowRight(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBNumericRange<int> value = new(20, 80);
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.FindAll("._bob-slider-thumb")[1].KeyDown(key: "ArrowRight");

        value.Min.Should().Be(20);
        value.Max.Should().Be(81);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Min_To_Max_When_Crossing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBNumericRange<int> value = new(50, 50);
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.FindAll("._bob-slider-thumb")[0].KeyDown(key: "ArrowRight");

        value.Min.Should().Be(50);
        value.Max.Should().Be(50);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Max_To_Min_When_Crossing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBNumericRange<int> value = new(50, 50);
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.FindAll("._bob-slider-thumb")[1].KeyDown(key: "ArrowLeft");

        value.Min.Should().Be(50);
        value.Max.Should().Be(50);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Jump_Min_To_Track_Min_On_Home(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBNumericRange<int> value = new(40, 80);
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.FindAll("._bob-slider-thumb")[0].KeyDown(key: "Home");

        value.Min.Should().Be(0);
        value.Max.Should().Be(80);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Jump_Max_To_Track_Max_On_End(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBNumericRange<int> value = new(40, 80);
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        cut.FindAll("._bob-slider-thumb")[1].KeyDown(key: "End");

        value.Min.Should().Be(40);
        value.Max.Should().Be(100);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Change_Value_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBNumericRange<int> value = new(20, 80);
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Value, value)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, v => value = v));

        cut.FindAll("._bob-slider-thumb")[0].KeyDown(key: "ArrowRight");

        value.Min.Should().Be(20);
        value.Max.Should().Be(80);
    }
}
