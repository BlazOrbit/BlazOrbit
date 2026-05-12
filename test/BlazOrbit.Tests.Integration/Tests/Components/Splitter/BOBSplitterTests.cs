using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Splitter;

[Trait("Component Rendering", "BOBSplitter")]
public class BOBSplitterRenderingTests
{
    private static RenderFragment TwoPanes() => b =>
    {
        b.OpenComponent<BOBSplitterPane>(0);
        b.AddAttribute(1, "Size", 200d);
        b.AddAttribute(2, "MinSize", 100d);
        b.AddAttribute(3, "ChildContent", (RenderFragment)(c => c.AddMarkupContent(0, "<span class='p1'>p1</span>")));
        b.CloseComponent();

        b.OpenComponent<BOBSplitterPane>(4);
        b.AddAttribute(5, "Flex", true);
        b.AddAttribute(6, "ChildContent", (RenderFragment)(c => c.AddMarkupContent(0, "<span class='p2'>p2</span>")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Each_Pane(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("splitter");
        cut.FindAll(".bob-splitter__pane").Count.Should().Be(2);
        cut.FindAll(".p1").Count.Should().Be(1);
        cut.FindAll(".p2").Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Gripper_Between_Panes_Only(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        // 2 panes -> 1 gripper.
        cut.FindAll(".bob-splitter__gripper").Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Initial_Size_To_Sized_Pane(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        IElement sized = cut.FindAll(".bob-splitter__pane")[0];
        sized.GetAttribute("style").Should().Contain("200px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Flex_Pane_Without_Fixed_Size(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        IElement flex = cut.FindAll(".bob-splitter__pane")[1];
        flex.GetAttribute("style").Should().Contain("flex:1 1 0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Vertical_Token(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes())
            .Add(c => c.Orientation, BOBSplitterOrientation.Vertical));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("vertical");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Drag_Overlay_Only_While_Dragging(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        // No overlay initially.
        cut.FindAll(".bob-splitter__drag-overlay").Should().BeEmpty();

        // Pointer-down on the gripper toggles the overlay on so subsequent pointermove
        // can land on it without escaping the splitter root.
        cut.Find(".bob-splitter__gripper").PointerDown();
        cut.FindAll(".bob-splitter__drag-overlay").Count.Should().Be(1);

        // Pointer-up tears the overlay back down.
        cut.Find(".bob-splitter__drag-overlay").PointerUp();
        cut.FindAll(".bob-splitter__drag-overlay").Should().BeEmpty();
    }
}

[Trait("Component Interaction", "BOBSplitter")]
public class BOBSplitterInteractionTests
{
    private static RenderFragment TwoPanes() => b =>
    {
        b.OpenComponent<BOBSplitterPane>(0);
        b.AddAttribute(1, "Size", 200d);
        b.AddAttribute(2, "MinSize", 100d);
        b.AddAttribute(3, "MaxSize", 400d);
        b.CloseComponent();

        b.OpenComponent<BOBSplitterPane>(4);
        b.AddAttribute(5, "Size", 200d);
        b.AddAttribute(6, "MinSize", 100d);
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Resize_Panes_On_Drag(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        // Start drag at clientX=500, then move to clientX=560 (delta +60). The leading
        // pane should grow from 200 to 260, the trailing one should shrink from 200 to 140.
        cut.Find(".bob-splitter__gripper").PointerDown(new PointerEventArgs { ClientX = 500, ClientY = 0 });
        cut.Find(".bob-splitter__drag-overlay").PointerMove(new PointerEventArgs { ClientX = 560, ClientY = 0 });

        IReadOnlyList<IElement> panes = cut.FindAll(".bob-splitter__pane");
        panes[0].GetAttribute("style").Should().Contain("260px");
        panes[1].GetAttribute("style").Should().Contain("140px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_To_MinSize_On_Drag(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        // Drag far left so the leading pane would shrink below MinSize=100.
        cut.Find(".bob-splitter__gripper").PointerDown(new PointerEventArgs { ClientX = 500, ClientY = 0 });
        cut.Find(".bob-splitter__drag-overlay").PointerMove(new PointerEventArgs { ClientX = 0, ClientY = 0 });

        IReadOnlyList<IElement> panes = cut.FindAll(".bob-splitter__pane");
        panes[0].GetAttribute("style").Should().Contain("100px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_To_MaxSize_On_Drag(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes()));

        // Drag far right so the leading pane would grow past MaxSize=400.
        cut.Find(".bob-splitter__gripper").PointerDown(new PointerEventArgs { ClientX = 500, ClientY = 0 });
        cut.Find(".bob-splitter__drag-overlay").PointerMove(new PointerEventArgs { ClientX = 9999, ClientY = 0 });

        IReadOnlyList<IElement> panes = cut.FindAll(".bob-splitter__pane");
        panes[0].GetAttribute("style").Should().Contain("400px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Resize_Via_Arrow_Keys(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, TwoPanes())
            .Add(c => c.KeyboardStep, 25d));

        cut.Find(".bob-splitter__gripper").KeyDown("ArrowRight");

        IReadOnlyList<IElement> panes = cut.FindAll(".bob-splitter__pane");
        panes[0].GetAttribute("style").Should().Contain("225px");
        panes[1].GetAttribute("style").Should().Contain("175px");
    }
}

[Trait("Component Accessibility", "BOBSplitter")]
public class BOBSplitterAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Gripper_With_Separator_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        RenderFragment children = b =>
        {
            b.OpenComponent<BOBSplitterPane>(0);
            b.AddAttribute(1, "Size", 100d);
            b.CloseComponent();
            b.OpenComponent<BOBSplitterPane>(2);
            b.AddAttribute(3, "Flex", true);
            b.CloseComponent();
        };

        IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(p => p
            .Add(c => c.ChildContent, children));

        IElement gripper = cut.Find(".bob-splitter__gripper");
        gripper.GetAttribute("role").Should().Be("separator");
        // Horizontal splitter -> vertical gripper line per ARIA convention.
        gripper.GetAttribute("aria-orientation").Should().Be("vertical");
        gripper.GetAttribute("tabindex").Should().Be("0");
    }
}

[Trait("Component Snapshots", "BOBSplitter")]
public class BOBSplitterSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        RenderFragment twoPanes = b =>
        {
            b.OpenComponent<BOBSplitterPane>(0);
            b.AddAttribute(1, "Size", 220d);
            b.AddAttribute(2, "MinSize", 100d);
            b.CloseComponent();
            b.OpenComponent<BOBSplitterPane>(3);
            b.AddAttribute(4, "Flex", true);
            b.CloseComponent();
        };

        var testCases = new[]
        {
            new
            {
                Name = "Horizontal",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBSplitter>>)(p => p
                    .Add(c => c.ChildContent, twoPanes))
            },
            new
            {
                Name = "Vertical",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBSplitter>>)(p => p
                    .Add(c => c.ChildContent, twoPanes)
                    .Add(c => c.Orientation, BOBSplitterOrientation.Vertical))
            }
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBSplitter> cut = ctx.Render<BOBSplitter>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}