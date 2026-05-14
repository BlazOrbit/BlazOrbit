using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Pure-C# unit tests for <see cref="BOBDataCollectionClipboardSerializer{TItem}"/>. Produces the
/// TSV (text/plain) + HTML (text/html) payloads written to the system clipboard by the DataGrid
/// "Copy" toolbar button. TSV is what Excel/Sheets/Numbers pick up natively as tab-separated
/// values; HTML preserves cell typing (numbers stay numbers, dates stay dates) when the target
/// pastes from text/html.
/// </summary>
[Trait("Library", "DataCollectionClipboard")]
public class BOBDataCollectionClipboardSerializerTests
{
    private sealed class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal Salary { get; set; }
        public string? Notes { get; set; }
    }

    private static List<Person> Seed() =>
    [
        new() { Name = "Alice", Age = 30, Salary = 45_000.50m, Notes = "lead" },
        new() { Name = "Bob",   Age = 25, Salary = 32_000.00m, Notes = null }
    ];

    private static DataColumnRegistration<Person>[] AllColumns() =>
    [
        new() { Header = "Name",   ValueSelector = p => p.Name },
        new() { Header = "Age",    ValueSelector = p => p.Age },
        new() { Header = "Salary", ValueSelector = p => p.Salary, Format = "C2" },
        new() { Header = "Notes",  ValueSelector = p => p.Notes }
    ];

    [Fact]
    public void BuildTsv_Should_Emit_Headers_When_Requested()
    {
        string tsv = BOBDataCollectionClipboardSerializer<Person>.BuildTsv(
            Seed(), AllColumns(), includeHeaders: true);

        string[] lines = tsv.Split('\n');
        lines[0].Should().Be("Name\tAge\tSalary\tNotes");
    }

    [Fact]
    public void BuildTsv_Should_Omit_Headers_When_Not_Requested()
    {
        string tsv = BOBDataCollectionClipboardSerializer<Person>.BuildTsv(
            Seed(), AllColumns(), includeHeaders: false);

        string[] lines = tsv.Split('\n').Where(l => l.Length > 0).ToArray();
        lines.Should().HaveCount(2, "two data rows, no header row");
        lines[0].Should().StartWith("Alice");
    }

    [Fact]
    public void BuildTsv_Should_Use_Tab_As_Column_Separator()
    {
        string tsv = BOBDataCollectionClipboardSerializer<Person>.BuildTsv(
            Seed().Take(1).ToList(), AllColumns(), includeHeaders: false);

        tsv.TrimEnd('\n').Should().Contain("\t",
            "Excel and Sheets interpret tab-separated rows as columnar paste");
    }

    [Fact]
    public void BuildTsv_Should_Skip_Hidden_Columns()
    {
        DataColumnRegistration<Person>[] cols =
        [
            new() { Header = "Name", ValueSelector = p => p.Name },
            new() { Header = "Internal", ValueSelector = p => p.Salary, Visible = false },
            new() { Header = "Age", ValueSelector = p => p.Age }
        ];

        string tsv = BOBDataCollectionClipboardSerializer<Person>.BuildTsv(
            Seed(), cols, includeHeaders: true);

        tsv.Should().NotContain("Internal");
        tsv.Split('\n')[0].Should().Be("Name\tAge");
    }

    [Fact]
    public void BuildTsv_Should_Apply_Column_Format_To_Formattable_Values()
    {
        // Salary column has Format="C2". Currency format is locale-sensitive; we just verify
        // the format string was applied (output differs from the raw decimal "45000.50").
        string tsv = BOBDataCollectionClipboardSerializer<Person>.BuildTsv(
            Seed().Take(1).ToList(), AllColumns(), includeHeaders: false);

        tsv.Should().NotContain("45000.50",
            "the raw decimal must not appear — the C2 format was applied");
    }

    [Fact]
    public void BuildTsv_Should_Handle_Null_Values_As_Empty_String()
    {
        // Bob's Notes is null; expect an empty cell, not the literal "null".
        string tsv = BOBDataCollectionClipboardSerializer<Person>.BuildTsv(
            Seed(), AllColumns(), includeHeaders: false);

        tsv.Should().NotContain("null");
        // Bob's row: "Bob\t25\t<salary>\t" (trailing tab-empty for null Notes).
        string[] lines = tsv.Split('\n');
        lines[1].Should().EndWith("\t",
            "a null cell renders as an empty TSV field — Excel reads it as blank");
    }

    [Fact]
    public void BuildTsv_Should_Escape_Tabs_And_Newlines_Inside_Cell_Values()
    {
        List<Person> data = [new() { Name = "Has\ttab", Age = 1, Notes = "line1\nline2" }];

        string tsv = BOBDataCollectionClipboardSerializer<Person>.BuildTsv(
            data,
            [
                new() { Header = "Name", ValueSelector = p => p.Name },
                new() { Header = "Notes", ValueSelector = p => p.Notes }
            ],
            includeHeaders: false);

        // The user's tab must not be interpreted as a column boundary, nor the newline as a row boundary.
        tsv.Split('\n').Where(l => l.Length > 0).Should().HaveCount(1,
            "embedded newlines must be neutralised so the value stays on a single TSV row");
        tsv.Split('\t').Should().HaveCount(2,
            "embedded tabs must be neutralised so the value stays in a single TSV column");
    }

    [Fact]
    public void BuildHtml_Should_Wrap_Output_In_Table_Element()
    {
        string html = BOBDataCollectionClipboardSerializer<Person>.BuildHtml(
            Seed(), AllColumns(), includeHeaders: true);

        html.Should().StartWith("<table");
        html.Should().EndWith("</table>");
        html.Should().Contain("<thead>").And.Contain("<tbody>");
    }

    [Fact]
    public void BuildHtml_Should_Emit_Th_For_Headers_And_Td_For_Cells()
    {
        string html = BOBDataCollectionClipboardSerializer<Person>.BuildHtml(
            Seed(), AllColumns(), includeHeaders: true);

        html.Should().Contain("<th>Name</th>");
        html.Should().Contain("<td>Alice</td>");
    }

    [Fact]
    public void BuildHtml_Should_Escape_Html_Special_Characters_In_Cells()
    {
        List<Person> data = [new() { Name = "<script>alert(1)</script>", Age = 1 }];
        DataColumnRegistration<Person>[] cols =
        [
            new() { Header = "Name", ValueSelector = p => p.Name }
        ];

        string html = BOBDataCollectionClipboardSerializer<Person>.BuildHtml(
            data, cols, includeHeaders: false);

        html.Should().NotContain("<script>",
            "raw HTML in cell values must be escaped to defend Excel's HTML clipboard reader");
        html.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void BuildHtml_Should_Handle_Null_Values_As_Empty_Cell()
    {
        string html = BOBDataCollectionClipboardSerializer<Person>.BuildHtml(
            Seed(), AllColumns(), includeHeaders: false);

        // Bob has Notes = null → the corresponding <td> must be empty, not "null".
        html.Should().NotContain(">null<");
        html.Should().Contain("<td></td>");
    }

    [Fact]
    public void BuildHtml_Should_Skip_Hidden_Columns()
    {
        DataColumnRegistration<Person>[] cols =
        [
            new() { Header = "Name", ValueSelector = p => p.Name },
            new() { Header = "Internal", ValueSelector = p => p.Salary, Visible = false }
        ];

        string html = BOBDataCollectionClipboardSerializer<Person>.BuildHtml(
            Seed(), cols, includeHeaders: true);

        html.Should().NotContain("Internal");
        html.Should().NotContain("45000");
    }
}
