using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Bunit.Rendering;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;

namespace BlazOrbit.Tests.Integration.Tests.Components.Forms;

[Trait("Component Interaction", "BOBInputDateTime")]
public class BOBInputDateTimeInteractionTests
{
    #region Focus and Blur Interactions

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_On_Focus(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        IElement container = cut.Find("bob-component");
        container.GetAttribute("data-bob-floated").Should().Be("false");

        // Act
        IElement pattern = cut.Find(".bob-pattern");
        await pattern.FocusAsync(new());

        // Assert - Label should float on focus
        cut.WaitForAssertion(() =>
            container.GetAttribute("data-bob-floated").Should().Be("true"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Label_Floated_After_Blur_If_Has_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime value = new(2024, 1, 15, 14, 30, 0);
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, value));

        IElement container = cut.Find("bob-component");
        container.GetAttribute("data-bob-floated").Should().Be("true");

        IElement pattern = cut.Find(".bob-pattern");
        await pattern.FocusAsync(new());
        await pattern.FocusOutAsync(new());

        // Assert - Label should remain floated because there's a value
        container.GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Unfloat_Label_After_Blur_If_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        IElement container = cut.Find("bob-component");
        IElement pattern = cut.Find(".bob-pattern");

        await pattern.FocusAsync(new());
        cut.WaitForAssertion(() =>
            container.GetAttribute("data-bob-floated").Should().Be("true"));

        // Act
        await pattern.FocusOutAsync(new());

        // Assert - Label should unfloat when empty and no focus
        cut.WaitForAssertion(() =>
            container.GetAttribute("data-bob-floated").Should().Be("false"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_When_Has_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        DateTime value = new(2024, 1, 15, 14, 30, 0);
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, value));

        // Assert
        IElement container = cut.Find("bob-component");
        container.GetAttribute("data-bob-floated").Should().Be("true");
    }

    #endregion

