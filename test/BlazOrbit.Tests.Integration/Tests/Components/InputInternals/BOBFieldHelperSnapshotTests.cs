using BlazOrbit.Components.Internal;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Snapshots", "_BOBFieldHelper")]
public class BOBFieldHelperSnapshotTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Representative_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        EditContext editContext = new(model);
        Expression<Func<string?>> expr = () => model.Value;

        var testCases = new[]
        {
            new { Name = "Empty", Builder = (Action<ComponentParameterCollectionBuilder<_BOBFieldHelper<string?>>>)(p => p
                .Add(c => c.EditContext, editContext)
                .AddCascadingValue(editContext)) },

            new { Name = "HelperOnly", Builder = (Action<ComponentParameterCollectionBuilder<_BOBFieldHelper<string?>>>)(p => p
                .Add(c => c.EditContext, editContext)
                .Add(c => c.HelperText, "As it appears on your ID.")
                .Add(c => c.Id, "helper-1")
                .AddCascadingValue(editContext)) },

            new { Name = "ValidationOnly", Builder = (Action<ComponentParameterCollectionBuilder<_BOBFieldHelper<string?>>>)(p => p
                .Add(c => c.EditContext, editContext)
                .Add(c => c.ShowValidation, true)
                .Add(c => c.For, expr)
                .AddCascadingValue(editContext)) },

            new { Name = "HelperAndValidation", Builder = (Action<ComponentParameterCollectionBuilder<_BOBFieldHelper<string?>>>)(p => p
                .Add(c => c.EditContext, editContext)
                .Add(c => c.HelperText, "Required field")
                .Add(c => c.ShowValidation, true)
                .Add(c => c.For, expr)
                .AddCascadingValue(editContext)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
