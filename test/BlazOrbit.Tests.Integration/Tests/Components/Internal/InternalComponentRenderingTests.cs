using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Components.Forms.Internal;
using BlazOrbit.Components.Internal;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Internal;

/* ─────────────────────────────────────────────────────────────
 *  _BOBAddon
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBAddon")]
public class BOBAddonRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBAddon> cut = ctx.Render<_BOBAddon>(p => p
            .Add(c => c.Icon, BOBIconKeys.UI.Close));

        cut.Find("bob-component").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Loading_Indicator(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBAddon> cut = ctx.Render<_BOBAddon>(p => p
            .Add(c => c.Loading, true));

        cut.Find("._bob-addon bob-component").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Child_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBAddon> cut = ctx.Render<_BOBAddon>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "text")));

        cut.Find("._bob-addon").TextContent.Should().Be("text");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Icon_Rotation_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBAddon> cut = ctx.Render<_BOBAddon>(p => p
            .Add(c => c.Icon, BOBIconKeys.UI.ExpandMore)
            .Add(c => c.IconRotation, 180));

        cut.Find("._bob-addon").InnerHtml.Should().Contain("rotate(180deg)");
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBBtn
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBBtn")]
public class BOBBtnRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_As_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBBtn> cut = ctx.Render<_BOBBtn>(p => p
            .Add(c => c.AriaLabel, "Close"));

        cut.Find("button").Should().NotBeNull();
        cut.Find("button").GetAttribute("type").Should().Be("button");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBBtn> cut = ctx.Render<_BOBBtn>(p => p
            .Add(c => c.Icon, BOBIconKeys.UI.Close)
            .Add(c => c.AriaLabel, "Close"));

        cut.Find("bob-component").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_And_Tabindex(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBBtn> cut = ctx.Render<_BOBBtn>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.AriaLabel, "Close"));

        var btn = cut.Find("button");
        btn.HasAttribute("disabled").Should().BeTrue();
        btn.GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Active_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBBtn> cut = ctx.Render<_BOBBtn>(p => p
            .Add(c => c.Active, true)
            .Add(c => c.AriaLabel, "Page 1"));

        cut.Find("button").GetAttribute("data-bob-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnClick(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<_BOBBtn> cut = ctx.Render<_BOBBtn>(p => p
            .Add(c => c.AriaLabel, "Action")
            .Add(c => c.OnClick, _ => { fired = true; return Task.CompletedTask; }));

        cut.Find("button").Click();

        fired.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBBtn> cut = ctx.Render<_BOBBtn>(p => p
            .Add(c => c.Loading, true)
            .Add(c => c.AriaLabel, "Loading"));

        cut.Find("button").GetAttribute("aria-busy").Should().Be("true");
        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBCheckMark
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBCheckMark")]
public class BOBCheckMarkRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Checkbox_Role_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBCheckMark> cut = ctx.Render<_BOBCheckMark>();

        cut.Find("span").GetAttribute("role").Should().Be("checkbox");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Radio_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBCheckMark> cut = ctx.Render<_BOBCheckMark>(p => p
            .Add(c => c.Mode, _BOBCheckMark.CheckMarkMode.Radio));

        cut.Find("span").GetAttribute("role").Should().Be("radio");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Checked_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBCheckMark> cut = ctx.Render<_BOBCheckMark>(p => p
            .Add(c => c.Checked, true));

        cut.Find("span").GetAttribute("data-bob-checked").Should().Be("true");
        cut.Find("span").GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Indeterminate_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBCheckMark> cut = ctx.Render<_BOBCheckMark>(p => p
            .Add(c => c.Checked, null));

        cut.Find("span").GetAttribute("data-bob-indeterminate").Should().Be("true");
        cut.Find("span").GetAttribute("aria-checked").Should().Be("mixed");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBCheckMark> cut = ctx.Render<_BOBCheckMark>(p => p
            .Add(c => c.Disabled, true));

        cut.Find("span").GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("span").GetAttribute("aria-disabled").Should().Be("true");
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBFieldHelper
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBFieldHelper")]
public class BOBFieldHelperRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Helper_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBFieldHelper<string>> cut = ctx.Render<_BOBFieldHelper<string>>(p => p
            .Add(c => c.Id, "helper-1")
            .Add(c => c.HelperText, "This is a hint"));

        var div = cut.Find(".bob-field-helper");
        div.TextContent.Should().Be("This is a hint");
        div.GetAttribute("id").Should().Be("helper-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBFieldHelper<string>> cut = ctx.Render<_BOBFieldHelper<string>>();

        cut.FindAll(".bob-field-helper").Should().BeEmpty();
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBInNumber
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBInNumber")]
public class BOBInNumberRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Number_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBInNumber> cut = ctx.Render<_BOBInNumber>(p => p
            .Add(c => c.Value, 42));

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("number");
        input.GetAttribute("value").Should().Be("42");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int? changed = null;
        IRenderedComponent<_BOBInNumber> cut = ctx.Render<_BOBInNumber>(p => p
            .Add(c => c.ValueChanged, v => { changed = v; return Task.CompletedTask; }));

        cut.Find("input").Change("7");

        changed.Should().Be(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Parse_Invalid_As_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int? changed = -1;
        IRenderedComponent<_BOBInNumber> cut = ctx.Render<_BOBInNumber>(p => p
            .Add(c => c.ValueChanged, v => { changed = v; return Task.CompletedTask; }));

        cut.Find("input").Change("not-a-number");

        changed.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Min_Max_Step(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBInNumber> cut = ctx.Render<_BOBInNumber>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Step, 5));

        var input = cut.Find("input");
        input.GetAttribute("min").Should().Be("0");
        input.GetAttribute("max").Should().Be("100");
        input.GetAttribute("step").Should().Be("5");
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBInSelect
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBInSelect")]
public class BOBInSelectRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Select_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBInSelect> cut = ctx.Render<_BOBInSelect>(p => p
            .Add(c => c.Value, "b")
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                "<option value='a'>A</option><option value='b'>B</option>")));

        var options = cut.FindAll("option");
        options.Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? changed = null;
        IRenderedComponent<_BOBInSelect> cut = ctx.Render<_BOBInSelect>(p => p
            .Add(c => c.ValueChanged, v => { changed = v; return Task.CompletedTask; }));

        cut.Find("select").Change("new-val");

        changed.Should().Be("new-val");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBInSelect> cut = ctx.Render<_BOBInSelect>(p => p
            .Add(c => c.Disabled, true));

        cut.Find("select").HasAttribute("disabled").Should().BeTrue();
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBInText
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBInText")]
public class BOBInTextRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Text_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBInText> cut = ctx.Render<_BOBInText>(p => p
            .Add(c => c.Value, "hello"));

        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("text");
        input.GetAttribute("value").Should().Be("hello");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? changed = null;
        IRenderedComponent<_BOBInText> cut = ctx.Render<_BOBInText>(p => p
            .Add(c => c.ValueChanged, v => { changed = v; return Task.CompletedTask; }));

        cut.Find("input").Change("world");

        changed.Should().Be("world");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Placeholder(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBInText> cut = ctx.Render<_BOBInText>(p => p
            .Add(c => c.Placeholder, "Type here"));

        cut.Find("input").GetAttribute("placeholder").Should().Be("Type here");
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBPagination
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBPagination")]
public class BOBPaginationRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Page_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBPagination> cut = ctx.Render<_BOBPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.TotalItems, 50)
            .Add(c => c.CurrentPage, 1));

        // Prev + 5 pages + Next = 7 buttons
        cut.FindAll("button").Should().HaveCount(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Info_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBPagination> cut = ctx.Render<_BOBPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.TotalItems, 50)
            .Add(c => c.PageSize, 10)
            .Add(c => c.CurrentPage, 2));

        cut.Find("._bob-pagination__info").TextContent.Should().Contain("Showing 11 to 20 of 50");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Previous_On_First_Page(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBPagination> cut = ctx.Render<_BOBPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.TotalItems, 50)
            .Add(c => c.CurrentPage, 1));

        var buttons = cut.FindAll("button");
        buttons[0].HasAttribute("disabled").Should().BeTrue(); // Previous
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Next_On_Last_Page(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBPagination> cut = ctx.Render<_BOBPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.TotalItems, 50)
            .Add(c => c.CurrentPage, 5));

        var buttons = cut.FindAll("button");
        buttons[^1].HasAttribute("disabled").Should().BeTrue(); // Next
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnPageChange(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int? clickedPage = null;
        IRenderedComponent<_BOBPagination> cut = ctx.Render<_BOBPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.TotalItems, 50)
            .Add(c => c.CurrentPage, 1)
            .Add(c => c.OnPageChange, p => { clickedPage = p; return Task.CompletedTask; }));

        // Click page 3 (index 3: prev + page1 + page2 + page3)
        cut.FindAll("button")[3].Click();

        clickedPage.Should().Be(3);
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBSliderThumb
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBSliderThumb")]
public class BOBSliderThumbRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Slider_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderThumb> cut = ctx.Render<_BOBSliderThumb>();

        var thumb = cut.Find("[role='slider']");
        thumb.Should().NotBeNull();
        thumb.GetAttribute("tabindex").Should().Be("0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Value_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderThumb> cut = ctx.Render<_BOBSliderThumb>(p => p
            .Add(c => c.AriaValueMin, "0")
            .Add(c => c.AriaValueMax, "100")
            .Add(c => c.AriaValueNow, "50")
            .Add(c => c.AriaValueText, "Fifty percent"));

        var thumb = cut.Find("[role='slider']");
        thumb.GetAttribute("aria-valuemin").Should().Be("0");
        thumb.GetAttribute("aria-valuemax").Should().Be("100");
        thumb.GetAttribute("aria-valuenow").Should().Be("50");
        thumb.GetAttribute("aria-valuetext").Should().Be("Fifty percent");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Tabindex(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderThumb> cut = ctx.Render<_BOBSliderThumb>(p => p
            .Add(c => c.Disabled, true));

        cut.Find("[role='slider']").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Value_Label_When_Dragging(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderThumb> cut = ctx.Render<_BOBSliderThumb>(p => p
            .Add(c => c.Dragging, true)
            .Add(c => c.ValueLabel, "75"));

        cut.Find("._bob-slider-thumb__label").TextContent.Should().Be("75");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Value_Label_When_Not_Dragging_Or_Focused(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderThumb> cut = ctx.Render<_BOBSliderThumb>(p => p
            .Add(c => c.Dragging, false)
            .Add(c => c.Focused, false)
            .Add(c => c.ValueLabel, "75"));

        cut.FindAll("._bob-slider-thumb__label").Should().BeEmpty();
    }
}

/* ─────────────────────────────────────────────────────────────
 *  _BOBSliderTrack
 * ───────────────────────────────────────────────────────────── */

[Trait("Component Rendering", "_BOBSliderTrack")]
public class BOBSliderTrackRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Track_With_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderTrack> cut = ctx.Render<_BOBSliderTrack>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "thumb")));

        cut.Find("._bob-slider-track").Should().NotBeNull();
        cut.Find("._bob-slider-track__thumbs").TextContent.Should().Be("thumb");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Ticks_When_ShowTicks(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderTrack> cut = ctx.Render<_BOBSliderTrack>(p => p
            .Add(c => c.ShowTicks, true)
            .Add(c => c.TickPercents, [0, 25, 50, 75, 100]));

        cut.FindAll("._bob-slider-track__tick").Should().HaveCount(5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Orientation_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<_BOBSliderTrack> cut = ctx.Render<_BOBSliderTrack>(p => p
            .Add(c => c.Orientation, BOBSliderOrientation.Vertical));

        cut.Find("._bob-slider-track").GetAttribute("data-orientation").Should().Be("vertical");
    }
}
