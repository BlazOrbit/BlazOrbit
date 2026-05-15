using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Snapshots", "BOBConfirmDialog")]
public class BOBConfirmDialogSnapshotTests
{
    private static ModalReference DummyReference => new("test-ref", _ => Task.CompletedTask);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BOBConfirmDialog>> Builder)[] testCases =
        [
            ("Info_Default", p => p
                .Add(c => c.ModalReference, DummyReference)
                .Add(c => c.Title, "Confirm")
                .Add(c => c.Message, "Are you sure?")),
            ("Warning", p => p
                .Add(c => c.ModalReference, DummyReference)
                .Add(c => c.Title, "Warning")
                .Add(c => c.Message, "This may break things.")
                .Add(c => c.Severity, ConfirmSeverity.Warning)),
            ("Danger", p => p
                .Add(c => c.ModalReference, DummyReference)
                .Add(c => c.Title, "Danger")
                .Add(c => c.Message, "This cannot be undone.")
                .Add(c => c.Severity, ConfirmSeverity.Danger)),
            ("Custom_Labels", p => p
                .Add(c => c.ModalReference, DummyReference)
                .Add(c => c.Title, "Delete")
                .Add(c => c.Message, "Remove this item?")
                .Add(c => c.YesLabel, "Delete")
                .Add(c => c.NoLabel, "Keep"))
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verify(results).UseParameters(scenario.Name);
    }
}
