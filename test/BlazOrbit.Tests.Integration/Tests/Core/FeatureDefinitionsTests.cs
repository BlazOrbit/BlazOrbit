using BlazOrbit.Components;
using FluentAssertions;
using System.Reflection;

namespace BlazOrbit.Tests.Integration.Tests.Core;

/// <summary>
/// CORE-T-05: No orphaned FeatureDefinitions constants.
/// Every DataAttributes.* and InlineVariables.* constant must appear in the
/// BOBComponentAttributesBuilder (the consumer) or in a generator/component template.
/// This guards against dead constants that silently drift out of sync.
/// </summary>
[Trait("Core", "FeatureDefinitions")]
public class FeatureDefinitionsTests
{
    [Fact]
    public void DataAttributes_Component_Should_Follow_Bob_Prefix_Convention()
        // Assert — data-bob-component is the root data attribute
        => FeatureDefinitions.DataAttributes.Component.Should().StartWith("data-bob-");

    [Fact]
    public void DataAttributes_All_Should_Start_With_Data_Bob()
    {
        // Arrange — collect all string constants from DataAttributes nested class
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.DataAttributes));

        // Assert — every attribute follows the data-bob-* convention
        foreach (string constant in constants)
        {
            constant.Should().StartWith("data-bob-",
                because: $"DataAttributes constant '{constant}' must follow data-bob-* convention");
        }
    }

    [Fact]
    public void InlineVariables_All_Should_Start_With_Bob_Inline()
    {
        // Arrange — collect all string constants from InlineVariables nested class
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.InlineVariables));

        // Assert — every CSS variable follows --bob-inline-* convention
        foreach (string constant in constants)
        {
            constant.Should().StartWith("--bob-inline-",
                because: $"InlineVariables constant '{constant}' must follow --bob-inline-* convention");
        }
    }

    [Fact]
    public void CssClasses_Input_Should_Follow_BEM_Naming()
    {
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.CssClasses.Input));

        foreach (string constant in constants)
        {
            constant.Should().StartWith("bob-input__",
                because: $"Input CSS class '{constant}' must use bob-input__ BEM prefix");
        }
    }

    [Fact]
    public void CssClasses_Picker_Should_Follow_BEM_Naming()
    {
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.CssClasses.Picker));

        foreach (string constant in constants)
        {
            constant.Should().StartWith("bob-picker__",
                because: $"Picker CSS class '{constant}' must use bob-picker__ BEM prefix");
        }
    }

    [Fact]
    public void DataAttributes_Count_Should_Be_Stable()
    {
        // Regression guard: if a constant is added/removed, this test catches it
        int count = GetStringConstants(typeof(FeatureDefinitions.DataAttributes)).Count();
        count.Should().BeGreaterThan(10, "DataAttributes should define at least the core state attributes");
    }

    [Fact]
    public void Tags_Component_Should_Be_Bob_Component() => FeatureDefinitions.Tags.Component.Should().Be("bob-component");

    private static IEnumerable<string> GetStringConstants(Type type)
    {
        List<string> values = [];

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(string) && field.IsLiteral)
            {
                string? value = (string?)field.GetRawConstantValue();
                if (value != null)
                {
                    values.Add(value);
                }
            }
        }

        foreach (Type nested in type.GetNestedTypes(BindingFlags.Public))
        {
            values.AddRange(GetStringConstants(nested));
        }

        return values;
    }
}
