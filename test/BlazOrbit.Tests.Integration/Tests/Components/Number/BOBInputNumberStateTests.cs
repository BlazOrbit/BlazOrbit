using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Number;

[Trait("Component State", "BOBInputNumber")]
public class BOBInputNumberStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Value, 10));

        cut.Find("input.bob-input__field").GetAttribute("value").Should().Be("10");

        cut.Render(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Value, 99));

        cut.Find("input.bob-input__field").GetAttribute("value").Should().Be("99");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().Be("false");

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Loading, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-loading").Should().Be("false");

        cut.Render(p => p.Add(c => c.Loading, true));

        root.GetAttribute("data-bob-loading").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Error, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().Be("false");

        cut.Render(p => p.Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.ReadOnly, false));

        cut.Find("bob-component").GetAttribute("data-bob-readonly").Should().Be("false");

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        cut.Find("bob-component").GetAttribute("data-bob-readonly").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("readonly").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Dictionary<string, object> extra = new()
        {
            { "data-testid", "qty-input" },
            { "class", "my-num" }
        };

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.AdditionalAttributes, extra));

        cut.Render(p => p
            .Add(c => c.AdditionalAttributes, extra)
            .Add(c => c.Disabled, true));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-testid").Should().Be("qty-input");
        root.ClassList.Should().Contain("my-num");
    }
}
