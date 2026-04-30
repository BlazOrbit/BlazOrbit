using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using ServerSelector = BlazOrbit.Components.Server.BOBCultureSelector;
using SharedVariant = BlazOrbit.Localization.Shared.BOBCultureSelectorVariant;
using WasmSelector = BlazOrbit.Components.Wasm.BOBCultureSelector;

namespace BlazOrbit.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Snapshots", "BOBCultureSelector")]
public class BOBCultureSelectorSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Dropdown_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup()
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Dropdown)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup();

        await Verifier.Verify(markup).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Flags_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup()
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, SharedVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup();

        await Verifier.Verify(markup).UseParameters(scenario.Name);
    }
}
