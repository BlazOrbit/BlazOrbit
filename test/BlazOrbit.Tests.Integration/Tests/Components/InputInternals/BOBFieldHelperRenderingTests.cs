using AngleSharp.Dom;
using BlazOrbit.Components.Internal;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "_BOBFieldHelper")]
public class BOBFieldHelperRenderingTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_No_Parameters(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>();

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Helper_Text_In_FieldHelper_Div(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.HelperText, "As it appears on your ID.")
            .Add(c => c.Id, "helper-1"));

        // Assert
        IElement helper = cut.Find("div.bob-field-helper");
        helper.GetAttribute("id").Should().Be("helper-1");
        helper.TextContent.Should().Be("As it appears on your ID.");
        helper.ClassList.Should().NotContain("bob-field-helper--error");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Helper_When_HelperText_Whitespace(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.HelperText, "   "));

        // Assert
        cut.FindAll("div.bob-field-helper").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Validation_Block_When_ShowValidation_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        EditContext editContext = new(model);
        Expression<Func<string?>> expr = () => model.Value;

        // Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.ShowValidation, false)
            .Add(c => c.EditContext, editContext)
            .Add(c => c.For, expr));

        // Assert
        cut.FindAll("div.bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Validation_Block_When_EditContext_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        Expression<Func<string?>> expr = () => model.Value;

        // Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.ShowValidation, true)
            .Add(c => c.For, expr));

        // Assert
        cut.FindAll("div.bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Validation_Block_When_For_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        EditContext editContext = new(model);

        // Act
        IRenderedComponent<_BOBFieldHelper<string?>> cut = ctx.Render<_BOBFieldHelper<string?>>(p => p
            .Add(c => c.ShowValidation, true)
            .Add(c => c.EditContext, editContext));

        // Assert
        cut.FindAll("div.bob-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Validation_Block_When_All_Conditions_Met(BlazorScenario scenario)
    {
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
        IElement errorWrapper = cut.Find("div.bob-field-helper.bob-field-helper--error");
        errorWrapper.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Both_Validation_And_Helper_When_Both_Supplied(BlazorScenario scenario)
    {
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
            .Add(c => c.HelperText, "Required field")
            .AddCascadingValue(editContext));

        // Assert
        cut.FindAll("div.bob-field-helper").Should().HaveCount(2);
        cut.FindAll("div.bob-field-helper--error").Should().HaveCount(1);
    }
}
