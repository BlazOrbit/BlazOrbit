using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Interaction", "BOBInputDropdown")]
public class BOBInputDropdownInteractionTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    private static Action<ComponentParameterCollectionBuilder<BOBInputDropdown<string>>> WithOptions(string? value = null)
        => p =>
        {
            p.Add(c => c.ValueExpression, _expr);
            if (value != null)
            {
                p.Add(c => c.Value, value);
            }

            p.Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
                builder.OpenComponent<DropdownOption<string>>(3);
                builder.AddAttribute(4, "Value", "opt2");
                builder.AddAttribute(5, "Text", "Option 2");
                builder.CloseComponent();
            });
        };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Menu_On_Trigger_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert
        cut.Find(".bob-dropdown__menu").Should().NotBeNull();
        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Menu_On_Second_Trigger_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bob-dropdown__trigger").Click(); // open
        cut.Find("button.bob-dropdown__trigger").Click(); // close

        // Assert
        cut.FindAll(".bob-dropdown__menu").Should().BeEmpty();
        cut.Find("bob-component").GetAttribute("data-bob-dropdown-open").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Options_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert
        IReadOnlyList<IElement> options = cut.FindAll(".bob-dropdown__option");
        options.Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Option_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? captured = null;
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p =>
        {
            WithOptions()(p);
            p.Add(c => c.ValueChanged, v => captured = v);
        });

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();
        cut.Find(".bob-dropdown__option").Click();

        // Assert
        captured.Should().Be("opt1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Menu_After_Selection_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();
        cut.Find(".bob-dropdown__option").Click();

        // Assert — CloseOnSelect=true by default, menu closes
        cut.FindAll(".bob-dropdown__menu").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Open_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p =>
        {
            WithOptions()(p);
            p.Add(c => c.Disabled, true);
        });

        // Act — trigger is disabled, click won't fire
        cut.FindAll(".bob-dropdown__menu").Should().BeEmpty();
        cut.Find("button.bob-dropdown__trigger").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Aria_Expanded_On_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(WithOptions());

        // Assert initial
        cut.Find("button.bob-dropdown__trigger").GetAttribute("aria-expanded").Should().Be("false");

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert after open
        cut.Find("button.bob-dropdown__trigger").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Selected_Option_As_Selected(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(WithOptions("opt1"));

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert — selected option has aria-selected=true
        IElement selectedOption = cut.FindAll(".bob-dropdown__option")
            .First(o => o.TextContent.Contains("Option 1"));
        selectedOption.GetAttribute("aria-selected").Should().Be("true");
    }
}
