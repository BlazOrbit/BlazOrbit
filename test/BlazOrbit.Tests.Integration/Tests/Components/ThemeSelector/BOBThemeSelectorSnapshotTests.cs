using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Snapshots", "BOBThemeSelector")]
public class BOBThemeSelectorSnapshotTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx, string theme = "light")
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>(theme));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_ThemeSelector_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx, "light");

        (string Name, Action<ComponentParameterCollectionBuilder<BOBThemeSelector>> Builder)[] testCases =
        [
            ("Default_Light", p => p
                .Add(c => c.Variant, BOBThemeSelectorVariant.Default)
                .Add(c => c.ShowIcon, true)),
            ("Default_NoIcon", p => p
                .Add(c => c.Variant, BOBThemeSelectorVariant.Default)
                .Add(c => c.ShowIcon, false)),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
