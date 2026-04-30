using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Stubs;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Snapshots", "BOBModalHost")]
public class BOBModalHostSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBModalHost> cut = ctx.Render<BOBModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        string emptyMarkup = cut.GetNormalizedMarkup();

        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        string singleModalMarkup = cut.GetNormalizedMarkup();

        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        string stackedMarkup = cut.GetNormalizedMarkup();

        var testCases = new[]
        {
            new { Name = "Empty", Html = emptyMarkup },
            new { Name = "Single_Modal", Html = singleModalMarkup },
            new { Name = "Stacked_Modals", Html = stackedMarkup },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
