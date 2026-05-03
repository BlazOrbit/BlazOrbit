using System.Collections.Generic;
using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputRangeSlider;

[Trait("Component Accessibility", "BOBInputRangeSlider")]
public class BOBInputRangeSliderAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Slider_Role_For_Both_Thumbs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>();

        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t => t.GetAttribute("role").Should().Be("slider"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Default_AriaLabels_Min_Max(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>();

        IReadOnlyList<IElement> thumbs = cut.FindAll("._bob-slider-thumb");
        thumbs[0].GetAttribute("aria-label").Should().Be("Minimum");
        thumbs[1].GetAttribute("aria-label").Should().Be("Maximum");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Custom_AriaLabels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.AriaLabelMin, "From")
            .Add(c => c.AriaLabelMax, "To"));

        IReadOnlyList<IElement> thumbs = cut.FindAll("._bob-slider-thumb");
        thumbs[0].GetAttribute("aria-label").Should().Be("From");
        thumbs[1].GetAttribute("aria-label").Should().Be("To");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_Same_Min_Max_On_Both_Thumbs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, -10)
            .Add(c => c.Max, 90));

        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t =>
        {
            t.GetAttribute("aria-valuemin").Should().Be("-10");
            t.GetAttribute("aria-valuemax").Should().Be("90");
        });
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Vertical_Aria_Orientation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Orientation, BOBSliderOrientation.Vertical));

        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t => t.GetAttribute("aria-orientation").Should().Be("vertical"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Be_Tabbable_When_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>();

        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t => t.GetAttribute("tabindex").Should().Be("0"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Format_AriaValueText_Per_Thumb(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<decimal>> cut = ctx.Render<BOBInputRangeSlider<decimal>>(p => p
            .Add(c => c.Min, 0m)
            .Add(c => c.Max, 500m)
            .Add(c => c.Value, new BOBNumericRange<decimal>(50m, 250m))
            .Add(c => c.ValueLabelFormat, "0.00")
            .Add(c => c.Culture, System.Globalization.CultureInfo.InvariantCulture));

        IReadOnlyList<IElement> thumbs = cut.FindAll("._bob-slider-thumb");
        thumbs[0].GetAttribute("aria-valuetext").Should().Be("50.00");
        thumbs[1].GetAttribute("aria-valuetext").Should().Be("250.00");
    }
}
