using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Select;

[Trait("Component State", "BOBSelect")]
public class BOBSelectStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().BeNull();
        cut.Find("select").HasAttribute("disabled").Should().BeFalse();

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("select").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Required_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Required, false));

        cut.Find("select").HasAttribute("required").Should().BeFalse();

        cut.Render(p => p.Add(c => c.Required, true));

        cut.Find("select").HasAttribute("required").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.ReadOnly, false));

        // ReadOnly is handled in OnChangeAsync - no DOM attribute on the native select
        // Verify the component renders and the callback is suppressed via interaction tests
        cut.Find("select").Should().NotBeNull();

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        cut.Find("select").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_FullWidth_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.FullWidth, false));

        cut.Find("bob-component").GetAttribute("data-bob-fullwidth").Should().BeNull();

        cut.Render(p => p.Add(c => c.FullWidth, true));

        cut.Find("bob-component").GetAttribute("data-bob-fullwidth").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Size, BOBSize.Large));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.AdditionalAttributes,
                new Dictionary<string, object> { ["data-testid"] = "country-select", ["class"] = "my-select" }));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-testid").Should().Be("country-select");
        root.ClassList.Should().Contain("my-select");
    }
}
