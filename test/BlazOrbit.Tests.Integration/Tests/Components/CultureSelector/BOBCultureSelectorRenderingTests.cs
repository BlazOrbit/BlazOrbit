using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServerSelector = BlazOrbit.Components.Server.BOBCultureSelector;
using ServerSettings = BlazOrbit.Localization.Server.ServerLocalizationSettings;
using SharedVariant = BlazOrbit.Localization.Shared.BOBCultureSelectorVariant;
using WasmSelector = BlazOrbit.Components.Wasm.BOBCultureSelector;
using WasmSettings = BlazOrbit.Localization.Wasm.WasmLocalizationSettings;

namespace BlazOrbit.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Rendering", "BOBCultureSelector")]
public class BOBCultureSelectorRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dropdown_With_BOBInputDropdown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Dropdown)).Markup
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Dropdown)).Markup;

        // Assert
        markup.Should().Contain("data-bob-component=\"dropdown-container\"");
        markup.Should().Contain("bob-dropdown__trigger");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Options_For_Each_Supported_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int expectedCount = scenario.Name == "Server"
            ? ctx.Services.GetRequiredService<ServerSettings>().SupportedCultures.Count
            : ctx.Services.GetRequiredService<WasmSettings>().SupportedCultures.Count;

        // BOBInputDropdown only emits option DOM when the menu is open — open it first.
        IReadOnlyList<IElement> options;
        if (scenario.Name == "Server")
        {
            IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(
                p => p.Add(c => c.Variant, SharedVariant.Dropdown));
            cut.Find("button.bob-dropdown__trigger").Click();
            options = cut.FindAll(".bob-dropdown__option");
        }
        else
        {
            IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(
                p => p.Add(c => c.Variant, SharedVariant.Dropdown));
            cut.Find("button.bob-dropdown__trigger").Click();
            options = cut.FindAll(".bob-dropdown__option");
        }

        // Assert
        options.Should().HaveCount(expectedCount);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Flags_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> buttons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)).FindAll("button")
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)).FindAll("button");

        // Assert
        buttons.Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Flag_Emoji_When_ShowFlag_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)).Markup
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)).Markup;

        // Assert
        markup.Should().Contain("🇺🇸");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Culture_Name_When_ShowName_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, false)
                .Add(c => c.ShowName, true)).Markup
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, false)
                .Add(c => c.ShowName, true)).Markup;

        // Assert
        markup.Should().Contain("English");
    }
}
