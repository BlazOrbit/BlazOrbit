using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.AutoComplete;

[Trait("Component Interaction", "BOBAutoComplete")]
public class BOBAutoCompleteInteractionTests
{
    private static Func<string, CancellationToken, Task<IEnumerable<string>>> StaticSource(params string[] items) =>
        (q, _) => Task.FromResult(items.Where(i => i.Contains(q, StringComparison.OrdinalIgnoreCase)));

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Panel_And_Render_Results_On_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain", "Sweden", "Switzerland"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").Input("S");

        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(3));
        cut.Find("input.bob-input__field").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_NoResults_When_Source_Returns_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, (q, _) => Task.FromResult(Enumerable.Empty<string>()))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").Input("Zzz");

        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__no-results").Should().HaveCount(1));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Search_Below_MinSearchLength(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int callCount = 0;
        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, (q, _) =>
            {
                callCount++;
                return Task.FromResult(Enumerable.Empty<string>());
            })
            .Add(c => c.ItemText, s => s)
            .Add(c => c.MinSearchLength, 3)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").Input("ab");

        callCount.Should().Be(0);

        cut.Find("input.bob-input__field").Input("abc");

        callCount.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pick_Item_On_Mousedown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? selected = null;
        string? captured = null;
        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Argentina", "Brazil"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0)
            .Add(c => c.OnSelected, v => selected = v)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("input.bob-input__field").Input("Br");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(1));

        cut.Find(".bob-autocomplete__option").MouseDown();

        selected.Should().Be("Brazil");
        captured.Should().Be("Brazil");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_With_Arrow_Keys(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain", "Sweden", "Switzerland"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").Input("S");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(3));

        cut.FindAll(".bob-autocomplete__option")[0].GetAttribute("data-bob-focused").Should().Be("true");

        cut.Find("input.bob-input__field").KeyDown("ArrowDown");
        cut.FindAll(".bob-autocomplete__option")[1].GetAttribute("data-bob-focused").Should().Be("true");

        cut.Find("input.bob-input__field").KeyDown("End");
        cut.FindAll(".bob-autocomplete__option")[2].GetAttribute("data-bob-focused").Should().Be("true");

        cut.Find("input.bob-input__field").KeyDown("Home");
        cut.FindAll(".bob-autocomplete__option")[0].GetAttribute("data-bob-focused").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pick_Focused_Item_On_Enter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain", "Sweden"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("input.bob-input__field").Input("S");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(2));

        cut.Find("input.bob-input__field").KeyDown("ArrowDown");
        cut.Find("input.bob-input__field").KeyDown("Enter");

        captured.Should().Be("Sweden");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Panel_On_Escape(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").Input("S");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(1));

        cut.Find("input.bob-input__field").KeyDown("Escape");

        cut.FindAll(".bob-autocomplete__panel").Should().BeEmpty();
    }
}
