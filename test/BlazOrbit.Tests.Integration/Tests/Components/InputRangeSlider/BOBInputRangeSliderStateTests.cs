using System.Collections.Generic;
using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputRangeSlider;

[Trait("Component State", "BOBInputRangeSlider")]
public class BOBInputRangeSliderStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Range_On_Both_Thumbs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, new BOBNumericRange<int>(20, 80)));

        IReadOnlyList<IElement> thumbs = cut.FindAll("._bob-slider-thumb");
        thumbs[0].GetAttribute("aria-valuenow").Should().Be("20");
        thumbs[1].GetAttribute("aria-valuenow").Should().Be("80");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Both_Thumbs_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, new BOBNumericRange<int>(10, 30)));

        cut.Render(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, new BOBNumericRange<int>(40, 60)));

        IReadOnlyList<IElement> thumbs = cut.FindAll("._bob-slider-thumb");
        thumbs[0].GetAttribute("aria-valuenow").Should().Be("40");
        thumbs[1].GetAttribute("aria-valuenow").Should().Be("60");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>();

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().Be("false");

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t => t.GetAttribute("aria-disabled").Should().Be("true"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>();

        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t => t.GetAttribute("aria-readonly").Should().Be("false"));

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        cut.FindAll("._bob-slider-thumb").Should().AllSatisfy(t => t.GetAttribute("aria-readonly").Should().Be("true"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Normalize_Inverted_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Inverted input: Min=70, Max=30. NormalizeRange should swap so the lower
        // thumb shows 30 and upper shows 70.
        IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, new BOBNumericRange<int>(70, 30)));

        IReadOnlyList<IElement> thumbs = cut.FindAll("._bob-slider-thumb");
        thumbs[0].GetAttribute("aria-valuenow").Should().Be("30");
        thumbs[1].GetAttribute("aria-valuenow").Should().Be("70");
    }
}
