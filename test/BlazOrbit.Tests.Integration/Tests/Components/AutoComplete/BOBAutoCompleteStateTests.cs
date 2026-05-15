using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.AutoComplete;

[Trait("Component State", "BOBAutoComplete")]
public class BOBAutoCompleteStateTests
{
    private static Func<string, CancellationToken, Task<IEnumerable<string>>> StaticSource(params string[] items) =>
        (q, _) => Task.FromResult(items.Where(i => i.Contains(q, StringComparison.OrdinalIgnoreCase)));

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s));

        cut.Render(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.Disabled, true));

        cut.Find("bob-component").GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s));

        cut.Render(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.ReadOnly, true));

        cut.Find("input.bob-input__field").HasAttribute("readonly").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s));

        cut.Render(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.Error, true));

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Search_Text_When_Value_Changes_Externally(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Foo", "Bar"))
            .Add(c => c.ItemText, s => s));

        cut.Find("input.bob-input__field").GetAttribute("value").Should().BeEmpty();

        cut.Render(p => p
            .Add(c => c.SearchAsync, StaticSource("Foo", "Bar"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.Value, "Foo"));

        cut.Find("input.bob-input__field").GetAttribute("value").Should().Be("Foo");
    }
}
