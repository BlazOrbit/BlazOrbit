using BlazOrbit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Trait("Localization", "BOBDataCollectionResources")]
public class BOBDataCollectionLocalizationTests
{
    [Fact]
    public void ResourceManager_Should_Load_Embedded_Resources_Directly()
    {
        System.Reflection.Assembly translationsAssembly =
            System.Reflection.Assembly.Load("BlazOrbit.Translations");

        ResourceManager rm = new(
            "BlazOrbit.Translations.Resources.BOBDataCollectionResources",
            translationsAssembly);

        rm.GetString("ClearFilter", CultureInfo.GetCultureInfo("en")).Should().Be("Clear filter");
        rm.GetString("ClearFilter", CultureInfo.GetCultureInfo("es")).Should().Be("Borrar filtro");
        rm.GetString("NoDataAvailable", CultureInfo.GetCultureInfo("en")).Should().Be("No data available");
        rm.GetString("NoDataAvailable", CultureInfo.GetCultureInfo("es")).Should().Be("No hay datos disponibles");
    }

    [Fact]
    public void IStringLocalizer_Should_Resolve_BOBDataCollectionResources()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddBlazOrbitLocalizationServer();
        ServiceProvider sp = services.BuildServiceProvider();

        IStringLocalizer<BOBDataCollectionResources> localizer =
            sp.GetRequiredService<IStringLocalizer<BOBDataCollectionResources>>();

        CultureInfo previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            localizer["ClearFilter"].Value.Should().Be("Clear filter");
            localizer["ItemsSelected", 3].Value.Should().Be("3 selected");
            localizer["SortPriority", 2].Value.Should().Be("Sort priority 2");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es");
            localizer["ClearFilter"].Value.Should().Be("Borrar filtro");
            localizer["ItemsSelected", 3].Value.Should().Be("3 seleccionados");
            localizer["SortPriority", 2].Value.Should().Be("Prioridad de ordenación 2");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
