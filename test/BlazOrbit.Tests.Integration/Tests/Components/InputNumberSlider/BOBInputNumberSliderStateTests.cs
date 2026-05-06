using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputNumberSlider;

[Trait("Component State", "BOBInputNumberSlider")]
public class BOBInputNumberSliderStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Value_Changes_On_Thumb_AriaValueNow(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, 25));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-valuenow").Should().Be("25");

        cut.Render(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, 75));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-valuenow").Should().Be("75");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>();

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().BeNull();

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("._bob-slider-thumb").GetAttribute("aria-disabled").Should().Be("true");
        cut.Find("._bob-slider-thumb").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>();

        cut.Find("._bob-slider-thumb").GetAttribute("aria-readonly").Should().Be("false");

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-readonly").Should().Be("true");
        cut.Find("bob-component").GetAttribute("data-bob-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>();

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().BeNull();

        cut.Render(p => p.Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Min_Max_On_Thumb_Aria(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, -50)
            .Add(c => c.Max, 50));

        cut.Find("._bob-slider-thumb").GetAttribute("aria-valuemin").Should().Be("-50");
        cut.Find("._bob-slider-thumb").GetAttribute("aria-valuemax").Should().Be("50");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Value_Above_Max(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Value, 250));

        // Aria reflects raw value; visual percent clamps inside ValueToPercent.
        // Raw value is preserved on aria-valuenow because we expose CurrentValue verbatim.
        // What we test here is that the thumb percent style is clamped to 100%.
        string? style = cut.Find("._bob-slider-thumb").GetAttribute("style");
        style.Should().Contain("--_thumb-percent: 100%");
    }
}
