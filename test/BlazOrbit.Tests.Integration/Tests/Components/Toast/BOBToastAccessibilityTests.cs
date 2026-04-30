using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Accessibility", "BOBToast")]
public class BOBToastAccessibilityTests
{
    private static ToastState CreateState(bool closable = true, ToastSeverity severity = ToastSeverity.Info) => new()
    {
        Content = b => b.AddContent(0, "msg"),
        Options = new ToastOptions { Closable = closable, AutoDismiss = false, Severity = severity }
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Button_Have_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState(closable: true)));

        // Assert
        cut.Find("[aria-label='Close']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content_With_Toast_Class(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState()));

        // Assert
        cut.Find(".bob-toast__content").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Polite_Status_For_NonError_Severities(BlazorScenario scenario)
    {
        // WCAG 4.1.3 Status Messages: non-error toasts announce politely so they
        // don't interrupt the screen reader's current output.
        foreach (ToastSeverity severity in new[] { ToastSeverity.Info, ToastSeverity.Success, ToastSeverity.Warning })
        {
            await using BlazorTestContextBase ctx = scenario.CreateContext();

            IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
                .Add(c => c.State, CreateState(severity: severity)));

            AngleSharp.Dom.IElement root = cut.Find("bob-component");
            root.GetAttribute("role").Should().Be("status", $"severity={severity}");
            root.GetAttribute("aria-live").Should().Be("polite", $"severity={severity}");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Assertive_Alert_For_Error_Severity(BlazorScenario scenario)
    {
        // role="alert" implies aria-live="assertive"; we set both so consumers
        // can rely on either attribute when querying / styling.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState(severity: ToastSeverity.Error)));

        AngleSharp.Dom.IElement root = cut.Find("bob-component");
        root.GetAttribute("role").Should().Be("alert");
        root.GetAttribute("aria-live").Should().Be("assertive");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_AriaAtomic_True_So_Whole_Toast_Is_Read(BlazorScenario scenario)
    {
        // aria-atomic="true" tells assistive tech to announce the entire toast
        // on change rather than diffing the descendant text.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, CreateState()));

        cut.Find("bob-component").GetAttribute("aria-atomic").Should().Be("true");
    }
}
