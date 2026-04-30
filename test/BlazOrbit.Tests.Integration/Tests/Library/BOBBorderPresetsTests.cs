using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Direct unit tests for <see cref="BOBBorderPresets" /> and <see cref="BorderStyle" /> /
/// <see cref="BorderCssValues" />.
/// Pins every preset's CSS output and the fluent builder semantics (All / per-side /
/// None / Radius / mixed side colors).
/// </summary>
[Trait("Core", "BOBBorderPresets")]
public class BOBBorderPresetsTests
{
    // ─────────── Presets ───────────

    [Fact]
    public void None_Should_Produce_Zero_All_And_No_Per_Side_Or_Radius()
    {
        BorderCssValues css = BOBBorderPresets.None.GetCssValues();

        css.All.Should().Be("none");
        css.Top.Should().BeNull();
        css.Right.Should().BeNull();
        css.Bottom.Should().BeNull();
        css.Left.Should().BeNull();
        css.Radius.Should().BeNull();
    }

    [Fact]
    public void Default_Should_Be_1px_Solid_Gray()
    {
        BorderCssValues css = BOBBorderPresets.Default.GetCssValues();

        css.All.Should().StartWith("1px solid ");
        css.Radius.Should().BeNull();
    }

    [Fact]
    public void Rounded_Should_Apply_4px_Radius()
    {
        BorderCssValues css = BOBBorderPresets.Rounded.GetCssValues();

        css.All.Should().StartWith("1px solid ");
        css.Radius.Should().Be("4px");
    }

    [Fact]
    public void RoundedLarge_Should_Apply_8px_Radius()
    {
        BorderCssValues css = BOBBorderPresets.RoundedLarge.GetCssValues();

        css.Radius.Should().Be("8px");
    }

    [Fact]
    public void Pill_Should_Apply_9999px_Radius()
    {
        BorderCssValues css = BOBBorderPresets.Pill.GetCssValues();

        css.Radius.Should().Be("9999px");
    }

    [Fact]
    public void Dashed_Should_Use_Dashed_Style()
    {
        BorderCssValues css = BOBBorderPresets.Dashed.GetCssValues();

        css.All.Should().Contain("dashed");
    }

    [Fact]
    public void Dotted_Should_Use_Dotted_Style()
    {
        BorderCssValues css = BOBBorderPresets.Dotted.GetCssValues();

        css.All.Should().Contain("dotted");
    }

    [Fact]
    public void Double_Should_Use_3px_Double()
    {
        BorderCssValues css = BOBBorderPresets.Double.GetCssValues();

        css.All.Should().StartWith("3px double");
    }

    [Fact]
    public void Strong_Should_Be_2px_Solid()
    {
        BorderCssValues css = BOBBorderPresets.Strong.GetCssValues();

        css.All.Should().StartWith("2px solid");
    }

    [Fact]
    public void Primary_Should_Reference_Primary_Palette()
    {
        BorderCssValues css = BOBBorderPresets.Primary.GetCssValues();

        css.All.Should().Contain("2px solid");
        css.All.Should().Contain("--palette-primary");
    }

    [Theory]
    [InlineData("Error", "--palette-error")]
    [InlineData("Success", "--palette-success")]
    [InlineData("Warning", "--palette-warning")]
    [InlineData("Info", "--palette-info")]
    [InlineData("Secondary", "--palette-secondary")]
    public void Semantic_Presets_Should_Reference_Palette_Token(string preset, string paletteVar)
    {
        BorderStyle style = preset switch
        {
            "Error" => BOBBorderPresets.Error,
            "Success" => BOBBorderPresets.Success,
            "Warning" => BOBBorderPresets.Warning,
            "Info" => BOBBorderPresets.Info,
            "Secondary" => BOBBorderPresets.Secondary,
            _ => throw new ArgumentOutOfRangeException(nameof(preset))
        };

        BorderCssValues css = style.GetCssValues();

        css.All.Should().Contain("2px solid");
        css.All.Should().Contain(paletteVar);
    }

    // ─────────── BorderStyle fluent builder ───────────

    [Fact]
    public void All_Should_Clear_Per_Side_Values()
    {
        BorderStyle style = BorderStyle.Create()
            .Top("3px", BorderStyleType.Solid, "red")
            .All("1px", BorderStyleType.Solid, "blue");

        BorderCssValues css = style.GetCssValues();

        css.All.Should().Be("1px solid blue");
        css.Top.Should().BeNull();
        css.Right.Should().BeNull();
        css.Bottom.Should().BeNull();
        css.Left.Should().BeNull();
    }

