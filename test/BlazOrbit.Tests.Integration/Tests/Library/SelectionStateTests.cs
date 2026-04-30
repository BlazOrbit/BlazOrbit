using BlazOrbit.Abstractions;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Direct unit tests for <see cref="SelectionState{TValue}" /> and
/// <see cref="SelectionTypeInfo" />.
/// Pins single/multi transitions, type detection for T / T[] / List&lt;T&gt; /
/// HashSet&lt;T&gt;, and StateChanged notification.
/// </summary>
[Trait("Core", "SelectionState")]
public class SelectionStateTests
{
    // ─────────── SelectionTypeInfo ───────────

    [Fact]
    public void TypeInfo_Should_Detect_Single_For_Value_Type()
    {
        SelectionTypeInfo info = new(typeof(int));

        info.IsMultiple.Should().BeFalse();
        info.ElementType.Should().Be(typeof(int));
        info.ValueType.Should().Be(typeof(int));
    }

    [Fact]
    public void TypeInfo_Should_Detect_Single_For_String()
    {
        SelectionTypeInfo info = new(typeof(string));

        info.IsMultiple.Should().BeFalse();
        info.ElementType.Should().Be(typeof(string));
    }

    [Fact]
    public void TypeInfo_Should_Detect_Multiple_For_Array()
    {
        SelectionTypeInfo info = new(typeof(int[]));

        info.IsMultiple.Should().BeTrue();
        info.ElementType.Should().Be(typeof(int));
        info.ValueType.Should().Be(typeof(int[]));
    }

    [Fact]
    public void TypeInfo_Should_Detect_Multiple_For_List()
    {
        SelectionTypeInfo info = new(typeof(List<string>));

        info.IsMultiple.Should().BeTrue();
        info.ElementType.Should().Be(typeof(string));
    }

    [Fact]
    public void TypeInfo_Should_Detect_Multiple_For_HashSet()
    {
        SelectionTypeInfo info = new(typeof(HashSet<int>));

        info.IsMultiple.Should().BeTrue();
        info.ElementType.Should().Be(typeof(int));
    }

    [Fact]
    public void TypeInfo_Should_Detect_Multiple_For_IReadOnlyList()
    {
        SelectionTypeInfo info = new(typeof(IReadOnlyList<int>));

        info.IsMultiple.Should().BeTrue();
        info.ElementType.Should().Be(typeof(int));
    }

    [Fact]
    public void TypeInfo_CreateValue_Single_Should_Return_First_Item()
    {
        SelectionTypeInfo info = new(typeof(int));

        int result = info.CreateValue<int>(new object[] { 42, 7 });

        result.Should().Be(42);
    }

    [Fact]
    public void TypeInfo_CreateValue_Single_Empty_Should_Return_Default()
    {
        SelectionTypeInfo info = new(typeof(string));

        string? result = info.CreateValue<string?>(Array.Empty<object>());

        result.Should().BeNull();
    }

