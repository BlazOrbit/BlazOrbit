using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Tests.Components.Initializer;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.BlazorLayout;

[Trait("Component Snapshots", "BOBBlazorLayout")]
public class BOBBlazorLayoutSnapshotTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BOBInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Layout_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        (string Name, Action<ComponentParameterCollectionBuilder<BOBBlazorLayout>> Builder)[] testCases =
        [
            ("EmptyBody", _ => { }),
            ("SimpleBody", p => p
                .Add(c => c.Body, (RenderFragment)(b => b.AddMarkupContent(0,
                    "<main class=\"app-main\">Hello</main>")))),
            ("StructuredBody", p => p
                .Add(c => c.Body, (RenderFragment)(b => b.AddMarkupContent(0,
                    "<header class=\"app-header\">H</header>"
                    + "<aside class=\"app-sidebar\">S</aside>"
                    + "<main class=\"app-main\">M</main>")))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verify(results).UseParameters(scenario.Name);
    }
}
