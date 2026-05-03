using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputNumberSlider;

[Trait("Component Accessibility", "BOBInputNumberSlider")]
public class BOBInputNumberSliderAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Slider_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>();

        cut.Find("._bob-slider-thumb").GetAttribute("role").Should().Be("slider");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_Aria_Min_Max_Now(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, 42));

        IElement thumb = cut.Find("._bob-slider-thumb");
        thumb.GetAttribute("aria-valuemin").Should().Be("0");
        thumb.GetAttribute("aria-valuemax").Should().Be("100");
        thumb.GetAttribute("aria-valuenow").Should().Be("42");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_AriaLabel_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.AriaLabel, "Master volume"));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-label").Should().Be("Master volume");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fall_Back_To_Label_For_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Label, "Brightness"));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-label").Should().Be("Brightness");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Vertical_Aria_Orientation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Orientation, BOBSliderOrientation.Vertical));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-orientation").Should().Be("vertical");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Be_Tabbable_When_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>();

        cut.Find("._bob-slider-thumb").GetAttribute("tabindex").Should().Be("0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Format_AriaValueText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<decimal>> cut = ctx.Render<BOBInputNumberSlider<decimal>>(p => p
            .Add(c => c.Value, 21.5m)
            .Add(c => c.ValueLabelFormat, "0.0 °C")
            .Add(c => c.Culture, System.Globalization.CultureInfo.InvariantCulture));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-valuetext").Should().Be("21.5 °C");
    }
}
