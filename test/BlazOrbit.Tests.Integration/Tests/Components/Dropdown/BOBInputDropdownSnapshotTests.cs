using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Snapshots", "BOBInputDropdown")]
public class BOBInputDropdownSnapshotTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    private static Action<ComponentParameterCollectionBuilder<BOBInputDropdown<string>>> BuildWithOptions(
        string? value = null, bool disabled = false, bool required = false, string? label = null, string? helper = null)
    {
        return p =>
        {
            p.Add(c => c.ValueExpression, _expr);
            if (value != null)
            {
                p.Add(c => c.Value, value);
            }

            if (disabled)
            {
                p.Add(c => c.Disabled, true);
            }

            if (required)
            {
                p.Add(c => c.Required, true);
            }

            if (label != null)
            {
                p.Add(c => c.Label, label);
            }

            if (helper != null)
            {
                p.Add(c => c.HelperText, helper);
            }

            p.Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
                builder.OpenComponent<DropdownOption<string>>(3);
                builder.AddAttribute(4, "Value", "opt2");
                builder.AddAttribute(5, "Text", "Option 2");
                builder.CloseComponent();
            });
        };
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default_Closed", Builder = BuildWithOptions(label: "Select") },
            new { Name = "With_Value", Builder = BuildWithOptions(value: "opt1", label: "Select") },
            new { Name = "Disabled", Builder = BuildWithOptions(disabled: true, label: "Select") },
            new { Name = "Required", Builder = BuildWithOptions(required: true, label: "Select") },
            new { Name = "With_Helper", Builder = BuildWithOptions(label: "Select", helper: "Pick one") }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputDropdown<string>> cut =
                ctx.Render<BOBInputDropdown<string>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