    [Fact]
    public void Per_Side_Setters_Should_Emit_Only_Configured_Sides()
    {
        BorderStyle style = BorderStyle.Create()
            .Top("1px", BorderStyleType.Solid, "red")
            .Left("2px", BorderStyleType.Dashed, "blue");

        BorderCssValues css = style.GetCssValues();

        css.All.Should().BeNull();
        css.Top.Should().Be("1px solid red");
        css.Left.Should().Be("2px dashed blue");
        css.Right.Should().BeNull();
        css.Bottom.Should().BeNull();
    }

    [Fact]
    public void None_Builder_Should_Emit_Zero_All_And_Clear_Radius()
    {
        BorderStyle style = BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, "red")
            .Radius(8)
            .None();

        BorderCssValues css = style.GetCssValues();

        css.All.Should().Be("none");
        css.Radius.Should().BeNull();
    }

    [Fact]
    public void Zero_Width_Or_None_Style_Collapses_To_None()
    {
        BorderStyle style = BorderStyle.Create()
            .All("0", BorderStyleType.Solid, "red");

        BorderCssValues css = style.GetCssValues();

        css.All.Should().Be("none");
    }

    [Fact]
    public void Radius_All_Int_Should_Emit_Uniform_Value()
    {
        BorderStyle style = BorderStyle.Create().Radius(12);

        style.GetRadiusCss().Should().Be("12px");
    }

    [Fact]
    public void Radius_Per_Corner_Should_Emit_Four_Value_Css()
    {
        BorderStyle style = BorderStyle.Create()
            .Radius(topLeft: 1, topRight: 2, bottomRight: 3, bottomLeft: 4);

        style.GetRadiusCss().Should().Be("1px 2px 3px 4px");
    }

    [Fact]
    public void Radius_Partial_Corners_Should_Default_Missing_To_TopLeft()
    {
        BorderStyle style = BorderStyle.Create().Radius(topLeft: 5);

        style.GetRadiusCss().Should().Be("5px");
    }

    [Fact]
    public void Radius_Negative_Should_Be_Clamped_To_Zero()
    {
        BorderStyle style = BorderStyle.Create().Radius(-10);

        style.GetRadiusCss().Should().Be("0px");
    }

    [Fact]
    public void GetColorCss_Should_Return_All_Color_When_All_Is_Set()
    {
        BorderStyle style = BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, "red");

        style.GetColorCss().Should().Be("red");
    }

    [Fact]
    public void GetColorCss_Should_Return_Single_Color_When_All_Sides_Match()
    {
        BorderStyle style = BorderStyle.Create()
            .Top("1px", BorderStyleType.Solid, "red")
            .Right("1px", BorderStyleType.Solid, "red")
            .Bottom("1px", BorderStyleType.Solid, "red")
            .Left("1px", BorderStyleType.Solid, "red");

        style.GetColorCss().Should().Be("red");
    }

    [Fact]
    public void GetColorCss_Should_Emit_Four_Colors_When_Sides_Differ()
    {
        BorderStyle style = BorderStyle.Create()
            .Top("1px", BorderStyleType.Solid, "red")
            .Right("1px", BorderStyleType.Solid, "green")
            .Bottom("1px", BorderStyleType.Solid, "blue")
            .Left("1px", BorderStyleType.Solid, "orange");

        style.GetColorCss().Should().Be("red green blue orange");
    }

    [Fact]
    public void GetColorCss_Should_Fallback_CurrentColor_For_Missing_Sides()
    {
        BorderStyle style = BorderStyle.Create()
            .Top("1px", BorderStyleType.Solid, "red");

        style.GetColorCss().Should().Be("red currentColor currentColor currentColor");
    }

    [Fact]
    public void GetColorCss_Should_Return_Null_When_No_Borders_Set()
    {
        BorderStyle style = BorderStyle.Create();

        style.GetColorCss().Should().BeNull();
    }

    [Fact]
    public void Presets_Should_Be_Fresh_Instances_Per_Access()
    {
        BorderStyle a = BOBBorderPresets.Rounded;
        BorderStyle b = BOBBorderPresets.Rounded;

        a.Should().NotBeSameAs(b);
    }
}
