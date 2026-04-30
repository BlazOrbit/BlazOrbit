using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Globalization;
using ServerSelector = BlazOrbit.Components.Server.BOBCultureSelector;
using SharedVariant = BlazOrbit.Localization.Shared.BOBCultureSelectorVariant;
using WasmSelector = BlazOrbit.Components.Wasm.BOBCultureSelector;

namespace BlazOrbit.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Interaction", "BOBCultureSelector")]
public class BOBCultureSelectorInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnCultureChanged_When_Dropdown_Selects_Different_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        CultureInfo? captured = null;
        IReadOnlyList<IElement> options;

        if (scenario.Name == "Server")
        {
            IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci));
            cut.Find("button.bob-dropdown__trigger").Click();
            options = cut.FindAll(".bob-dropdown__option");
        }
        else
        {
            IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci));
            cut.Find("button.bob-dropdown__trigger").Click();
            options = cut.FindAll(".bob-dropdown__option");
        }

        // Act — pick the option whose label is es-ES
        IElement target = options.First(o => o.TextContent.Contains("es-ES")
            || o.TextContent.Contains("español", StringComparison.OrdinalIgnoreCase)
            || o.TextContent.Contains("Spanish", StringComparison.OrdinalIgnoreCase));
        target.Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("es-ES");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_OnCultureChanged_For_Same_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int callCount = 0;
        string currentCulture = CultureInfo.CurrentUICulture.Name;
        IReadOnlyList<IElement> options;

        if (scenario.Name == "Server")
        {
            IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo _) => callCount++));
            cut.Find("button.bob-dropdown__trigger").Click();
            options = cut.FindAll(".bob-dropdown__option");
        }
        else
        {
            IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo _) => callCount++));
            cut.Find("button.bob-dropdown__trigger").Click();
            options = cut.FindAll(".bob-dropdown__option");
        }

        // Act — click the option matching the already-active culture
        IElement? sameCultureOption = options
            .FirstOrDefault(o => o.GetAttribute("aria-selected") == "true");
        sameCultureOption?.Click();

        // Assert — no culture change event for same culture
        callCount.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnCultureChanged_When_Non_Active_Flag_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        CultureInfo? captured = null;
        IReadOnlyList<IElement> buttons;

        if (scenario.Name == "Server")
        {
            IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci));
            buttons = cut.FindAll("button");
        }
        else
        {
            IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci));
            buttons = cut.FindAll("button");
        }

        // Act — click a non-disabled (non-active) button
        IElement? clickable = buttons.FirstOrDefault(b => b.GetAttribute("disabled") == null);
        clickable?.Click();

        // Assert — callback fired if there was a non-active button
        if (clickable != null)
        {
            captured.Should().NotBeNull();
        }
    }
}
