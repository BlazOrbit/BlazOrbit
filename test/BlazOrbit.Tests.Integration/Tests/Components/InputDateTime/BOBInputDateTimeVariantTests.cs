using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Variants", "BOBInputDateTime")]
public class BOBInputDateTimeVariantTests
{
    private class DummyModel { public DateOnly? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<DateOnly?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Outlined_Variant_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("outlined");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Filled_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Variant, BOBInputVariant.Filled));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("filled");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Standard_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Variant, BOBInputVariant.Standard));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("standard");
    }
}
