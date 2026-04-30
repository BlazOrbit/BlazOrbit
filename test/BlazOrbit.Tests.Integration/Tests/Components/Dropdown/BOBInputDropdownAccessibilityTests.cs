using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Accessibility", "BOBInputDropdown")]
public class BOBInputDropdownAccessibilityTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Haspopup_On_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert — aria-haspopup indicates the type of popup (listbox)
        cut.Find("button.bob-dropdown__trigger").GetAttribute("aria-haspopup").Should().Be("listbox");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Expanded_False_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("button.bob-dropdown__trigger").GetAttribute("aria-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Expanded_True_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "v");
                builder.CloseComponent();
            }));

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert
        cut.Find("button.bob-dropdown__trigger").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Label_Linked_Via_For_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Label, "Select"));

        // Assert — label's for matches the trigger button's id
        IElement label = cut.Find("label");
        string? labelFor = label.GetAttribute("for");
        labelFor.Should().StartWith("bob-dropdown-");

        // The trigger button should have that id
        cut.Find($"button#{labelFor}").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Required_Data_Attribute_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Required, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Options_With_Role_Option_When_Open(BlazorScenario scenario)
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

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert
        cut.Find("[role='option']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Required_Asterisk_When_Required_And_Labeled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Label, "Select")
            .Add(c => c.Required, true));

        // Assert — CSS-OPT-02 block B.5: visual marker is now CSS-only via
        // [data-bob-required="true"] .bob-input__label::after on the host.
        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }
}
