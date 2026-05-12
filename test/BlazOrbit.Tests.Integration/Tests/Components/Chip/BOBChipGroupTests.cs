using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Chip;

[Trait("Component State", "BOBChipGroup")]
public class BOBChipGroupTests
{
    private static RenderFragment ChipChild(string value, string label) => b =>
    {
        b.OpenComponent<BOBChip>(0);
        b.AddAttribute(1, "Value", value);
        b.AddAttribute(2, "Label", label);
        b.CloseComponent();
    };

    private static RenderFragment ThreeChips() => b =>
    {
        ChipChild("a", "Alpha")(b);
        ChipChild("b", "Beta")(b);
        ChipChild("c", "Gamma")(b);
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Radiogroup_Role_For_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBChipGroup<string>> cut = ctx.Render<BOBChipGroup<string>>(p => p
            .Add(c => c.Mode, ChipGroupSelectionMode.Single)
            .Add(c => c.ChildContent, ThreeChips()));

        // Single-select chips behave like a radio group semantically.
        cut.Find(".bob-chip-group__items").GetAttribute("role").Should().Be("radiogroup");
        cut.FindAll("button.bob-chip").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Group_Role_For_Multiple_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBChipGroup<string>> cut = ctx.Render<BOBChipGroup<string>>(p => p
            .Add(c => c.Mode, ChipGroupSelectionMode.Multiple)
            .Add(c => c.ChildContent, ThreeChips()));

        cut.Find(".bob-chip-group__items").GetAttribute("role").Should().Be("group");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Single_Select_Replace_Previous_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<string>? captured = null;
        IRenderedComponent<BOBChipGroup<string>> cut = ctx.Render<BOBChipGroup<string>>(p => p
            .Add(c => c.Mode, ChipGroupSelectionMode.Single)
            .Add(c => c.SelectedValues, ["a"])
            .Add(c => c.SelectedValuesChanged,
                EventCallback.Factory.Create<IReadOnlyList<string>>(p, v => captured = v))
            .Add(c => c.ChildContent, ThreeChips()));

        // Tap chip "b" — single-select replaces the whole list with the new value.
        cut.FindAll("button.bob-chip")[1].Click();

        captured.Should().NotBeNull();
        captured!.Should().BeEquivalentTo(new[] { "b" });
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Single_Select_Clear_When_Tapping_Selected_Again(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<string>? captured = null;
        IRenderedComponent<BOBChipGroup<string>> cut = ctx.Render<BOBChipGroup<string>>(p => p
            .Add(c => c.Mode, ChipGroupSelectionMode.Single)
            .Add(c => c.SelectedValues, ["a"])
            .Add(c => c.SelectedValuesChanged,
                EventCallback.Factory.Create<IReadOnlyList<string>>(p, v => captured = v))
            .Add(c => c.ChildContent, ThreeChips()));

        // Tap the already-selected chip — toggle-off clears the list to empty.
        cut.FindAll("button.bob-chip")[0].Click();

        captured.Should().NotBeNull();
        captured!.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Multiple_Select_Toggle_Membership(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<string>? captured = null;
        IRenderedComponent<BOBChipGroup<string>> cut = ctx.Render<BOBChipGroup<string>>(p => p
            .Add(c => c.Mode, ChipGroupSelectionMode.Multiple)
            .Add(c => c.SelectedValues, ["a"])
            .Add(c => c.SelectedValuesChanged,
                EventCallback.Factory.Create<IReadOnlyList<string>>(p, v => captured = v))
            .Add(c => c.ChildContent, ThreeChips()));

        cut.FindAll("button.bob-chip")[1].Click();

        captured.Should().NotBeNull();
        captured!.Should().BeEquivalentTo(new[] { "a", "b" });
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Selected_State_On_Child_Chips(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBChipGroup<string>> cut = ctx.Render<BOBChipGroup<string>>(p => p
            .Add(c => c.Mode, ChipGroupSelectionMode.Multiple)
            .Add(c => c.SelectedValues, ["a", "c"])
            .Add(c => c.ChildContent, ThreeChips()));

        IReadOnlyList<AngleSharp.Dom.IElement> chips = cut.FindAll("button.bob-chip");
        chips[0].GetAttribute("data-bob-selected").Should().Be("true");
        chips[1].GetAttribute("data-bob-selected").Should().BeNull();
        chips[2].GetAttribute("data-bob-selected").Should().Be("true");
        chips[0].GetAttribute("aria-checked").Should().Be("true");
        chips[1].GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Children_When_Group_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<string>? captured = null;
        IRenderedComponent<BOBChipGroup<string>> cut = ctx.Render<BOBChipGroup<string>>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.SelectedValues, ["a"])
            .Add(c => c.SelectedValuesChanged,
                EventCallback.Factory.Create<IReadOnlyList<string>>(p, v => captured = v))
            .Add(c => c.ChildContent, ThreeChips()));

        IReadOnlyList<AngleSharp.Dom.IElement> chips = cut.FindAll("button.bob-chip");
        chips.Should().AllSatisfy(b => b.HasAttribute("disabled").Should().BeTrue());

        chips[1].Click();

        // Disabled group → click is a no-op, callback never fires.
        captured.Should().BeNull();
    }
}