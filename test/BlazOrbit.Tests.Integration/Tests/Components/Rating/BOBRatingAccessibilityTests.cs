using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Rating;

[Trait("Component Accessibility", "BOBRating")]
public class BOBRatingAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Radiogroup_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        cut.Find(".bob-rating").GetAttribute("role").Should().Be("radiogroup");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Each_Cell_As_Radio(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        cut.FindAll(".bob-rating__cell").Should().AllSatisfy(cell =>
            cell.GetAttribute("role").Should().Be("radio"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Checked_On_Active_Cell(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Value, 3));

        var cells = cut.FindAll(".bob-rating__cell");
        cells[0].GetAttribute("aria-checked").Should().Be("false");
        cells[2].GetAttribute("aria-checked").Should().Be("true");
        cells[3].GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Label_As_Aria_Label_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Label, "How was it?"));

        cut.Find(".bob-rating").GetAttribute("aria-label").Should().Be("How was it?");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Make_First_Cell_Tabbable_Only(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        var cells = cut.FindAll(".bob-rating__cell");
        cells[0].GetAttribute("tabindex").Should().Be("0");
        cells[1].GetAttribute("tabindex").Should().Be("-1");
    }
}
