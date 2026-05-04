using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Accordion;

[Trait("Component Snapshots", "BOBAccordion")]
public class BOBAccordionSnapshotTests
{
    private static RenderFragment TwoItems() => b =>
    {
        b.OpenComponent<BOBAccordionItem>(0);
        b.AddAttribute(1, "Id", "i1");
        b.AddAttribute(2, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "Header A")));
        b.AddAttribute(3, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Body A")));
        b.CloseComponent();
        b.OpenComponent<BOBAccordionItem>(4);
        b.AddAttribute(5, "Id", "i2");
        b.AddAttribute(6, nameof(BOBAccordionItem.Header), (RenderFragment)(b2 => b2.AddContent(0, "Header B")));
        b.AddAttribute(7, nameof(BOBAccordionItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Body B")));
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
                Name = "Multiple_All_Collapsed",
                Html = ctx.Render<BOBAccordion>(p => p
                    .Add(c => c.ChildContent, TwoItems())).GetNormalizedMarkup()
            },
            new
            {
                Name = "Multiple_First_Expanded",
                Html = ctx.Render<BOBAccordion>(p => p
                    .Add(c => c.ExpandedItems, new[] { "i1" })
                    .Add(c => c.ChildContent, TwoItems())).GetNormalizedMarkup()
            },
            new
            {
                Name = "SingleStrict_AutoFirst",
                Html = ctx.Render<BOBAccordion>(p => p
                    .Add(c => c.Mode, BOBAccordionMode.SingleStrict)
                    .Add(c => c.ChildContent, TwoItems())).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
