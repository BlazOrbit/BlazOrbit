using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Snapshots", "BOBToastHost")]
public class BOBToastHostSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBToastHost> cut = ctx.Render<BOBToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        string emptyMarkup = cut.GetNormalizedMarkup();

        await toastService.ShowAsync(
            b => b.AddContent(0, "Top right toast"),
            new ToastOptions { AutoDismiss = false, Position = ToastPosition.TopRight });
        string singleTopRight = cut.GetNormalizedMarkup();

        await toastService.ShowAsync(
            b => b.AddContent(0, "Bottom left toast"),
            new ToastOptions { AutoDismiss = false, Position = ToastPosition.BottomLeft });
        string twoPositions = cut.GetNormalizedMarkup();

        var testCases = new[]
        {
            new { Name = "Empty", Html = emptyMarkup },
            new { Name = "Single_TopRight", Html = singleTopRight },
            new { Name = "Two_Positions", Html = twoPositions },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
