using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Rating;

[Trait("Component Interaction", "BOBRating")]
public class BOBRatingInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Value_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        double captured = 0;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-rating__cell")[2].Click();

        captured.Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Off_On_Click_Same_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        double captured = -1;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Value, 3)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-rating__cell")[2].Click();

        captured.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, _ => fired = true));

        cut.FindAll(".bob-rating__cell")[2].Click();

        fired.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.ReadOnly, true)
            .Add(c => c.ValueChanged, _ => fired = true));

        cut.FindAll(".bob-rating__cell")[2].Click();

        fired.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Increment_On_Arrow_Right(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        double captured = -1;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Value, 2)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-rating__cell")[0].KeyDown("ArrowRight");

        captured.Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Decrement_On_Arrow_Left(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        double captured = -1;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Value, 3)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-rating__cell")[0].KeyDown("ArrowLeft");

        captured.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pick_Numeric_Key_When_In_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        double captured = -1;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-rating__cell")[0].KeyDown("4");

        captured.Should().Be(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reset_To_Zero_On_Zero_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        double captured = -1;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Value, 4)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-rating__cell")[0].KeyDown("0");

        captured.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Step_By_Half_When_AllowHalf(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        double captured = -1;
        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.AllowHalf, true)
            .Add(c => c.Value, 2)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-rating__cell")[0].KeyDown("ArrowRight");

        captured.Should().Be(2.5);
    }
}
