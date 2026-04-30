using BlazOrbit.CodeGeneration.Tests.Infrastructure;
using BlazOrbit.Generator;
using FluentAssertions;

namespace BlazOrbit.CodeGeneration.Tests.Tests;

[Trait("Generator Snapshots", "ComponentInfoGenerator")]
public class ComponentInfoGeneratorTests
{
    private const string ParameterAttrSource = """
        namespace Microsoft.AspNetCore.Components;

        [System.AttributeUsage(System.AttributeTargets.Property)]
        public sealed class ParameterAttribute : System.Attribute { }
        """;

    [Fact]
    public async Task Should_Skip_Razor_Without_GenerateComponentInfo_Attribute()
    {
        string razor = """
            @namespace TestNs

            @code {
                [Parameter] public string? Text { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts: [("/src/NoMark.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Generate_From_Basic_Razor()
    {
        string razor = """
            @namespace TestNs
            @attribute [GenerateComponentInfo]

            @code {
                /// <summary>Visible label.</summary>
                [Parameter] public string? Text { get; set; }

                /// <summary>Disables interaction.</summary>
                [Parameter] public bool Disabled { get; set; } = false;

                [Parameter] public int Count { get; set; } = 42;
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts: [("/src/MyComponent.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Resolve_Inherited_Params_From_Razor_Base()
    {
        string baseRazor = """
            @namespace TestNs

            @code {
                /// <summary>Shared size.</summary>
                [Parameter] public string? Size { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits BaseThing
            @attribute [GenerateComponentInfo]

            @code {
                /// <summary>Own text.</summary>
                [Parameter] public string? Text { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts:
            [
                ("/src/BaseThing.razor", baseRazor),
                ("/src/Derived.razor", derivedRazor),
            ]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Resolve_Inherited_Params_From_CSharp_Base()
    {
        string csBase = """
            using Microsoft.AspNetCore.Components;

            namespace TestNs;

            public class CsBase
            {
                /// <summary>From C# base.</summary>
                [Parameter] public string? Shared { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits CsBase
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? Own { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource, csBase],
            additionalTexts: [("/src/Derived.razor", derivedRazor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Warn_On_Razor_Name_Collision_Across_Namespaces()
    {
        // two .razor files share the simple name "BaseThing" but live
        // in different folders/namespaces. The generator must emit BOBGEN001
        // so the consumer notices the collision instead of silently picking one.
        string baseRazorA = """
            @namespace TestNs.A

            @code {
                [Parameter] public string? FromA { get; set; }
            }
            """;

        string baseRazorB = """
            @namespace TestNs.B

            @code {
                [Parameter] public string? FromB { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs.A
            @inherits BaseThing
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? Own { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts:
            [
                ("/src/A/BaseThing.razor", baseRazorA),
                ("/src/B/BaseThing.razor", baseRazorB),
                ("/src/A/Derived.razor", derivedRazor),
            ]);

        // BOBGEN001 must appear in the output; same-namespace match (TestNs.A)
        // wins so the derived component picks up `FromA`.
        await Verify(output);
    }

    [Fact]
    public async Task Should_Resolve_Inherited_Params_From_FullyQualified_CSharp_Base()
    {
        // fast path: when @inherits is fully qualified, use
        // GetTypeByMetadataName directly (deterministic, O(1) in metadata).
        string csBaseFoo = """
            using Microsoft.AspNetCore.Components;

            namespace Foo;

            public class Base
            {
                [Parameter] public string? FromFoo { get; set; }
            }
            """;

        string csBaseBar = """
            using Microsoft.AspNetCore.Components;

            namespace Bar;

            public class Base
            {
                [Parameter] public string? FromBar { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits Foo.Base
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? Own { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource, csBaseFoo, csBaseBar],
            additionalTexts: [("/src/Derived.razor", derivedRazor)]);

        // Must pick Foo.Base (FQN match) without emitting BOBGEN002, even
        // though both `Base` types exist in the compilation.
        await Verify(output);
    }

    [Fact]
    public async Task Should_Warn_On_Ambiguous_CSharp_Base_With_Simple_Name()
    {
        // when @inherits uses a simple name and multiple types match,
        // BOBGEN002 fires and the resolver picks the first candidate
        // alphabetically (Bar.Base < Foo.Base).
        string csBaseFoo = """
            using Microsoft.AspNetCore.Components;

            namespace Foo;

            public class Base
            {
                [Parameter] public string? FromFoo { get; set; }
            }
            """;

        string csBaseBar = """
            using Microsoft.AspNetCore.Components;

            namespace Bar;

            public class Base
            {
                [Parameter] public string? FromBar { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits Base
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? Own { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource, csBaseFoo, csBaseBar],
            additionalTexts: [("/src/Derived.razor", derivedRazor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Deduplicate_Child_Overrides_Of_Base_Params()
    {
        string baseRazor = """
            @namespace TestNs

            @code {
                /// <summary>Base desc.</summary>
                [Parameter] public string? Text { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits BaseThing
            @attribute [GenerateComponentInfo]

            @code {
                /// <summary>Child desc.</summary>
                [Parameter] public string? Text { get; set; } = "override";
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts:
            [
                ("/src/BaseThing.razor", baseRazor),
                ("/src/Derived.razor", derivedRazor),
            ]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Stop_On_Inheritance_Cycle()
    {
        // the resolver protects against a → b → a cycles via a
        // `visited` HashSet. Without it, the generator would recurse forever
        // and stack-overflow the build. We render both sides as authored —
        // the cycle just stops collection at the second visit, neither file
        // crashes the generator.
        string razorA = """
            @namespace TestNs
            @inherits B
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? FromA { get; set; }
            }
            """;

        string razorB = """
            @namespace TestNs
            @inherits A
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? FromB { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts:
            [
                ("/src/A.razor", razorA),
                ("/src/B.razor", razorB),
            ]);

        await Verify(output);
    }

    [Fact]
    public void Should_Generate_Identical_Output_When_Run_Twice()
    {
        // incremental generators must be deterministic — the same
        // inputs must produce the same outputs (byte-identical). Two
        // invocations on a fresh harness lock that contract; if a future
        // change introduced ordering instability (e.g. switching to a
        // non-stable sort, hashing without seed), this test catches it.
        string razor = """
            @namespace TestNs
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? Text { get; set; }
                [Parameter] public bool Flag { get; set; } = true;
                [Parameter] public int Count { get; set; } = 7;
            }
            """;

        string first = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts: [("/src/MyComponent.razor", razor)]);

        string second = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts: [("/src/MyComponent.razor", razor)]);

        first.Should().Be(second);
    }

    [Theory]
    [InlineData("private string X = $\"{{\";\n    [Parameter] public int A { get; set; }\n")]
    [InlineData("private string X = @\"hello { world\";\n    [Parameter] public int B { get; set; }\n")]
    [InlineData("private string X = \"\"\"hello { world\"\"\";\n    [Parameter] public int C { get; set; }\n")]
    [InlineData("private char C = '\\u007B';\n    [Parameter] public int D { get; set; }\n")]
    public void ExtractCodeBlock_Should_Handle_Modern_CSharp_Strings(string codeContent)
    {
        string razorContent = $"@code {{\n    {codeContent}}}";
        string? extracted = RazorParser.ExtractCodeBlock(razorContent);
        extracted.Should().NotBeNull();
        extracted!.Trim().Should().Be(codeContent.Trim());
    }
}
