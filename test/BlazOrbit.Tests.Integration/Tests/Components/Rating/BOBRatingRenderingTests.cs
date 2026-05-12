using AngleSharp.Dom;
using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Rating;

[Trait("Component Rendering", "BOBRating")]
public class BOBRatingRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("rating");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Five_Slots_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>();

        cut.FindAll(".bob-rating__cell").Should().HaveCount(5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Honor_Custom_MaxValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.MaxValue, 7));

        cut.FindAll(".bob-rating__cell").Should().HaveCount(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_MaxValue_To_Sane_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> tooSmall = ctx.Render<BOBRating>(p => p
            .Add(c => c.MaxValue, 0));
        tooSmall.FindAll(".bob-rating__cell").Should().HaveCount(1);

        IRenderedComponent<BOBRating> tooBig = ctx.Render<BOBRating>(p => p
            .Add(c => c.MaxValue, 25));
        tooBig.FindAll(".bob-rating__cell").Should().HaveCount(10);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Resolve_Fill_State_Per_Cell(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.AllowHalf, true)
            .Add(c => c.Value, 2.5));

        IReadOnlyList<IElement> cells = cut.FindAll(".bob-rating__cell");
        cells[0].GetAttribute("data-bob-fill").Should().Be("full");
        cells[1].GetAttribute("data-bob-fill").Should().Be("full");
        cells[2].GetAttribute("data-bob-fill").Should().Be("half");
        cells[3].GetAttribute("data-bob-fill").Should().Be("empty");
        cells[4].GetAttribute("data-bob-fill").Should().Be("empty");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Label, "Quality")
            .Add(c => c.HelperText, "Pick one"));

        cut.Find(".bob-rating__label").TextContent.Should().Contain("Quality");
        cut.Find(".bob-rating__helper").TextContent.Should().Contain("Pick one");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_InlineColor_When_Color_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(p => p
            .Add(c => c.Color, "#ff0000"));

        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-color: #ff0000");
    }
}