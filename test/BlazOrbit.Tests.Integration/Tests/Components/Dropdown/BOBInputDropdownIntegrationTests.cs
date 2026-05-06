using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Integration", "BOBInputDropdown")]
public class BOBInputDropdownIntegrationTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    private sealed class RecordingDropdownJsInterop : IDropdownJsInterop
    {
        public List<string> Calls { get; } = [];

        public ValueTask InitializeAsync(
            ElementReference triggerElement,
            ElementReference menuElement,
            DotNetObjectReference<DropdownCallbacksRelay> dotnetReference,
            string componentId)
        {
            Calls.Add($"initialize:{componentId}");
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync(string componentId)
        {
            Calls.Add($"dispose:{componentId}");
            return ValueTask.CompletedTask;
        }

        public ValueTask FocusSearchInputAsync(string componentId) => ValueTask.CompletedTask;

        public ValueTask<DropdownPosition> GetPositionAsync(string componentId)
            => ValueTask.FromResult<DropdownPosition>(default);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_JsInterop_Once_When_Unmounted(BlazorScenario scenario)
    {
        // COMP-INPUTDROPDOWN-01: BOBInputDropdown.DisposeAsync used to call
        // _container.DisposeAsync() manually on top of Blazor's automatic
        // child disposal, firing JsInterop.DisposeAsync(componentId) twice.
        BlazorTestContextBase ctx = scenario.CreateContext();
        RecordingDropdownJsInterop interop = new();
        ctx.Services.AddScoped<IDropdownJsInterop>(_ => interop);

        // Arrange — render and open the dropdown so the JS side is initialized
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));
        cut.Find("button.bob-dropdown__trigger").Click();
        interop.Calls.Should().ContainSingle(c => c.StartsWith("initialize:"),
            "opening the dropdown initializes the JS side once");

        // Act — disposing the bUnit context unmounts the host and its children
        await ctx.DisposeAsync();

        // Assert — exactly one dispose JS call (Blazor's automatic child disposal),
        // not two (which would also include the now-removed manual call from
        // BOBInputDropdown.DisposeAsync).
        interop.Calls.Count(c => c.StartsWith("dispose:")).Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_And_Display_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? selected = null;
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Label, "Choose")
            .Add(c => c.ValueChanged, v => selected = v)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "apple");
                builder.AddAttribute(2, "Text", "Apple");
                builder.CloseComponent();
                builder.OpenComponent<DropdownOption<string>>(3);
                builder.AddAttribute(4, "Value", "banana");
                builder.AddAttribute(5, "Text", "Banana");
                builder.CloseComponent();
            }));

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();
        IElement appleOption = cut.FindAll(".bob-dropdown__option")
            .First(o => o.TextContent.Contains("Apple"));
        appleOption.Click();

        // Assert
        selected.Should().Be("apple");
        cut.FindAll(".bob-dropdown__menu").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Selected_Value_As_Display_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Value, "banana")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "apple");
                builder.AddAttribute(2, "Text", "Apple");
                builder.CloseComponent();
                builder.OpenComponent<DropdownOption<string>>(3);
                builder.AddAttribute(4, "Value", "banana");
                builder.AddAttribute(5, "Text", "Banana");
                builder.CloseComponent();
            }));

        // Assert — display value shows selected option label
        cut.Find(".bob-dropdown__value").TextContent.Trim().Should().Be("Banana");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Menu_Open_When_CloseOnSelect_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.CloseOnSelect, false)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();
        cut.Find(".bob-dropdown__option").Click();

        // Assert — menu stays open when CloseOnSelect=false
        cut.Find(".bob-dropdown__menu").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Select_Disabled_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? selected = null;
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ValueChanged, v => selected = v)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Disabled Opt");
                builder.AddAttribute(3, "Disabled", true);
                builder.CloseComponent();
            }));

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();
        cut.Find("[data-bob-disabled=\"true\"]").Click();

        // Assert — value stays null, not changed
        selected.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_No_Options_Message_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDropdown<string>> cut = ctx.Render<BOBInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Find("button.bob-dropdown__trigger").Click();

        // Assert
        cut.Find(".bob-dropdown__no-options").TextContent.Should().Contain("No options available");
    }
}
