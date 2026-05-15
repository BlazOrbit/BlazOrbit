using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.AutoComplete;

[Trait("Component Rendering", "BOBAutoComplete")]
public class BOBAutoCompleteRenderingTests
{
    private static Func<string, CancellationToken, Task<IEnumerable<string>>> StaticSource(params string[] items) =>
        (q, _) => Task.FromResult(items.Where(i => i.Contains(q, StringComparison.OrdinalIgnoreCase)));

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("alpha"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.Label, "Search"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("auto-complete");
        root.GetAttribute("data-bob-variant").Should().Be("outlined");
        root.GetAttribute("data-bob-size").Should().Be("medium");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Combobox_Role_With_Aria_Wiring(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s));

        IElement input = cut.Find("input.bob-input__field");
        input.GetAttribute("role").Should().Be("combobox");
        input.GetAttribute("aria-autocomplete").Should().Be("list");
        input.GetAttribute("aria-expanded").Should().Be("false");
        input.GetAttribute("aria-controls").Should().NotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Panel_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s));

        cut.FindAll(".bob-autocomplete__panel").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_HelperText_And_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("a"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.Label, "Country")
            .Add(c => c.HelperText, "Type to search.")
            .Add(c => c.Required, true));

        cut.Find(".bob-input__label").TextContent.Should().Contain("Country");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Type to search.");
        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hydrate_Search_Text_From_Initial_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(p => p
            .Add(c => c.SearchAsync, StaticSource("Spain"))
            .Add(c => c.ItemText, s => s)
            .Add(c => c.Value, "Spain"));

        cut.Find("input.bob-input__field").GetAttribute("value").Should().Be("Spain");
    }
}
