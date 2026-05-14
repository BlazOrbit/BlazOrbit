using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.AutoComplete;

[Trait("Component Accessibility", "BOBAutoComplete")]
public class BOBAutoCompleteAccessibilityTests
{
    private static Func<string, CancellationToken, Task<IEnumerable<string>>> StaticSource(params string[] items) =>
        (q, _) => Task.FromResult(items.Where(i => i.Contains(q, StringComparison.OrdinalIgnoreCase)));

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Aria_Expanded(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").GetAttribute("aria-expanded").Should().Be("false");

        cut.Find("input.bob-input__field").Input("S");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(1));

        cut.Find("input.bob-input__field").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Activedescendant_On_Focused_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain", "Sweden"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").Input("S");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(2));

        cut.Find("input.bob-input__field").KeyDown("ArrowDown");

        string? activeId = cut.Find("input.bob-input__field").GetAttribute("aria-activedescendant");
        string? focusedOptId = cut.Find(".bob-autocomplete__option[data-bob-focused='true']").GetAttribute("id");

        activeId.Should().NotBeNullOrEmpty();
        activeId.Should().Be(focusedOptId);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Options_With_Role_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.DebounceMs, 0));

        cut.Find("input.bob-input__field").Input("S");
        cut.WaitForAssertion(() => cut.FindAll(".bob-autocomplete__option").Should().HaveCount(1));

        cut.Find(".bob-autocomplete__option").GetAttribute("role").Should().Be("option");
        cut.Find(".bob-autocomplete__panel").GetAttribute("role").Should().Be("listbox");
    }
}