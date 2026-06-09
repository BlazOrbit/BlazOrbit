using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Banner;

[Trait("Component Rendering", "BOBBanner")]
public class BOBBannerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Message, "Hi"));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("banner");
        cut.Find(".bob-banner").GetAttribute("data-bob-severity").Should().Be("info");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Map_Severity_To_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Severity, BOBBannerSeverity.Error)
            .Add(c => c.Message, "Boom"));

        cut.Find(".bob-banner").GetAttribute("data-bob-severity").Should().Be("error");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Title_And_Message(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Title, "Heads up")
            .Add(c => c.Message, "Pay attention."));

        cut.Find(".bob-banner__title").TextContent.Should().Be("Heads up");
        cut.Find(".bob-banner__message").TextContent.Should().Contain("Pay attention.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Icon_When_ShowIcon_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Message, "x")
            .Add(c => c.ShowIcon, false));

        cut.FindAll(".bob-banner__icon").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_Over_Message(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Message, "Should not appear")
            .AddChildContent("<span>From child</span>"));

        cut.Find(".bob-banner__message").InnerHtml.Should().Contain("From child");
        cut.Find(".bob-banner__message").TextContent.Should().NotContain("Should not appear");
    }
}

[Trait("Component Interaction", "BOBBanner")]
public class BOBBannerInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_On_Dismiss_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Message, "x")
            .Add(c => c.Dismissible, true)
            .Add(c => c.OnDismissed, () => fired = true));

        cut.FindAll(".bob-banner").Should().HaveCount(1);

        cut.Find(".bob-banner ._bob-btn[data-bob-variant='ghost']").Click();

        fired.Should().BeTrue();
        cut.FindAll(".bob-banner").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Restore_On_Reset(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Message, "x")
            .Add(c => c.Dismissible, true));

        cut.Find(".bob-banner ._bob-btn[data-bob-variant='ghost']").Click();
        cut.FindAll(".bob-banner").Should().BeEmpty();

        await cut.InvokeAsync(() => cut.Instance.Reset());
        cut.Render();

        cut.FindAll(".bob-banner").Should().HaveCount(1);
    }
}

[Trait("Component Accessibility", "BOBBanner")]
public class BOBBannerAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Status_Role_For_Non_Error(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Severity, BOBBannerSeverity.Info)
            .Add(c => c.Message, "x"));

        cut.Find(".bob-banner").GetAttribute("role").Should().Be("status");
        cut.Find(".bob-banner").GetAttribute("aria-live").Should().Be("polite");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Alert_Role_For_Error(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Severity, BOBBannerSeverity.Error)
            .Add(c => c.Message, "x"));

        cut.Find(".bob-banner").GetAttribute("role").Should().Be("alert");
        cut.Find(".bob-banner").GetAttribute("aria-live").Should().Be("assertive");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Label_On_Dismiss(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
            .Add(c => c.Message, "x")
            .Add(c => c.Dismissible, true)
            .Add(c => c.DismissText, "Cerrar"));

        cut.Find(".bob-banner ._bob-btn[data-bob-variant='ghost']").GetAttribute("aria-label").Should().Be("Cerrar");
    }
}

[Trait("Component Snapshots", "BOBBanner")]
public class BOBBannerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Severities(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Info", Severity = BOBBannerSeverity.Info },
            new { Name = "Success", Severity = BOBBannerSeverity.Success },
            new { Name = "Warning", Severity = BOBBannerSeverity.Warning },
            new { Name = "Error", Severity = BOBBannerSeverity.Error }
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBBanner> cut = ctx.Render<BOBBanner>(p => p
                .Add(c => c.Severity, tc.Severity)
                .Add(c => c.Title, "Heads up")
                .Add(c => c.Message, "Sample message."));
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
