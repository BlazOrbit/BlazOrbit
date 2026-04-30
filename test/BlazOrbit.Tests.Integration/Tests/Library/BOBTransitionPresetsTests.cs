using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// LIB-06: BOBTransitionPresets produce valid data-bob-transitions values and CSS variables.
/// </summary>
[Trait("Library", "BOBTransitionPresets")]
public class BOBTransitionPresetsTests
{
    [Theory]
    [InlineData(nameof(BOBTransitionPresets.HoverScale))]
    [InlineData(nameof(BOBTransitionPresets.HoverShadow))]
    [InlineData(nameof(BOBTransitionPresets.HoverFade))]
    [InlineData(nameof(BOBTransitionPresets.HoverLift))]
    [InlineData(nameof(BOBTransitionPresets.HoverGlow))]
    [InlineData(nameof(BOBTransitionPresets.CardHover))]
    [InlineData(nameof(BOBTransitionPresets.FocusRing))]
    [InlineData(nameof(BOBTransitionPresets.Interactive))]
    [InlineData(nameof(BOBTransitionPresets.MaterialButton))]
    [InlineData(nameof(BOBTransitionPresets.PremiumButton))]
    [InlineData(nameof(BOBTransitionPresets.GlassMorphism))]
    [InlineData(nameof(BOBTransitionPresets.Neumorphism))]
    public void Preset_Should_Have_Transitions(string presetName)
    {
        // Arrange
        BOBTransitions transitions = (BOBTransitions)typeof(BOBTransitionPresets)
            .GetProperty(presetName)!
            .GetValue(null)!;

        // Assert
        transitions.HasTransitions.Should().BeTrue(
            because: $"preset '{presetName}' must define at least one transition entry");
    }

    [Fact]
    public void HoverScale_DataAttribute_Should_Contain_Hover_Scale()
    {
        // Arrange & Act
        string attr = BOBTransitionPresets.HoverScale.GetDataAttributeValue();

        // Assert
        attr.Should().Contain("hover:scale");
    }

    [Fact]
    public void HoverShadow_DataAttribute_Should_Contain_Hover_BoxShadow()
    {
        // Arrange & Act
        string attr = BOBTransitionPresets.HoverShadow.GetDataAttributeValue();

        // Assert
        attr.Should().Contain("hover:box-shadow");
    }

    [Fact]
    public void Interactive_DataAttribute_Should_Contain_Hover_Focus_Active()
    {
        // Arrange & Act
        string attr = BOBTransitionPresets.Interactive.GetDataAttributeValue();

        // Assert
        attr.Should().Contain("hover:");
        attr.Should().Contain("focus:");
        attr.Should().Contain("active:");
    }

    [Fact]
    public void HoverLift_CssVariables_Should_Include_Translate_And_Shadow()
    {
        // Arrange & Act
        Dictionary<string, string> vars = BOBTransitionPresets.HoverLift.GetCssVariables();

        // Assert — both translate and box-shadow vars present
        vars.Keys.Should().Contain(k => k.Contains("translate"),
            because: "HoverLift includes a translate transition");
        vars.Keys.Should().Contain(k => k.Contains("box-shadow"),
            because: "HoverLift includes a box-shadow transition");
    }

    [Fact]
    public void GetCssVariables_Should_Always_Include_Transition_Shorthand()
    {
        // Arrange & Act
        Dictionary<string, string> vars = BOBTransitionPresets.HoverScale.GetCssVariables();

        // Assert — --bob-t-transition always present
        vars.Should().ContainKey("--bob-t-transition");
        vars["--bob-t-transition"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BOBTransitions_MergeWith_Should_Override_Matching_Properties()
    {
        // Arrange
        BOBTransitions base_ = BOBTransitionPresets.HoverShadow;
        BOBTransitions override_ = BOBTransitionPresets.HoverGlow;

        // Act — merge; both have hover:box-shadow, override should win
        BOBTransitions merged = base_.MergeWith(override_);
        Dictionary<string, string> mergedVars = merged.GetCssVariables();
        Dictionary<string, string> overrideVars = override_.GetCssVariables();

        // Assert — merged box-shadow value equals the override's value
        string? mergedShadow = mergedVars.GetValueOrDefault("--bob-t-hover-box-shadow");
        string? overrideShadow = overrideVars.GetValueOrDefault("--bob-t-hover-box-shadow");
        mergedShadow.Should().Be(overrideShadow);
    }
}
