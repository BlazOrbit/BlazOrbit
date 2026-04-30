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

[Trait("Component Accessibility", "BOBCultureSelector")]
public class BOBCultureSelectorAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Title_On_Flag_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> buttons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                  .FindAll(".bob-culture-selector__flag-button")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                  .FindAll(".bob-culture-selector__flag-button");

        // Assert
        buttons.Should().NotBeEmpty();
        foreach (IElement btn in buttons)
        {
            btn.GetAttribute("title").Should().NotBeNullOrEmpty();
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Current_Culture_Flag_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> disabledButtons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                  .FindAll("button[disabled]")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                  .FindAll("button[disabled]");

        // Assert
        disabledButtons.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_BOBInputDropdown_For_Dropdown_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IElement trigger = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Dropdown))
                  .Find("button.bob-dropdown__trigger")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Dropdown))
                  .Find("button.bob-dropdown__trigger");

        // Assert
        trigger.GetAttribute("aria-haspopup").Should().Be("listbox");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Localize_Dropdown_AriaLabel_On_Wrapper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        CultureInfo previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es");

            IElement wrapper = scenario.Name == "Server"
                ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Dropdown))
                      .Find("[data-bob-component=\"culture-selector\"]")
                : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Dropdown))
                      .Find("[data-bob-component=\"culture-selector\"]");

            wrapper.GetAttribute("role").Should().Be("group");
            wrapper.GetAttribute("aria-label").Should().Be("Seleccionar idioma");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Localize_AriaLabel_To_English_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — aria-label resolves via IStringLocalizer<BOBCultureSelectorResources>
        CultureInfo previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");

            // Act
            IElement flagList = scenario.Name == "Server"
                ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                      .Find(".bob-culture-selector__flag-list")
                : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                      .Find(".bob-culture-selector__flag-list");

            // Assert
            flagList.GetAttribute("role").Should().Be("radiogroup");
            flagList.GetAttribute("aria-label").Should().Be("Select language");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Localize_AriaLabel_To_Spanish_When_Culture_Is_es(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        CultureInfo previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es");

            // Act
            IElement flagList = scenario.Name == "Server"
                ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                      .Find(".bob-culture-selector__flag-list")
                : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, SharedVariant.Flags))
                      .Find(".bob-culture-selector__flag-list");

            // Assert
            flagList.GetAttribute("aria-label").Should().Be("Seleccionar idioma");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
