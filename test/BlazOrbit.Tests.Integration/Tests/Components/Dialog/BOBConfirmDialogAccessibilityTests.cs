using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Accessibility", "BOBConfirmDialog")]
public class BOBConfirmDialogAccessibilityTests
{
    private static ModalReference DummyReference => new("test-ref", _ => Task.CompletedTask);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Title_As_Heading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "Delete file?")
            .Add(c => c.Message, "M"));

        cut.Find(".bob-confirm__title").NodeName.Should().Be("H2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Icon_As_Aria_Hidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBConfirmDialog> cut = ctx.Render<BOBConfirmDialog>(p => p
            .Add(c => c.ModalReference, DummyReference)
            .Add(c => c.Title, "T")
            .Add(c => c.Message, "M"));

        cut.Find(".bob-confirm__icon").GetAttribute("aria-hidden").Should().Be("true");
    }
}
