using BlazOrbit.Components.Navigation;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Stepper;

[Trait("Component Rendering", "BOBStepper")]
public class BOBStepperRenderingTests
{
    private static RenderFragment ThreeSteps() => b =>
    {
        b.OpenComponent<BOBStep>(0);
        b.AddAttribute(1, "Title", "Account");
        b.AddAttribute(2, "ChildContent", (RenderFragment)(c => c.AddMarkupContent(0, "<span class='step-body s1'>account</span>")));
        b.CloseComponent();

        b.OpenComponent<BOBStep>(3);
        b.AddAttribute(4, "Title", "Plan");
        b.AddAttribute(5, "ChildContent", (RenderFragment)(c => c.AddMarkupContent(0, "<span class='step-body s2'>plan</span>")));
        b.CloseComponent();

        b.OpenComponent<BOBStep>(6);
        b.AddAttribute(7, "Title", "Confirm");
        b.AddAttribute(8, "ChildContent", (RenderFragment)(c => c.AddMarkupContent(0, "<span class='step-body s3'>confirm</span>")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_All_Step_Headers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps()));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("stepper");
        cut.FindAll(".bob-stepper__entry").Count.Should().Be(3);
        cut.FindAll(".bob-stepper__title").Select(t => t.TextContent.Trim())
            .Should().BeEquivalentTo(new[] { "Account", "Plan", "Confirm" });
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_CurrentStep_To_Zero_With_Active_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps()));

        var entries = cut.FindAll(".bob-stepper__entry");
        entries[0].GetAttribute("data-bob-state").Should().Be("active");
        entries[1].GetAttribute("data-bob-state").Should().Be("pending");
        entries[2].GetAttribute("data-bob-state").Should().Be("pending");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Past_Steps_Complete(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.CurrentStep, 2));

        var entries = cut.FindAll(".bob-stepper__entry");
        entries[0].GetAttribute("data-bob-state").Should().Be("complete");
        entries[1].GetAttribute("data-bob-state").Should().Be("complete");
        entries[2].GetAttribute("data-bob-state").Should().Be("active");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Active_Step_Body(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.CurrentStep, 1));

        cut.Find(".bob-stepper__body").TextContent.Trim().Should().Be("plan");
        cut.FindAll(".bob-stepper__body .step-body").Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Step_With_Error_Override(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        RenderFragment children = b =>
        {
            b.OpenComponent<BOBStep>(0);
            b.AddAttribute(1, "Title", "ok");
            b.CloseComponent();

            b.OpenComponent<BOBStep>(2);
            b.AddAttribute(3, "Title", "broken");
            b.AddAttribute(4, "Error", true);
            b.CloseComponent();
        };

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, children));

        var entries = cut.FindAll(".bob-stepper__entry");
        entries[1].GetAttribute("data-bob-state").Should().Be("error");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Vertical_Token(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.Orientation, BOBStepperOrientation.Vertical));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("vertical");
    }
}

[Trait("Component Interaction", "BOBStepper")]
public class BOBStepperInteractionTests
{
    private static RenderFragment ThreeSteps() => b =>
    {
        b.OpenComponent<BOBStep>(0);
        b.AddAttribute(1, "Title", "A");
        b.CloseComponent();
        b.OpenComponent<BOBStep>(2);
        b.AddAttribute(3, "Title", "B");
        b.CloseComponent();
        b.OpenComponent<BOBStep>(4);
        b.AddAttribute(5, "Title", "C");
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Block_Click_On_Pending_Step_When_AllowSkip_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.CurrentStep, 0));

        var headers = cut.FindAll(".bob-stepper__header");
        headers[2].GetAttribute("disabled").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_Click_On_Pending_Step_When_AllowSkip(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int captured = -1;
        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.AllowSkip, true)
            .Add(c => c.CurrentStepChanged, v => captured = v));

        cut.FindAll(".bob-stepper__header")[2].Click();
        captured.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Cancel_Transition_When_OnStepChange_Sets_Cancel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int captured = -1;
        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.AllowSkip, true)
            .Add(c => c.OnStepChange, EventCallback.Factory.Create<BOBStepChangeEventArgs>(this, args => args.Cancel = true))
            .Add(c => c.CurrentStepChanged, v => captured = v));

        cut.FindAll(".bob-stepper__header")[2].Click();
        captured.Should().Be(-1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Move_Forward_Via_NextAsync(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps()));

        await cut.InvokeAsync(() => cut.Instance.NextAsync());
        cut.Instance.CurrentStep.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_NextAsync_At_Last_Step(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.CurrentStep, 2));

        await cut.InvokeAsync(() => cut.Instance.NextAsync());
        cut.Instance.CurrentStep.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_PreviousAsync_At_Zero(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, ThreeSteps())
            .Add(c => c.CurrentStep, 0));

        await cut.InvokeAsync(() => cut.Instance.PreviousAsync());
        cut.Instance.CurrentStep.Should().Be(0);
    }
}

[Trait("Component Accessibility", "BOBStepper")]
public class BOBStepperAccessibilityTests
{
    private static RenderFragment TwoSteps() => b =>
    {
        b.OpenComponent<BOBStep>(0);
        b.AddAttribute(1, "Title", "Now");
        b.CloseComponent();
        b.OpenComponent<BOBStep>(2);
        b.AddAttribute(3, "Title", "Later");
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Active_Step_With_Aria_Current(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(p => p
            .Add(c => c.ChildContent, TwoSteps()));

        var headers = cut.FindAll(".bob-stepper__header");
        headers[0].GetAttribute("aria-current").Should().Be("step");
        headers[1].GetAttribute("aria-current").Should().BeNull();
    }
}

[Trait("Component Snapshots", "BOBStepper")]
public class BOBStepperSnapshotTests
{
    private static RenderFragment ThreeSteps() => b =>
    {
        b.OpenComponent<BOBStep>(0);
        b.AddAttribute(1, "Title", "Account");
        b.AddAttribute(2, "Description", "Sign up");
        b.CloseComponent();
        b.OpenComponent<BOBStep>(3);
        b.AddAttribute(4, "Title", "Plan");
        b.CloseComponent();
        b.OpenComponent<BOBStep>(5);
        b.AddAttribute(6, "Title", "Confirm");
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBStepper>>)(p => p
                .Add(c => c.ChildContent, ThreeSteps())) },
            new { Name = "Middle", Builder = (Action<ComponentParameterCollectionBuilder<BOBStepper>>)(p => p
                .Add(c => c.ChildContent, ThreeSteps())
                .Add(c => c.CurrentStep, 1)) },
            new { Name = "Vertical", Builder = (Action<ComponentParameterCollectionBuilder<BOBStepper>>)(p => p
                .Add(c => c.ChildContent, ThreeSteps())
                .Add(c => c.Orientation, BOBStepperOrientation.Vertical)) },
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBStepper> cut = ctx.Render<BOBStepper>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
