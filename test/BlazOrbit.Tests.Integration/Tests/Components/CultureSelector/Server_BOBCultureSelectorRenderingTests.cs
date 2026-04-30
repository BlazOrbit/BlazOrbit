using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using BOBCultureSelector = BlazOrbit.Components.Server.BOBCultureSelector;
using BOBCultureSelectorVariant = BlazOrbit.Localization.Shared.BOBCultureSelectorVariant;
using LocalizationSettings = BlazOrbit.Localization.Server.ServerLocalizationSettings;

namespace BlazOrbit.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Rendering", "BOBCultureSelector")]
public class Server_BOBCultureSelectorRenderingTests : TestFixtureBase<ServerTestContext>
{
    public Server_BOBCultureSelectorRenderingTests(ServerTestContext context) : base(context)
    {
    }

    [Fact]
    public async Task Should_Render_Dropdown_Variant_With_Options()
    {
        // Arrange
        LocalizationSettings localizationSettings = Context.Services.GetRequiredService<LocalizationSettings>();

        // Act
        Bunit.IRenderedComponent<BOBCultureSelector> cut = Context.Render<BOBCultureSelector>(p => p
            .Add(c => c.Variant, BOBCultureSelectorVariant.Dropdown));

        // BOBInputDropdown only emits option DOM when the menu is open — open it first.
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert
        IReadOnlyList<IElement> options = cut.FindAll(".bob-dropdown__option");
        options.Count.Should().Be(localizationSettings.SupportedCultures.Count);
    }

    [Fact]
    public async Task Should_Render_Flags_When_ShowFlag_Is_True()
    {
        // Act
        Bunit.IRenderedComponent<BOBCultureSelector> cut = Context.Render<BOBCultureSelector>(p => p
            .Add(c => c.Variant, BOBCultureSelectorVariant.Flags)
            .Add(c => c.ShowFlag, true));

        // Assert
        cut.Markup.Should().Contain("🇺🇸"); // en-US flag
        cut.Markup.Should().Contain("🇪🇸"); // es-ES flag
    }
}