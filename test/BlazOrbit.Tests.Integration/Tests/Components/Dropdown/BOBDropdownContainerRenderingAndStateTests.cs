using System.Linq.Expressions;
using BlazOrbit.Components.Forms;
using BlazOrbit.Components.Forms.Dropdown;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Rendering", "BOBDropdownContainer")]
public class BOBDropdownContainerRenderingTests
{
    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Trigger_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.DisplayValue, "Selected"));

        cut.Find("button.bob-dropdown__trigger").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Display_Value_In_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.DisplayValue, "Option A")
            .Add(c => c.HasValue, true));

        cut.Find(".bob-dropdown__value").TextContent.Trim().Should().Be("Option A");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Placeholder_When_No_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Placeholder, "Choose...")
            .Add(c => c.HasValue, false));

        cut.Find(".bob-dropdown__placeholder").TextContent.Trim().Should().Be("Choose...");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Menu_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.MenuContent, b => b.AddContent(0, "Menu items")));

        await cut.InvokeAsync(() => cut.Instance.OpenAsync());

        cut.Find(".bob-dropdown__menu").TextContent.Should().Be("Menu items");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Menu_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.MenuContent, b => b.AddContent(0, "Menu items")));

        cut.FindAll(".bob-dropdown__menu").Should().BeEmpty();
    }
}

[Trait("Component State", "BOBDropdownContainer")]
public class BOBDropdownContainerStateTests
{
    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Open_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        await cut.InvokeAsync(() => cut.Instance.OpenAsync());

        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_On_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Disabled, true));

        cut.Find("button.bob-dropdown__trigger").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Loading, true));

        cut.Find("bob-component").GetAttribute("data-bob-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Placement_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Placement, DropdownPlacement.Top));

        cut.Find("bob-component").GetAttribute("data-bob-placement").Should().Be("top");
    }
}

[Trait("Component Accessibility", "BOBDropdownContainer")]
public class BOBDropdownContainerAccessibilityTests
{
    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Expanded_On_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        await cut.InvokeAsync(() => cut.Instance.OpenAsync());

        cut.Find("button.bob-dropdown__trigger").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Haspopup_On_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.AriaRole, "listbox"));

        cut.Find("button.bob-dropdown__trigger").GetAttribute("aria-haspopup").Should().Be("listbox");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Menu_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.AriaRole, "tree"));

        await cut.InvokeAsync(() => cut.Instance.OpenAsync());

        cut.Find(".bob-dropdown__menu").GetAttribute("role").Should().Be("tree");
    }
}

[Trait("Component Interaction", "BOBDropdownContainer")]
public class BOBDropdownContainerInteractionTests
{
    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Disabled, true));

        cut.Find("button.bob-dropdown__trigger").Click();

        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_When_Loading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDropdownContainer<string>> cut = ctx.Render<BOBDropdownContainer<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Loading, true));

        cut.Find("button.bob-dropdown__trigger").Click();

        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().BeNull();
    }
}
