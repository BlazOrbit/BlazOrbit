using AngleSharp.Dom;
using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Rating;

[Trait("Component State", "BOBRating")]
public class BOBRatingStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        cut.Render(p => p.Add(c => c.Disabled, true));

        cut.Find("bob-component").GetAttribute("data-bob-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        cut.Find("bob-component").GetAttribute("data-bob-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Fill_When_Value_Changes_Externally(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        cut.FindAll(".bob-rating__cell")[0].GetAttribute("data-bob-fill").Should().Be("empty");

        cut.Render(p => p.Add(c => c.Value, 4));

        IReadOnlyList<IElement> cells = cut.FindAll(".bob-rating__cell");
        cells[0].GetAttribute("data-bob-fill").Should().Be("full");
        cells[3].GetAttribute("data-bob-fill").Should().Be("full");
        cells[4].GetAttribute("data-bob-fill").Should().Be("empty");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Value_Above_MaxValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.MaxValue, 5)
            .Add(c => c.Value, 99));

        cut.FindAll(".bob-rating__cell").Should().AllSatisfy(c =>
            c.GetAttribute("data-bob-fill").Should().Be("full"));
    }
}