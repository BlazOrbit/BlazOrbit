using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Disposal", "BOBTabs")]
public class BOBTabsDisposalTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        static void twoTabs(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder b)
        {
            b.OpenComponent<BOBTab>(0);
            b.AddAttribute(1, "Id", "t1");
            b.AddAttribute(2, "Label", "T1");
            b.CloseComponent();
            b.OpenComponent<BOBTab>(3);
            b.AddAttribute(4, "Id", "t2");
            b.AddAttribute(5, "Label", "T2");
            b.CloseComponent();
        }

        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, twoTabs));

        cut.FindAll("[role='tab']").Should().HaveCount(2);

        // Act + Assert — dispose does not throw
        Func<Task> act = async () => await cut.Instance.DisposeAsync();
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Tab_When_Unregistered(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — start with two tabs
        bool showSecond = true;
        IRenderedComponent<BOBTabs> cut = null!;
        cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BOBTab>(0);
                b.AddAttribute(1, "Id", "t1");
                b.AddAttribute(2, "Label", "T1");
                b.CloseComponent();
                if (showSecond)
                {
                    b.OpenComponent<BOBTab>(3);
                    b.AddAttribute(4, "Id", "t2");
                    b.AddAttribute(5, "Label", "T2");
                    b.CloseComponent();
                }
            }));

        cut.FindAll("[role='tab']").Should().HaveCount(2);

        // Act — remove second tab
        showSecond = false;
        cut.Render(p => p.Add(c => c.ChildContent, b =>
        {
            b.OpenComponent<BOBTab>(0);
            b.AddAttribute(1, "Id", "t1");
            b.AddAttribute(2, "Label", "T1");
            b.CloseComponent();
        }));

        // Assert
        cut.FindAll("[role='tab']").Should().HaveCount(1);
    }
}
