using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputNumberSlider;

[Trait("Component Rendering", "BOBInputNumberSlider")]
public class BOBInputNumberSliderRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Label, "Volume"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-number-slider");
        root.GetAttribute("data-bob-input-base").Should().NotBeNull();
        root.GetAttribute("data-bob-orientation").Should().Be("horizontal");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-disabled").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Track_And_Thumb(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>();

        cut.Find("._bob-slider-track").Should().NotBeNull();
        cut.Find("._bob-slider-thumb").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Label, "Brightness")
            .Add(c => c.HelperText, "Adjust brightness."));

        cut.Find(".bob-input-number-slider__label").TextContent.Should().Contain("Brightness");
        cut.Find(".bob-input-number-slider__helper-text").TextContent.Should().Contain("Adjust brightness.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Vertical_Orientation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Orientation, BOBSliderOrientation.Vertical));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("vertical");
        cut.Find("._bob-slider-track").GetAttribute("data-orientation").Should().Be("vertical");
        cut.Find("._bob-slider-thumb").GetAttribute("data-orientation").Should().Be("vertical");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Ticks_When_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 10)
            .Add(c => c.Step, 1)
            .Add(c => c.ShowTicks, true));

        cut.FindAll("._bob-slider-track__tick").Count.Should().BeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Ticks_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>();

        cut.FindAll("._bob-slider-track__tick").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Indicator(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Label, "Quantity")
            .Add(c => c.Required, true));

        cut.Find(".bob-input-number-slider__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Design_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.Color, "rgba(10,20,30,1)"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-size").Should().Be("large");
        root.GetAttribute("data-bob-density").Should().Be("compact");
        root.GetAttribute("style").Should().Contain("--bob-inline-color: rgba(10,20,30,1)");
    }
}
