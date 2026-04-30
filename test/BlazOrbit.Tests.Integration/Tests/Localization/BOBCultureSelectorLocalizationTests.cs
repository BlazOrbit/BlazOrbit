using BlazOrbit.Localization.Shared;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Trait("Localization", "BOBCultureSelectorResources")]
public class BOBCultureSelectorLocalizationTests
{
    [Fact]
    public void ResourceManager_Should_Load_Embedded_Resources_Directly()
    {
        // Resources live in the sidecar `BlazOrbit.Translations` assembly.
        // The marker type is still `BOBCultureSelectorResources` (in Localization.Shared);
        // resource lookups are rerouted by ReroutedStringLocalizerFactory.
        System.Reflection.Assembly translationsAssembly =
            System.Reflection.Assembly.Load("BlazOrbit.Translations");

        ResourceManager rm = new(
            "BlazOrbit.Translations.Resources.BOBCultureSelectorResources",
            translationsAssembly);

        rm.GetString("SelectLanguage", CultureInfo.GetCultureInfo("en")).Should().Be("Select language");
        rm.GetString("SelectLanguage", CultureInfo.GetCultureInfo("es")).Should().Be("Seleccionar idioma");
    }

    [Fact]
    public void IStringLocalizer_Should_Resolve_BOBCultureSelectorResources()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddBlazOrbitLocalizationServer();
        ServiceProvider sp = services.BuildServiceProvider();

        IStringLocalizer<BOBCultureSelectorResources> localizer =
            sp.GetRequiredService<IStringLocalizer<BOBCultureSelectorResources>>();

        CultureInfo previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            localizer["SelectLanguage"].Value.Should().Be("Select language");
            localizer["CurrentLanguage", "English"].Value.Should().Be("Current language: English");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es");
            localizer["SelectLanguage"].Value.Should().Be("Seleccionar idioma");
            localizer["CurrentLanguage", "Inglés"].Value.Should().Be("Idioma actual: Inglés");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
