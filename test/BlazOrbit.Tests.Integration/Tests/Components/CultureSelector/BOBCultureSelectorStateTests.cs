using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using ServerSelector = BlazOrbit.Components.Server.BOBCultureSelector;
using SharedVariant = BlazOrbit.Localization.Shared.BOBCultureSelectorVariant;
using WasmSelector = BlazOrbit.Components.Wasm.BOBCultureSelector;

namespace BlazOrbit.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component State", "BOBCultureSelector")]
public class BOBCultureSelectorStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Flags_In_Dropdown_When_ShowFlag_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Open the dropdown menu so option content appears in the DOM.
        string markup;
        if (scenario.Name == "Server")
        {
            IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.ShowFlag, false));
            cut.Find("button.bob-dropdown__trigger").Click();
            markup = cut.Markup;
        }
        else
        {
            IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.ShowFlag, false));
            cut.Find("button.bob-dropdown__trigger").Click();
            markup = cut.Markup;
        }

        // Assert — no flag emojis in option text
        markup.Should().NotContain("🇺🇸");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Names_When_ShowName_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> labels = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, false)).FindAll(".bob-culture-selector__flag-label")
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, false)).FindAll(".bob-culture-selector__flag-label");

        // Assert
        labels.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Current_Culture_Button_As_Active(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> activeButtons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags)).FindAll("button.active")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags)).FindAll("button.active");

        // Assert
        activeButtons.Should().HaveCount(1);
        activeButtons[0].GetAttribute("disabled").Should().NotBeNull();
    }
}
