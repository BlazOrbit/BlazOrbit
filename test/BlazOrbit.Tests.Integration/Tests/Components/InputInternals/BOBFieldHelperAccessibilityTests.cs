using BlazOrbit.Components.Internal;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Accessibility", "_BOBFieldHelper")]
public class BOBFieldHelperAccessibilityTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Validation_Block_As_Alert(BlazorScenario scenario)
    {
        // WCAG 4.1.3 + 3.3.1: validation errors must interrupt the screen
        // reader so users notice the form failed before navigating away.
        // role="alert" implies aria-live="assertive" + aria-atomic="true" per
        // the ARIA spec — that is what we assert.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        EditContext editContext = new(model);
        Expression<Func<string?>> expr = () => model.Value;

        // Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.ShowValidation, true)
            .Add(c => c.EditContext, editContext)
            .Add(c => c.For, expr)
            .AddCascadingValue(editContext));

        // Assert
        cut.Find("div[data-bob-error=\"true\"]").GetAttribute("role").Should().Be("alert");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_HelperText_As_Polite_Live_Region(BlazorScenario scenario)
    {
        // Informational helper text uses aria-live="polite" so dynamic updates
        // are announced after the screen reader finishes its current sentence
        // (no interruption for non-error guidance).
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.HelperText, "As it appears on your ID."));

        // Assert
        cut.Find("div.bob-field-helper").GetAttribute("aria-live").Should().Be("polite");
    }
}