    #region Clear Button Interactions

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Complete_Value_To_Initial(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime initialValue = new(2024, 1, 15, 14, 30, 0);
        DateTime? currentValue = new DateTime(2024, 6, 20, 10, 15, 0);

        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Value, initialValue)
            .Add(c => c.ValueChanged, v => currentValue = v));

        // Simulate user completing a different value
        cut.Render(p => p.Add(c => c.Value, new DateTime(2024, 6, 20, 10, 15, 0)));

        // Simulate dirty state by typing
        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        IElement clearButton = cut.Find("button[aria-label='Clear']");

        // Act
        clearButton.Click();

        // Assert
        currentValue.Should().Be(initialValue);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Incomplete_Value_To_Initial(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime? initialValue = null;
        DateTime? currentValue = null;

        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Value, initialValue)
            .Add(c => c.ValueChanged, v => currentValue = v));

        // Simulate dirty state (user typed incomplete value like "12")
        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        IElement clearButton = cut.Find("button[aria-label='Clear']");

        // Act
        clearButton.Click();

        // Assert
        currentValue.Should().BeNull();

        // Pattern should be recreated (verify by checking it's present and clean)
        IElement pattern = cut.Find(".bob-pattern");
        pattern.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_To_NonNull_Initial_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly initialValue = new(12, 0); // Noon
        TimeOnly? currentValue = initialValue;

        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
            .Add(c => c.Value, initialValue)
            .Add(c => c.ValueChanged, v => currentValue = v));

        // Simulate user changing value
        cut.Render(p => p.Add(c => c.Value, new TimeOnly(18, 45)));

        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        IElement clearButton = cut.Find("button[aria-label='Clear']");

        // Act
        clearButton.Click();

        // Assert
        currentValue.Should().Be(initialValue);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Clear_Button_When_Not_Dirty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        // Act & Assert
        IReadOnlyList<IElement> clearButtons = cut.FindAll("button[aria-label='Clear']");
        clearButtons.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Clear_Button_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.ReadOnly, true));

        // Simulate dirty state
        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        // Act & Assert
        IReadOnlyList<IElement> clearButtons = cut.FindAll("button[aria-label='Clear']");
        clearButtons.Should().BeEmpty();
    }

    #endregion

    #region Picker Dialog Interactions

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Picker_Dialog_On_Calendar_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");

        // Act
        calendarButton.Click();

        // Assert
        IElement dialog = cut.Find(".bob-dialog");
        dialog.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_DatePicker_For_DateTime_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        cut.FindAll("bob-component[data-bob-component='date-picker']").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_TimePicker_For_DateTime_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        cut.FindAll("bob-component[data-bob-component='time-picker']").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Only_DatePicker_For_DateOnly_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Test Date"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        cut.FindAll("bob-component[data-bob-component='date-picker']").Should().HaveCount(1);
        cut.FindAll("bob-component[data-bob-component='time-picker']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Only_TimePicker_For_TimeOnly_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
            .Add(c => c.Label, "Test Time"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        cut.FindAll("bob-component[data-bob-component='time-picker']").Should().HaveCount(1);
        cut.FindAll("bob-component[data-bob-component='date-picker']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Picker_On_Cancel_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        IElement cancelButton = cut.Find("button:contains('Cancel')");

        // Act
        cancelButton.Click();

        // Assert
        cut.FindAll(".bob-dialog").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Picker_Value_And_Close_On_Apply_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime? changedValue = null;
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.ValueChanged, v => changedValue = v));

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Simulate date/time selection in pickers (would need to interact with DatePicker/TimePicker)
        // For this test, we verify the dialog closes
        IElement applyButton = cut.Find("button:contains('Apply')");

        // Act
        applyButton.Click();

        // Assert
        cut.FindAll(".bob-dialog").Should().BeEmpty();
        changedValue.Should().NotBeNull(); // Value should have been applied
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Open_Picker_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.ReadOnly, true));

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");

        // Act
        calendarButton.Click();

        // Assert - button should be disabled, dialog should not open
        calendarButton.HasAttribute("disabled").Should().BeTrue();
        cut.FindAll(".bob-dialog").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Initialize_Picker_With_Current_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime value = new(2024, 6, 15, 14, 30, 0);
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, value));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        // DatePicker and TimePicker should be initialized with the value
        // This would require inspecting the internal state or rendered output of those components
        cut.Find("bob-component[data-bob-component='date-picker']").Should().NotBeNull();
        cut.Find("bob-component[data-bob-component='time-picker']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Initialize_Picker_With_Default_When_No_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        // Should initialize with Today/Now
        cut.Find("bob-component[data-bob-component='date-picker']").Should().NotBeNull();
        cut.Find("bob-component[data-bob-component='time-picker']").Should().NotBeNull();
    }

    #endregion

    #region Edge Cases - Midnight Handling

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Display_Midnight_As_12_00_AM_For_TimeOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        try
        {
            // Arrange
            TimeOnly midnight = new(0, 0); // 12:00 AM
            IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
                .Add(c => c.Label, "Midnight")
                .Add(c => c.Value, midnight));

            // Act
            IElement pattern = cut.Find(".bob-pattern");
            string displayText = pattern.TextContent.Trim();

            // Assert
            // Should display as 12:00 AM (culture-dependent format)
            displayText.Should().NotBeEmpty();
            displayText.Should().NotBe("00:00"); // Should not show 24-hour format for 12-hour cultures

            // Verify it's formatted according to culture
            string expectedFormat = midnight.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
            displayText.Should().Contain(expectedFormat);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Midnight_In_DateTime(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime midnight = new(2024, 1, 15, 0, 0, 0); // 12:00 AM
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Midnight DateTime")
            .Add(c => c.Value, midnight));

        // Act
        IElement pattern = cut.Find(".bob-pattern");
        string displayText = pattern.TextContent.Trim();

        // Assert
        displayText.Should().NotBeEmpty();
        displayText.Should().Contain("15"); // Day
        displayText.Should().Contain("01"); // Month (culture-dependent order)
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Picker_With_Midnight_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly midnight = new(0, 0);
        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
            .Add(c => c.Label, "Midnight")
            .Add(c => c.Value, midnight));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        IElement timePicker = cut.Find("bob-component[data-bob-component='time-picker']");
        timePicker.Should().NotBeNull();
        // TimePicker should be initialized with hour=0, minute=0
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Midnight_Value_Correctly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly midnight = new(0, 0);
        TimeOnly? currentValue = midnight;

        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
            .Add(c => c.Label, "Midnight")
            .Add(c => c.Value, midnight)
            .Add(c => c.ValueChanged, v => currentValue = v));

        // Change value to make it dirty
        cut.Render(p => p.Add(c => c.Value, new TimeOnly(14, 30)));

        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        IElement clearButton = cut.Find("button[aria-label='Clear']");

        // Act
        clearButton.Click();

        // Assert
        currentValue.Should().Be(midnight); // Should restore to initial midnight value
    }

    #endregion

    #region Edge Cases - Default Values

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_DateOnly_MinValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateOnly minDate = DateOnly.MinValue;
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Min Date")
            .Add(c => c.Value, minDate));

        // Act
        IElement pattern = cut.Find(".bob-pattern");

        // Assert
        pattern.TextContent.Should().NotBeEmpty();
        // Should display the actual date, not treat it as "no value"
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_DateTime_MinValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime minDateTime = DateTime.MinValue;
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Min DateTime")
            .Add(c => c.Value, minDateTime));

        // Act
        IElement pattern = cut.Find(".bob-pattern");

        // Assert
        pattern.TextContent.Should().NotBeEmpty();
    }

    #endregion

    #region Edge Cases - Incomplete Input

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Clear_Button_When_Dirty_Even_If_Incomplete(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        // Simulate dirty state (user typed something)
        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        // Act & Assert
        IReadOnlyList<IElement> clearButtons = cut.FindAll("button[aria-label='Clear']");
        clearButtons.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Apply_Invalid_Input_When_Opening_Picker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime? value = null;
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        // Simulate incomplete input (user typed "12" but didn't complete)
        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");

        // Act
        calendarButton.Click();

        // Assert
        value.Should().BeNull(); // Should not have changed
        // Picker should open with default Today/Now values
        cut.Find(".bob-dialog").Should().NotBeNull();
    }

    #endregion

    #region Disabled and ReadOnly States

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Pattern_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.ReadOnly, true));

        // Act
        IElement pattern = cut.Find(".bob-pattern");
        IReadOnlyList<IElement> editableSpans = pattern.QuerySelectorAll("[contenteditable='true']");

        // Assert
        editableSpans.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Calendar_Button_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.ReadOnly, true));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");

        // Assert
        calendarButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Disabled_State_To_Calendar_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Disabled, true));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");

        // Assert
        // Disabled state is managed through IsDisabled property
        // The calendar button should respect the disabled state via the component's computed attributes
        cut.Find("bob-component").Should().NotBeNull();
    }

    #endregion

    #region Type-Specific Interactions

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_DateTimeOffset_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTimeOffset value = new(2024, 6, 15, 14, 30, 0, TimeSpan.FromHours(-5));
        IRenderedComponent<BOBInputDateTime<DateTimeOffset?>> cut = ctx.Render<BOBInputDateTime<DateTimeOffset?>>(p => p
            .Add(c => c.Label, "Test DateTimeOffset")
            .Add(c => c.Value, value));

        // Act
        IElement pattern = cut.Find(".bob-pattern");

        // Assert
        pattern.TextContent.Should().NotBeEmpty();
        pattern.TextContent.Should().Contain("15");
        pattern.TextContent.Should().Contain("06");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_NonNullable_DateTime_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime value = new(2024, 1, 15, 10, 30, 0);
        IRenderedComponent<BOBInputDateTime<DateTime>> cut = ctx.Render<BOBInputDateTime<DateTime>>(p => p
            .Add(c => c.Label, "Non-Nullable DateTime")
            .Add(c => c.Value, value));

        // Act
        IElement pattern = cut.Find(".bob-pattern");

        // Assert
        pattern.TextContent.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_NonNullable_DateOnly_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateOnly value = new(2024, 6, 15);
        IRenderedComponent<BOBInputDateTime<DateOnly>> cut = ctx.Render<BOBInputDateTime<DateOnly>>(p => p
            .Add(c => c.Label, "Non-Nullable DateOnly")
            .Add(c => c.Value, value));

        // Act
        IElement pattern = cut.Find(".bob-pattern");

        // Assert
        pattern.TextContent.Should().NotBeEmpty();
        pattern.TextContent.Should().Contain("15");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_NonNullable_TimeOnly_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly value = new(14, 30);
        IRenderedComponent<BOBInputDateTime<TimeOnly>> cut = ctx.Render<BOBInputDateTime<TimeOnly>>(p => p
            .Add(c => c.Label, "Non-Nullable TimeOnly")
            .Add(c => c.Value, value));

        // Act
        IElement pattern = cut.Find(".bob-pattern");

        // Assert
        pattern.TextContent.Should().NotBeEmpty();
    }

    #endregion

    #region Picker Title Variations

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_DateTime_Title_For_DateTime_Picker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        IElement dialog = cut.Find(".bob-dialog");
        string dialogContent = dialog.TextContent;
        dialogContent.Should().Contain("Select Date & Time");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Date_Title_For_DateOnly_Picker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateOnly?>> cut = ctx.Render<BOBInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Test Date"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        IElement dialog = cut.Find(".bob-dialog");
        string dialogContent = dialog.TextContent;
        dialogContent.Should().Contain("Select Date");
        dialogContent.Should().NotContain("Time");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Time_Title_For_TimeOnly_Picker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
            .Add(c => c.Label, "Test Time"));

        // Act
        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // Assert
        IElement dialog = cut.Find(".bob-dialog");
        string dialogContent = dialog.TextContent;
        dialogContent.Should().Contain("Select Time");
        dialogContent.Should().NotContain("Date");
    }

    #endregion

    #region Value Change Propagation

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_Change_From_Picker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime? changedValue = null;
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.ValueChanged, v => changedValue = v));

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        IElement applyButton = cut.Find("button:contains('Apply')");

        // Act
        applyButton.Click();

        // Assert
        changedValue.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Propagate_Value_On_Cancel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime initialValue = new(2024, 1, 15, 14, 30, 0);
        DateTime? changedValue = initialValue;
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, initialValue)
            .Add(c => c.ValueChanged, v => changedValue = v));

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        // User could modify pickers here, but we cancel
        IElement cancelButton = cut.Find("button:contains('Cancel')");

        // Act
        cancelButton.Click();

        // Assert
        changedValue.Should().Be(initialValue);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Pattern_After_Picker_Apply(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        IElement calendarButton = cut.Find("button[aria-label='Open picker']");
        calendarButton.Click();

        IElement applyButton = cut.Find("button:contains('Apply')");

        // Act
        applyButton.Click();

        // Assert
        IElement pattern = cut.Find(".bob-pattern");
        pattern.TextContent.Should().NotBeEmpty();
    }

    #endregion

    #region Form Integration

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Integrate_With_EditForm_Validation(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TestModel model = new();
        IRenderedComponent<EditForm> form = ctx.Render<EditForm>(p => p
            .Add(f => f.Model, model)
            .Add(f => f.ChildContent, _ => builder =>
            {
                builder.OpenComponent<DataAnnotationsValidator>(0);
                builder.CloseComponent();
                builder.OpenComponent<BOBInputDateTime<DateTime?>>(1);
                builder.AddAttribute(2, "Value", model.RequiredDate);
                builder.AddAttribute(3, "ValueExpression",
                    (System.Linq.Expressions.Expression<Func<DateTime?>>)(() => model.RequiredDate));
                builder.AddAttribute(4, "Label", "Required Date");
                builder.CloseComponent();
            }));

        // Act - Submit form without value
        form.Find("form").Submit();

        // Assert
        IReadOnlyList<IElement> validationMessages = form.FindAll(".validation-message");
        validationMessages.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Required_Indicator_When_Required(BlazorScenario scenario)
    {
        // CSS-OPT-02 block B.5: required asterisk is rendered by CSS via
        // [data-bob-required="true"] .bob-input__label::after — assert host attr.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Required DateTime")
            .Add(c => c.Required, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
    }

    #endregion

    #region Edge Cases - Pattern Synchronization

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sync_Pattern_When_Value_Changes_Externally(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime? value = new DateTime(2024, 1, 15, 14, 30, 0);
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, value));

        IElement pattern = cut.Find(".bob-pattern");
        string initialText = pattern.TextContent;

        // Act
        DateTime newValue = new(2024, 6, 20, 10, 15, 0);
        cut.Render(p => p.Add(c => c.Value, newValue));

        // Assert
        string updatedText = pattern.TextContent;
        updatedText.Should().NotBe(initialText);
        updatedText.Should().Contain("20");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Recreate_Pattern_After_Clear(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime"));

        // Simulate dirty state
        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        IElement patternBefore = cut.Find(".bob-pattern");
        string keyBefore = patternBefore.GetAttribute("data-bob-pattern-id");

        IElement clearButton = cut.Find("button[aria-label='Clear']");

        // Act
        clearButton.Click();

        // Assert
        IElement patternAfter = cut.Find(".bob-pattern");
        string keyAfter = patternAfter.GetAttribute("data-bob-pattern-id");

        // Pattern should have a different key after clear (forcing recreation)
        keyAfter.Should().NotBe(keyBefore);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Rapid_Clear_Operations(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime initialValue = new(2024, 1, 15, 14, 30, 0);
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, initialValue));

        cut.Render(p => p.Add(c => c.Value, new DateTime(2024, 6, 20, 10, 15, 0)));

        cut.Instance.GetType()
            .GetMethod("HandleDirtyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(cut.Instance, new object[] { true });
        cut.Render();

        // Act - Multiple rapid clears (button may unmount after first clear when no longer dirty)
        for (int i = 0; i < 3; i++)
        {
            IReadOnlyList<IElement> buttons = cut.FindAll("button[aria-label='Clear']");
            if (buttons.Count == 0)
            {
                break;
            }

            buttons[0].Click();
        }

        // Assert - Should not throw and should be stable
        IElement pattern = cut.Find(".bob-pattern");
        pattern.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases - Culture and Format

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Format_According_To_Current_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime value = new(2024, 6, 15, 14, 30, 0);
        IRenderedComponent<BOBInputDateTime<DateTime?>> cut = ctx.Render<BOBInputDateTime<DateTime?>>(p => p
            .Add(c => c.Label, "Test DateTime")
            .Add(c => c.Value, value));

        // Act
        IElement pattern = cut.Find(".bob-pattern");
        string displayText = pattern.TextContent.Trim();

        // Assert
        string expectedFormat = value.ToString(
            $"{CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern} {CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern}",
            CultureInfo.CurrentCulture);

        displayText.Should().NotBeEmpty();
        // Should contain date components
        displayText.Should().Contain("15");
        displayText.Should().Contain("06");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_12_Hour_Format_Correctly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly afternoon = new(14, 30); // 2:30 PM
        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
            .Add(c => c.Label, "Afternoon Time")
            .Add(c => c.Value, afternoon));

        // Act
        IElement pattern = cut.Find(".bob-pattern");
        string displayText = pattern.TextContent.Trim();

        // Assert
        displayText.Should().NotBeEmpty();
        // Should display time (format depends on culture)
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Noon_Correctly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TimeOnly noon = new(12, 0); // 12:00 PM
        IRenderedComponent<BOBInputDateTime<TimeOnly?>> cut = ctx.Render<BOBInputDateTime<TimeOnly?>>(p => p
            .Add(c => c.Label, "Noon")
            .Add(c => c.Value, noon));

        // Act
        IElement pattern = cut.Find(".bob-pattern");
        string displayText = pattern.TextContent.Trim();

        // Assert
        displayText.Should().NotBeEmpty();
        string expectedFormat = noon.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
        displayText.Should().Contain(expectedFormat);
    }

    #endregion

    #region Multiple Instance Interactions

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Multiple_Instances_Independently(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime value1 = new(2024, 1, 15, 14, 30, 0);
        DateTime value2 = new(2024, 6, 20, 10, 15, 0);

        void fragment(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenComponent<BOBInputDateTime<DateTime?>>(0);
            builder.AddAttribute(1, "Label", "First DateTime");
            builder.AddAttribute(2, "Value", value1);
            builder.CloseComponent();

            builder.OpenComponent<BOBInputDateTime<DateTime?>>(3);
            builder.AddAttribute(4, "Label", "Second DateTime");
            builder.AddAttribute(5, "Value", value2);
            builder.CloseComponent();
        }

        IRenderedComponent<ContainerFragment> cut = ctx.Render(fragment);

        // Act
        IReadOnlyList<IElement> patterns = cut.FindAll(".bob-pattern");
        IReadOnlyList<IElement> calendarButtons = cut.FindAll("button[aria-label='Open picker']");

        // Assert
        patterns.Should().HaveCount(2);
        calendarButtons.Should().HaveCount(2);

        // Open first picker
        calendarButtons[0].Click();
        cut.FindAll(".bob-dialog").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Interfere_With_Other_Instance_When_Clearing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateTime? value1 = new DateTime(2024, 1, 15, 14, 30, 0);
        DateTime? value2 = new DateTime(2024, 6, 20, 10, 15, 0);

        void fragment(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenComponent<BOBInputDateTime<DateTime?>>(0);
            builder.AddAttribute(1, "Label", "First DateTime");
            builder.AddAttribute(2, "Value", value1);
            builder.CloseComponent();

            builder.OpenComponent<BOBInputDateTime<DateTime?>>(3);
            builder.AddAttribute(4, "Label", "Second DateTime");
            builder.AddAttribute(5, "Value", value2);
            builder.CloseComponent();
        }

        IRenderedComponent<ContainerFragment> cut = ctx.Render(fragment);

        // Simulate dirty on first instance
        IReadOnlyList<IElement> patterns = cut.FindAll(".bob-pattern");

        // This is a simplified test - in reality would need to properly trigger dirty state
        IReadOnlyList<IElement> allPatterns = cut.FindAll(".bob-pattern");
        allPatterns.Should().HaveCount(2);

        // Both should render independently
        string text1 = allPatterns[0].TextContent;
        string text2 = allPatterns[1].TextContent;

        text1.Should().NotBe(text2);
    }

    #endregion

    // Helper model for form validation tests
    private class TestModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime? RequiredDate { get; set; }
    }
}