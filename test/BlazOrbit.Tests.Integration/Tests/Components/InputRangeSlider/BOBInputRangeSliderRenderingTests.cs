using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputRangeSlider;

[Trait("Component Rendering", "BOBInputRangeSlider")]
public class BOBInputRangeSliderRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Label, "Range"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-range-slider");
        root.GetAttribute("data-bob-input-base").Should().NotBeNull();
        root.GetAttribute("data-bob-orientation").Should().Be("horizontal");
        root.GetAttribute("data-bob-size").Should().Be("medium");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Track_And_Two_Thumbs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>();

        cut.Find("._bob-slider-track").Should().NotBeNull();
        cut.FindAll("._bob-slider-thumb").Count.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Label, "Price")
            .Add(c => c.HelperText, "Set min and max."));

        cut.Find(".bob-input-range-slider__label").TextContent.Should().Contain("Price");
        cut.Find(".bob-input-range-slider__helper-text").TextContent.Should().Contain("Set min and max.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Vertical_Orientation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Orientation, BOBSliderOrientation.Vertical));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("vertical");
        cut.Find("._bob-slider-track").GetAttribute("data-orientation").Should().Be("vertical");
        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t => t.GetAttribute("data-orientation").Should().Be("vertical"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Ticks_When_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 10)
            .Add(c => c.Step, 1)
            .Add(c => c.ShowTicks, true));

        cut.FindAll("._bob-slider-track__tick").Count.Should().BeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Indicator(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Label, "Age range")
            .Add(c => c.Required, true));

        cut.Find(".bob-input-range-slider__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Design_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.Color, "rgba(10,20,30,1)"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-size").Should().Be("large");
        root.GetAttribute("data-bob-density").Should().Be("compact");
        root.GetAttribute("style").Should().Contain("--bob-inline-color: rgba(10,20,30,1)");
    }
}
