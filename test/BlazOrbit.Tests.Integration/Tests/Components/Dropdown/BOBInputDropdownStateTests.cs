using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component State", "BOBInputDropdown")]
public class BOBInputDropdownStateTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Readonly_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ReadOnly, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Loading_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.IsLoading, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Dropdown_Closed_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_When_Value_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Label, "Select")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert initial — not floated
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("false");

        // Act — set value
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Value, "opt1")
            .Add(c => c.Label, "Select")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert floated
        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Selected_Option_Text_In_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Value, "opt1")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert
        cut.Find(".bob-dropdown__value").TextContent.Trim().Should().Contain("Option 1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Trigger_Button_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("button.bob-dropdown__trigger").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Open_Attribute_When_Opened(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert initial
        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().Be("false");

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert opened
        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().Be("true");
    }
}
