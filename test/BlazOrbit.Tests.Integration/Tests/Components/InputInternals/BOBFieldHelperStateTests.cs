using BlazOrbit.Components.Internal;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component State", "_BOBFieldHelper")]
public class BOBFieldHelperStateTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Validation_When_Error_Added(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        EditContext editContext = new(model);
        ValidationMessageStore messageStore = new(editContext);
        Expression<Func<string?>> expr = () => model.Value;

        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.ShowValidation, true)
            .Add(c => c.EditContext, editContext)
            .Add(c => c.For, expr)
            .Add(c => c.HelperText, "Helper")
            .AddCascadingValue(editContext));

        cut.FindAll("div.bob-field-helper--error").Should().HaveCount(1);
        cut.Find("div.bob-field-helper--error").TextContent.Should().BeEmpty();

        // Act
        messageStore.Add(FieldIdentifier.Create(expr), "Required field");
        await cut.InvokeAsync(editContext.NotifyValidationStateChanged);

        // Assert
        cut.Find("div.bob-field-helper--error").TextContent.Should().Contain("Required field");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Validation_When_ShowValidation_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        EditContext editContext = new(model);
        ValidationMessageStore messageStore = new(editContext);
        Expression<Func<string?>> expr = () => model.Value;
        messageStore.Add(FieldIdentifier.Create(expr), "Error");

        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.ShowValidation, true)
            .Add(c => c.EditContext, editContext)
            .Add(c => c.For, expr)
            .Add(c => c.HelperText, "Helper text")
            .AddCascadingValue(editContext));

        cut.FindAll("div.bob-field-helper--error").Should().HaveCount(1);

        // Act — toggle ShowValidation off
        IRenderedComponent<_BOBFieldHelper<string?>> hiddenCut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.ShowValidation, false)
            .Add(c => c.EditContext, editContext)
            .Add(c => c.For, expr)
            .Add(c => c.HelperText, "Helper text")
            .AddCascadingValue(editContext));

        // Assert
        hiddenCut.FindAll("div.bob-field-helper--error").Should().BeEmpty();
        hiddenCut.Find("div.bob-field-helper").TextContent.Should().Be("Helper text");
    }
}
