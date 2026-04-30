using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Snapshots", "BOBTabs")]
public class BOBTabsSnapshotTests
{
    private static RenderFragment TwoTabs(string? activeId = null) => b =>
    {
        b.OpenComponent<BOBTab>(0);
        b.AddAttribute(1, "Id", "tab1");
        b.AddAttribute(2, "Label", "Tab One");
        b.AddAttribute(3, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Content One")));
        b.CloseComponent();
        b.OpenComponent<BOBTab>(4);
        b.AddAttribute(5, "Id", "tab2");
        b.AddAttribute(6, "Label", "Tab Two");
        b.AddAttribute(7, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Content Two")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Underline_First_Active",
                Html = ctx.Render<BOBTabs>(p => p
                    .Add(c => c.ChildContent, TwoTabs())
                    .Add(c => c.ActiveTab, "tab1")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Underline_Second_Active",
                Html = ctx.Render<BOBTabs>(p => p
                    .Add(c => c.ChildContent, TwoTabs())
                    .Add(c => c.ActiveTab, "tab2")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Pills_Variant",
                Html = ctx.Render<BOBTabs>(p => p
                    .Add(c => c.ChildContent, TwoTabs())
                    .Add(c => c.Variant, BOBTabsVariant.Pills)
                    .Add(c => c.ActiveTab, "tab1")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Enclosed_Variant",
                Html = ctx.Render<BOBTabs>(p => p
                    .Add(c => c.ChildContent, TwoTabs())
                    .Add(c => c.Variant, BOBTabsVariant.Enclosed)
                    .Add(c => c.ActiveTab, "tab1")).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
