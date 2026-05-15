using BlazOrbit.Localization;
using BlazOrbit.Localization.TnFormat;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Trait("Component Integration", "TnParser")]
public class TnParserTests
{
    [Fact]
    public void Should_Parse_Single_Entry()
    {
        TnDocument doc = TnParser.Parse(
            """
            # Hello
            Hola
            """, "es-ES");

        doc.Diagnostics.Should().BeEmpty();
        doc.Translations.Should().ContainKey("es-ES");
        doc.Translations["es-ES"][BobLocalizationHash.Compute("Hello")].Should().Be("Hola");
    }

    [Fact]
    public void Should_Parse_Multiple_Entries()
    {
        TnDocument doc = TnParser.Parse(
            """
            # Hello
            Hola

            # Goodbye
            Adiós

            # Welcome
            Bienvenido
            """, "es-ES");

        doc.Translations["es-ES"].Count.Should().Be(3);
        doc.Translations["es-ES"][BobLocalizationHash.Compute("Hello")].Should().Be("Hola");
        doc.Translations["es-ES"][BobLocalizationHash.Compute("Goodbye")].Should().Be("Adiós");
        doc.Translations["es-ES"][BobLocalizationHash.Compute("Welcome")].Should().Be("Bienvenido");
    }

    [Fact]
    public void Should_Support_Multi_Line_Value()
    {
        TnDocument doc = TnParser.Parse(
            """
            # Multi
            line 1
            line 2
            line 3
            """, "en-US");

        doc.Translations["en-US"][BobLocalizationHash.Compute("Multi")]
            .Should().Be("line 1\nline 2\nline 3");
    }

    [Fact]
    public void Should_Support_Multi_Line_Key()
    {
        TnDocument doc = TnParser.Parse(
            """
            # First line of key
            # Second line of key
            Translation here
            """, "en-US");

        ulong hash = BobLocalizationHash.Compute("First line of key\nSecond line of key");
        doc.Translations["en-US"][hash].Should().Be("Translation here");
    }

    [Fact]
    public void Should_Switch_Culture_On_Header()
    {
        TnDocument doc = TnParser.Parse(
            """
            @ es-ES

            # Hello
            Hola

            @ fr-FR

            # Hello
            Bonjour
            """, "");

        doc.Translations["es-ES"][BobLocalizationHash.Compute("Hello")].Should().Be("Hola");
        doc.Translations["fr-FR"][BobLocalizationHash.Compute("Hello")].Should().Be("Bonjour");
    }

    [Fact]
    public void Should_Use_Default_Culture_Before_First_Header()
    {
        TnDocument doc = TnParser.Parse(
            """
            # Hello
            Hola
            """, "es-MX");

        doc.Translations.Should().ContainKey("es-MX");
    }

    [Fact]
    public void Should_Skip_Comments()
    {
        TnDocument doc = TnParser.Parse(
            """
            // This is a comment
            # Hello
            // Inline comment
            Hola
            // Trailing
            """, "es-ES");

        doc.Diagnostics.Should().BeEmpty();
        doc.Translations["es-ES"][BobLocalizationHash.Compute("Hello")].Should().Be("Hola");
    }

    [Fact]
    public void Should_Unescape_Leading_Hash()
    {
        TnDocument doc = TnParser.Parse(
            """
            # The key
            \# This value starts with a literal hash
            """, "en-US");

        doc.Translations["en-US"][BobLocalizationHash.Compute("The key")]
            .Should().Be("# This value starts with a literal hash");
    }

    [Fact]
    public void Should_Unescape_Leading_At()
    {
        TnDocument doc = TnParser.Parse(
            """
            # The key
            \@ literal at-sign value
            """, "en-US");

        doc.Translations["en-US"][BobLocalizationHash.Compute("The key")]
            .Should().Be("@ literal at-sign value");
    }

    [Fact]
    public void Should_Preserve_Special_Characters_In_Body()
    {
        TnDocument doc = TnParser.Parse(
            """
            # Welcome
            ¡Bienvenido, "amigo"! Use < or > or & or { or } as you like.
            """, "es-ES");

        doc.Translations["es-ES"][BobLocalizationHash.Compute("Welcome")]
            .Should().Be("""¡Bienvenido, "amigo"! Use < or > or & or { or } as you like.""");
    }

    [Fact]
    public void Should_Diagnose_Empty_Culture_Header()
    {
        TnDocument doc = TnParser.Parse(
            """
            @
            # Hello
            Hola
            """, "en-US");

        doc.Diagnostics.Should().ContainSingle()
            .Which.Severity.Should().Be(TnDiagnosticSeverity.Error);
    }

    [Fact]
    public void Should_Diagnose_Value_Without_Key()
    {
        TnDocument doc = TnParser.Parse(
            """
            Orphan value with no key
            # Real key
            Real value
            """, "en-US");

        doc.Diagnostics.Should().ContainSingle(d =>
            d.Severity == TnDiagnosticSeverity.Error &&
            d.Message.Contains("outside of a key block"));
    }

    [Fact]
    public void Should_Warn_On_Duplicate_Key()
    {
        TnDocument doc = TnParser.Parse(
            """
            # Hello
            First

            # Hello
            Second
            """, "en-US");

        doc.Diagnostics.Should().ContainSingle(d =>
            d.Severity == TnDiagnosticSeverity.Warning &&
            d.Message.Contains("Duplicate"));
        doc.Translations["en-US"][BobLocalizationHash.Compute("Hello")].Should().Be("Second");
    }

    [Fact]
    public void Should_Handle_Empty_Input()
    {
        TnDocument doc = TnParser.Parse(string.Empty, "en-US");
        doc.Translations.Should().BeEmpty();
        doc.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Should_Handle_Only_Comments_And_Whitespace()
    {
        TnDocument doc = TnParser.Parse(
            """
            // Comment 1

            // Comment 2

            """, "en-US");

        doc.Translations.Should().BeEmpty();
        doc.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Should_Handle_Mixed_Line_Endings()
    {
        string mixed = "# Hello\r\nHola\n\r\n# Goodbye\nAdiós\r\n";
        TnDocument doc = TnParser.Parse(mixed, "es-ES");

        doc.Translations["es-ES"][BobLocalizationHash.Compute("Hello")].Should().Be("Hola");
        doc.Translations["es-ES"][BobLocalizationHash.Compute("Goodbye")].Should().Be("Adiós");
    }

    [Fact]
    public void Should_Handle_Unicode_Keys_And_Values()
    {
        TnDocument doc = TnParser.Parse(
            """
            # こんにちは
            Hello (Japanese)

            # 你好
            Hello (Chinese)

            # שלום
            Hello (Hebrew RTL)
            """, "en-US");

        doc.Translations["en-US"][BobLocalizationHash.Compute("こんにちは")].Should().Be("Hello (Japanese)");
        doc.Translations["en-US"][BobLocalizationHash.Compute("你好")].Should().Be("Hello (Chinese)");
        doc.Translations["en-US"][BobLocalizationHash.Compute("שלום")].Should().Be("Hello (Hebrew RTL)");
    }

    [Fact]
    public void Should_Throw_On_Null_Content()
    {
        Action act = () => TnParser.Parse(null!, "en-US");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_Throw_On_Null_Default_Culture()
    {
        Action act = () => TnParser.Parse("# k\nv", null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