    [Fact]
    public void TypeInfo_CreateValue_Array_Should_Build_Array_Of_ElementType()
    {
        SelectionTypeInfo info = new(typeof(int[]));

        int[] result = info.CreateValue<int[]>(new object[] { 1, 2, 3 });

        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void TypeInfo_CreateValue_List_Should_Build_List()
    {
        SelectionTypeInfo info = new(typeof(List<string>));

        List<string> result = info.CreateValue<List<string>>(new object[] { "a", "b" });

        result.Should().Equal("a", "b");
    }

    [Fact]
    public void TypeInfo_CreateValue_HashSet_Should_Build_Set()
    {
        SelectionTypeInfo info = new(typeof(HashSet<int>));

        HashSet<int> result = info.CreateValue<HashSet<int>>(new object[] { 1, 1, 2 });

        result.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void TypeInfo_ExtractValues_Should_Yield_Single_Wrapper_For_Non_Enumerable()
    {
        SelectionTypeInfo info = new(typeof(int));

        object[] extracted = info.ExtractValues(42).ToArray();

        extracted.Should().Equal(42);
    }

    [Fact]
    public void TypeInfo_ExtractValues_Should_Unwrap_Enumerable_For_Multiple()
    {
        SelectionTypeInfo info = new(typeof(int[]));

        object[] extracted = info.ExtractValues(new[] { 1, 2, 3 }).ToArray();

        extracted.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void TypeInfo_ExtractValues_Null_Should_Return_Empty()
    {
        SelectionTypeInfo info = new(typeof(int[]));

        info.ExtractValues(null).Should().BeEmpty();
    }

    [Fact]
    public void TypeInfo_ExtractValues_String_Is_Not_Treated_As_Enumerable()
    {
        SelectionTypeInfo info = new(typeof(string));

        object[] extracted = info.ExtractValues("abc").ToArray();

        extracted.Should().Equal("abc");
    }

    [Fact]
    public void TypeInfo_ContainsValue_Single_Uses_Equality()
    {
        SelectionTypeInfo info = new(typeof(int));

        info.ContainsValue(7, 7).Should().BeTrue();
        info.ContainsValue(7, 8).Should().BeFalse();
    }

    [Fact]
    public void TypeInfo_ContainsValue_Multiple_Checks_Membership()
    {
        SelectionTypeInfo info = new(typeof(int[]));

        info.ContainsValue(new[] { 1, 2, 3 }, 2).Should().BeTrue();
        info.ContainsValue(new[] { 1, 2, 3 }, 9).Should().BeFalse();
    }

    [Fact]
    public void TypeInfo_ContainsValue_Null_Inputs_Return_False()
    {
        SelectionTypeInfo info = new(typeof(int[]));

        info.ContainsValue(null, 1).Should().BeFalse();
        info.ContainsValue(new[] { 1 }, null).Should().BeFalse();
    }

    // ─────────── SelectionState (single) ───────────

    [Fact]
    public void Single_Select_Should_Replace_Previous_Value()
    {
        SelectionState<int> state = new();

        state.Select(1);
        state.Select(2);

        state.Count.Should().Be(1);
        state.IsSelected(2).Should().BeTrue();
        state.IsSelected(1).Should().BeFalse();
    }

    [Fact]
    public void Single_Deselect_Should_Empty_State()
    {
        SelectionState<int> state = new();

        state.Select(1);
        state.Deselect(1);

        state.Count.Should().Be(0);
        state.IsSelected(1).Should().BeFalse();
    }

    [Fact]
    public void Single_Toggle_Should_Alternate_Selection()
    {
        SelectionState<int> state = new();

        state.Toggle(1);
        state.IsSelected(1).Should().BeTrue();

        state.Toggle(1);
        state.IsSelected(1).Should().BeFalse();
    }

    [Fact]
    public void Single_Select_Null_Should_Be_NoOp()
    {
        SelectionState<string> state = new();
        state.Select("a");

        state.Select(null);

        state.IsSelected("a").Should().BeTrue();
        state.Count.Should().Be(1);
    }

    [Fact]
    public void Single_GetValue_Returns_First_Selected()
    {
        SelectionState<int> state = new();
        state.Select(42);

        state.GetValue().Should().Be(42);
    }

    [Fact]
    public void Single_SetValue_Replaces_Current_Selection()
    {
        SelectionState<int> state = new();
        state.Select(1);

        state.SetValue(99);

        state.IsSelected(99).Should().BeTrue();
        state.IsSelected(1).Should().BeFalse();
    }

    // ─────────── SelectionState (multiple) ───────────

    [Fact]
    public void Multiple_Select_Should_Accumulate_Values()
    {
        SelectionState<int[]> state = new();

        state.Select(1);
        state.Select(2);
        state.Select(3);

        state.Count.Should().Be(3);
        state.IsMultiple.Should().BeTrue();
    }

    [Fact]
    public void Multiple_Toggle_Should_Add_And_Remove()
    {
        SelectionState<int[]> state = new();

        state.Toggle(1);
        state.Toggle(2);
        state.Toggle(1);

        state.IsSelected(1).Should().BeFalse();
        state.IsSelected(2).Should().BeTrue();
        state.Count.Should().Be(1);
    }

    [Fact]
    public void Multiple_SelectAll_Should_Add_All_Non_Null()
    {
        SelectionState<int[]> state = new();

        state.SelectAll(new object?[] { 1, 2, null, 3 });

        state.Count.Should().Be(3);
    }

    [Fact]
    public void Multiple_SelectAll_NoOp_On_Single_Mode()
    {
        SelectionState<int> state = new();

        state.SelectAll(new object?[] { 1, 2 });

        state.Count.Should().Be(0);
    }

    [Fact]
    public void Multiple_Clear_Should_Reset_State()
    {
        SelectionState<int[]> state = new();
        state.Select(1);
        state.Select(2);

        state.Clear();

        state.Count.Should().Be(0);
    }

    [Fact]
    public void Multiple_GetValue_Should_Return_Array_Of_Selected()
    {
        SelectionState<int[]> state = new();
        state.Select(1);
        state.Select(2);

        int[] result = state.GetValue();

        result.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void Multiple_SetValue_Should_Replace_All()
    {
        SelectionState<int[]> state = new();
        state.Select(99);

        state.SetValue(new[] { 1, 2, 3 });

        state.IsSelected(99).Should().BeFalse();
        state.Count.Should().Be(3);
    }

    [Fact]
    public void SetSingleValue_Should_Clear_Then_Set()
    {
        SelectionState<int[]> state = new();
        state.Select(1);
        state.Select(2);

        state.SetSingleValue(99);

        state.Count.Should().Be(1);
        state.IsSelected(99).Should().BeTrue();
    }

    [Fact]
    public void SetSingleValue_Null_Should_Clear()
    {
        SelectionState<int[]> state = new();
        state.Select(1);

        state.SetSingleValue(null);

        state.Count.Should().Be(0);
    }

    // ─────────── StateChanged notification ───────────

    [Fact]
    public void StateChanged_Should_Fire_On_Select()
    {
        SelectionState<int> state = new();
        int fired = 0;
        state.StateChanged += () => fired++;

        state.Select(1);

        fired.Should().Be(1);
    }

    [Fact]
    public void StateChanged_Should_Fire_On_Deselect_And_Clear()
    {
        SelectionState<int> state = new();
        state.Select(1);

        int fired = 0;
        state.StateChanged += () => fired++;

        state.Deselect(1);
        state.Clear();
        state.SetValue(5);
        state.SetSingleValue(6);

        fired.Should().Be(4);
    }

    [Fact]
    public void StateChanged_Should_Not_Fire_When_Select_Null()
    {
        SelectionState<string> state = new();
        int fired = 0;
        state.StateChanged += () => fired++;

        state.Select(null);
        state.Deselect(null);
        state.Toggle(null);

        fired.Should().Be(0);
    }
}
