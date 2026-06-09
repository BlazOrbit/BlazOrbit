using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Variant", "BOBTreeMenu.SplitAffordance")]
public class BOBTreeMenuSplitAffordanceTests
{
    private static RenderFragment ContainerWithChildrenMarkup => b =>
    {
        // "Components" - container with both Href + child sub-pages.
        b.OpenComponent<BOBTreeMenuItem>(0);
        b.AddAttribute(1, "Text", "Components");
        b.AddAttribute(2, "Href", "/components");
        b.AddAttribute(3, "Key", "components");
        b.AddAttribute(4, "ChildContent", (RenderFragment)(b2 =>
        {
            b2.OpenComponent<BOBTreeMenuItem>(0);
            b2.AddAttribute(1, "Text", "Forms");
            b2.AddAttribute(2, "Href", "/components/forms");
            b2.AddAttribute(3, "Key", "components-forms");
            b2.CloseComponent();
        }));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Container_With_Href_Should_Render_Anchor_And_Toggle_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, ContainerWithChildrenMarkup));

        // Split-row markup: a single .bob-tree-menu__row that contains both
        // a real <a> and a <button>. Ensures right-click "Open in new tab"
        // works on the label.
        cut.FindAll(".bob-tree-menu__row").Should().HaveCount(1);
        AngleSharp.Dom.IElement anchor = cut.Find(".bob-tree-menu__row a.bob-tree-menu__link");
        anchor.GetAttribute("href").Should().Be("/components");
        anchor.GetAttribute("role").Should().Be("menuitem");

        AngleSharp.Dom.IElement toggle = cut.Find(".bob-tree-menu__toggle-btn");
        toggle.GetAttribute("aria-haspopup").Should().Be("true");
        toggle.GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Toggle_Button_Click_Should_Expand_Without_Navigating(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        bool navigated = false;

        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, ContainerWithChildrenMarkup)
            .Add(c => c.OnNavigate, _ =>
            {
                navigated = true;
                return Task.CompletedTask;
            }));

        // Click the chevron toggle.
        cut.Find(".bob-tree-menu__toggle-btn").Click();

        cut.Find(".bob-tree-menu__toggle-btn").GetAttribute("aria-expanded")
            .Should().Be("true", "the toggle button must expand the subtree");
        navigated.Should().BeFalse("the chevron must not trigger OnNavigate");
        cut.FindAll(".bob-tree-menu__submenu--inline").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Link_Click_Should_Auto_Expand_Subtree(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        string? navigatedTo = null;

        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, ContainerWithChildrenMarkup)
            .Add(c => c.OnNavigate, href =>
            {
                navigatedTo = href;
                return Task.CompletedTask;
            }));

        // Click the label anchor.
        cut.Find(".bob-tree-menu__row a.bob-tree-menu__link").Click();

        navigatedTo.Should().Be("/components",
            "the OnNavigate callback fires with the link's href");
        cut.Find(".bob-tree-menu__toggle-btn").GetAttribute("aria-expanded")
            .Should().Be("true",
                "clicking the link auto-expands the subtree so siblings appear");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Leaf_With_Href_Should_Stay_Single_Anchor(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BOBTreeMenuItem>(0);
                b.AddAttribute(1, "Text", "Privacy");
                b.AddAttribute(2, "Href", "/privacy");
                b.AddAttribute(3, "Key", "privacy");
                b.CloseComponent();
            }));

        // Leaf (no children) must NOT render the split-row markup.
        cut.FindAll(".bob-tree-menu__row").Should().BeEmpty();
        cut.FindAll(".bob-tree-menu__toggle-btn").Should().BeEmpty();
        cut.Find("a.bob-tree-menu__link").GetAttribute("href").Should().Be("/privacy");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Container_Without_Href_Should_Stay_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTreeMenu<object>> cut = ctx.Render<BOBTreeMenu<object>>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BOBTreeMenuItem>(0);
                b.AddAttribute(1, "Text", "Settings");
                b.AddAttribute(2, "Key", "settings");
                b.AddAttribute(3, "ChildContent", (RenderFragment)(b2 =>
                {
                    b2.OpenComponent<BOBTreeMenuItem>(0);
                    b2.AddAttribute(1, "Text", "Profile");
                    b2.AddAttribute(2, "Href", "/settings/profile");
                    b2.AddAttribute(3, "Key", "settings-profile");
                    b2.CloseComponent();
                }));
                b.CloseComponent();
            }));

        // Pure container (no Href on parent) → still uses the original
        // single-button layout, no split row.
        cut.FindAll(".bob-tree-menu__row").Should().BeEmpty();
        AngleSharp.Dom.IElement btn = cut.Find("button.bob-tree-menu__button");
        btn.GetAttribute("aria-haspopup").Should().Be("true");
    }
}
