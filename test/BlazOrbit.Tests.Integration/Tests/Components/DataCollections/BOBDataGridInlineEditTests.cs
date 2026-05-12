using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataGrid")]
public class BOBDataGridInlineEditTests
{
    private sealed class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private static List<Person> NewSeed() =>
    [
        new() { Id = 1, Name = "Alice", Age = 30 },
        new() { Id = 2, Name = "Bob", Age = 25 }
    ];

    private static RenderFragment EditableColumns(bool withValidator = false) => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Property", (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Name));
        b.AddAttribute(3, "Editable", true);
        if (withValidator)
        {
            b.AddAttribute(4, "Validator", (Func<Person, object?, string?>)((p, v) =>
                string.IsNullOrWhiteSpace(v?.ToString()) ? "Name is required" : null));
        }
        b.CloseComponent();
        b.OpenComponent<BOBDataColumn<Person>>(5);
        b.AddAttribute(6, "Header", "Age");
        b.AddAttribute(7, "Property", (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Age));
        b.AddAttribute(8, "Editable", true);
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task DoubleClick_Activates_Cell_Editor_When_EditMode_Cell(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        // Arrange
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Cell));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Act — double-click the first editable cell (Name column on row 1).
        AngleSharp.Dom.IElement targetCell = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => targetCell.DoubleClick());

        // Assert — the cell now hosts an <input>.
        cut.Markup.Should().Contain("bob-datagrid__cell--editing");
        cut.FindAll("tbody tr td input").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task DoubleClick_NoOp_When_EditMode_None(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.None));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        AngleSharp.Dom.IElement targetCell = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => targetCell.DoubleClick());

        // Assert — no editor inserted.
        cut.Markup.Should().NotContain("bob-datagrid__cell--editing");
        cut.FindAll("tbody tr td input").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Cell_Editor_Commits_Value_On_Blur(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();
        Person? saved = null;

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Cell)
            .Add(c => c.OnRowSave, EventCallback.Factory.Create<Person>(this, p => { saved = p; })));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Act — open editor, type new value, blur.
        AngleSharp.Dom.IElement targetCell = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => targetCell.DoubleClick());
        AngleSharp.Dom.IElement input = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input.Input("Alicia"));
        await cut.InvokeAsync(() => input.Blur());

        // Assert — value persisted to row item + OnRowSave fired with the mutated row.
        seed[0].Name.Should().Be("Alicia");
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Alicia");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Validator_Failure_Keeps_Editor_Open_And_Marks_Cell_Invalid(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns(withValidator: true))
            .Add(c => c.EditMode, BOBDataGridEditMode.Cell));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Act — open editor, clear value, blur (validator rejects empty string).
        AngleSharp.Dom.IElement targetCell = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => targetCell.DoubleClick());
        AngleSharp.Dom.IElement input = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input.Input(string.Empty));
        await cut.InvokeAsync(() => input.Blur());

        // Assert — editor stays open + cell flagged invalid + row Name unchanged.
        cut.Markup.Should().Contain("bob-datagrid__cell--invalid");
        seed[0].Name.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BatchMode_Save_Fires_OnRowSave_For_Each_Edited_Row(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();
        List<Person> saved = [];

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Batch)
            .Add(c => c.OnRowSave, EventCallback.Factory.Create<Person>(this, p => { saved.Add(p); })));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Act — edit cell on row 1, blur (stages without firing OnRowSave in batch mode).
        AngleSharp.Dom.IElement cellRow1 = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => cellRow1.DoubleClick());
        AngleSharp.Dom.IElement input1 = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input1.Input("Alicia"));
        await cut.InvokeAsync(() => input1.Blur());

        // Toolbar Save button — located by aria-label (the icon-only render path has no text).
        AngleSharp.Dom.IElement saveBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Save changes");
        await cut.InvokeAsync(() => saveBtn.Click());

        // Assert — OnRowSave fired exactly once for the row touched.
        saved.Should().HaveCount(1);
        saved[0].Name.Should().Be("Alicia");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BatchMode_Cancel_Reverts_To_Original_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Batch));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Act — edit + blur (stages).
        AngleSharp.Dom.IElement cellRow1 = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => cellRow1.DoubleClick());
        AngleSharp.Dom.IElement input1 = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input1.Input("Alicia"));
        await cut.InvokeAsync(() => input1.Blur());

        // Sanity — the staged change applied to the row item already.
        seed[0].Name.Should().Be("Alicia");

        // Cancel via the toolbar button.
        AngleSharp.Dom.IElement cancelBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Discard changes");
        await cut.InvokeAsync(() => cancelBtn.Click());

        // Assert — original value restored from snapshot.
        seed[0].Name.Should().Be("Alice");
    }

    /// <summary>
    /// Regression: pressing Space in a cell editor used to abort the keystroke because the row
    /// handler (<see cref="BlazOrbit.Components.BOBDataCollectionBase{TItem,TComponent,TVariant}.HandleRowKeyDown"/>)
    /// intercepts <c>" "</c> and <c>"Enter"</c> to toggle row selection / fire <c>OnRowClick</c>,
    /// then sets <c>PreventRowKeyDown=true</c>, which Blazor wires to <c>preventDefault()</c> via
    /// the directive on the <c>&lt;tr&gt;</c>. With no <c>stopPropagation</c> on the editor's input,
    /// the keydown bubbled to the row, the row called <c>preventDefault</c>, and the browser never
    /// inserted the space character — so a name like "Alice Johnson" was impossible to type.
    ///
    /// The cell editor's input must <c>stopPropagation</c> on keydown so the row's handler never
    /// sees keystrokes that are bound for the editor.
    /// </summary>
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Cell_Editor_KeyDown_Does_Not_Bubble_To_Row_Click_Handler(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();
        int rowClicks = 0;

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Cell)
            .Add(c => c.OnRowClick, EventCallback.Factory.Create<Person>(this, _ => rowClicks++)));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Open the cell editor.
        AngleSharp.Dom.IElement targetCell = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => targetCell.DoubleClick());

        AngleSharp.Dom.IElement input = cut.Find("tbody tr td input");

        // Space: must reach the input alone (would insert a space in a real browser). The row's
        // " "-bound HandleRowClick must NOT fire.
        await cut.InvokeAsync(() => input.KeyDown(" "));
        rowClicks.Should().Be(0,
            "space typed in a cell editor must not bubble to the row's HandleRowKeyDown — the row " +
            "would otherwise call preventDefault, blocking the character entirely");

        // Enter: cell editor uses it to commit. The row's Enter-bound HandleRowClick must also NOT
        // fire — committing a cell should not also select the row.
        await cut.InvokeAsync(() => input.KeyDown("Enter"));
        rowClicks.Should().Be(0,
            "Enter typed in a cell editor commits the cell only; it must not double-fire as a row click");
    }

    /// <summary>
    /// Regression: any re-render of the parent <see cref="BOBDataGrid{TItem}"/> while a cell editor
    /// is open used to revert the editor's buffered value to the row item's original value because
    /// <see cref="_BOBDataGridCellEditor{TItem}"/> re-seeded its buffer from
    /// <c>Column.ValueSelector(Item)</c> on every <c>OnParametersSet</c>. The cell editor must
    /// preserve the user's typed buffer across parent re-renders that don't change the underlying
    /// row item.
    /// </summary>
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Cell_Editor_Buffer_Survives_Parent_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Cell));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        AngleSharp.Dom.IElement targetCell = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => targetCell.DoubleClick());

        // Type a partial value into the editor.
        AngleSharp.Dom.IElement input = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input.Input("Ali"));

        // Force the parent grid to re-render with the same logical params. Production code
        // hits this path whenever a sibling state change triggers grid re-render (sort,
        // filter, cascading param, etc.) — the editor's [Parameter] EventCallbacks are
        // re-created as new struct instances, which Blazor considers parameter changes,
        // which invokes OnParametersSet on the editor.
        cut.Render(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Cell));

        // Re-query the input after the re-render. Its value must still be "Ali",
        // not the row item's untouched "Alice".
        input = cut.Find("tbody tr td input");
        input.GetAttribute("value").Should().Be("Ali",
            "the editor must not re-seed its buffer from Column.ValueSelector(Item) on parent re-render");
    }

    /// <summary>
    /// Regression: the batch-mode Save button used to fire <c>OnRowSave</c> for every staged row
    /// regardless of whether any cell had an unresolved validator error. The cell editor's
    /// <c>OnValidate</c> callback correctly populated <c>_cellErrors</c> and the column's
    /// <c>ValueSetter</c> was blocked (the row item's property stayed unchanged), but the toolbar
    /// Save still invoked <c>OnRowSave</c> — the consumer's persistence layer believed the row
    /// was saved while the on-screen cell was still flagged invalid. The grid must NOT commit a
    /// batch while any cell has a pending validation error.
    /// </summary>
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BatchMode_Save_With_Validation_Errors_Does_Not_Fire_OnRowSave(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();
        List<Person> saved = [];

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns(withValidator: true))
            .Add(c => c.EditMode, BOBDataGridEditMode.Batch)
            .Add(c => c.OnRowSave, EventCallback.Factory.Create<Person>(this, p => { saved.Add(p); })));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Open Name cell on row 1 and clear the value (validator rejects empty).
        AngleSharp.Dom.IElement cellRow1 = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => cellRow1.DoubleClick());
        AngleSharp.Dom.IElement input = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input.Input(string.Empty));
        await cut.InvokeAsync(() => input.Blur());

        // Cell is flagged invalid + row item's Name unchanged (validator blocked the setter).
        cut.Markup.Should().Contain("bob-datagrid__cell--invalid");
        seed[0].Name.Should().Be("Alice");

        // Click toolbar Save while the error is unresolved.
        AngleSharp.Dom.IElement saveBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Save changes");
        await cut.InvokeAsync(() => saveBtn.Click());

        // OnRowSave must NOT have fired — the batch is not in a valid state to commit.
        saved.Should().BeEmpty(
            "the grid must refuse to commit a batch while any staged cell has an unresolved validator error");

        // The invalid styling must persist so the user knows what blocked the save.
        cut.Markup.Should().Contain("bob-datagrid__cell--invalid",
            "the invalid-cell styling must remain visible until the user resolves the error");
    }

    /// <summary>
    /// The toolbar Save button must visually signal that it cannot commit while validation errors
    /// exist — the <c>disabled</c> attribute on the underlying <c>&lt;button&gt;</c> is the
    /// accessibility/UI contract. This both prevents click-through races and gives the user a
    /// clear "fix the cell before saving" affordance.
    /// </summary>
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BatchMode_Save_Button_Disabled_When_Cells_Have_Validation_Errors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns(withValidator: true))
            .Add(c => c.EditMode, BOBDataGridEditMode.Batch));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        AngleSharp.Dom.IElement cellRow1 = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => cellRow1.DoubleClick());
        AngleSharp.Dom.IElement input = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input.Input(string.Empty));
        await cut.InvokeAsync(() => input.Blur());

        AngleSharp.Dom.IElement saveBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Save changes");

        saveBtn.HasAttribute("disabled").Should().BeTrue(
            "Save button must be disabled while there are unresolved cell validation errors");
    }

    /// <summary>
    /// Once the user resolves the validation error by typing a valid value, the Save button must
    /// re-enable so the batch can commit. This closes the loop on the disabled-while-invalid
    /// behavior.
    /// </summary>
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BatchMode_Save_Button_ReEnables_When_Errors_Resolved(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns(withValidator: true))
            .Add(c => c.EditMode, BOBDataGridEditMode.Batch));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Make the cell invalid first.
        AngleSharp.Dom.IElement cellRow1 = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => cellRow1.DoubleClick());
        AngleSharp.Dom.IElement input = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input.Input(string.Empty));
        await cut.InvokeAsync(() => input.Blur());

        // Then resolve it with a valid value (editor is still open after the validator failure).
        input = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input.Input("Alicia"));
        await cut.InvokeAsync(() => input.Blur());

        AngleSharp.Dom.IElement saveBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Save changes");

        saveBtn.HasAttribute("disabled").Should().BeFalse(
            "Save must re-enable once every cell error is resolved");
    }

    /// <summary>
    /// Regression: the batch-mode Save / Cancel buttons used <see cref="_BOBInBtn"/> parameters that
    /// don't exist (<c>Text</c>, <c>LeadingIcon</c>, <c>Color</c>) — they fell through to
    /// <c>CaptureUnmatchedValues</c> and rendered as inert HTML attributes, producing a button with
    /// no visible label and no icon. The button must render either readable text content or an SVG
    /// icon so users can identify it.
    /// </summary>
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BatchMode_Save_Button_Renders_Visible_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        List<Person> seed = NewSeed();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, seed)
            .Add(c => c.Columns, EditableColumns())
            .Add(c => c.EditMode, BOBDataGridEditMode.Batch));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Stage one change so the batch-actions toolbar appears.
        AngleSharp.Dom.IElement cellRow1 = cut.FindAll("tbody tr td")[0];
        await cut.InvokeAsync(() => cellRow1.DoubleClick());
        AngleSharp.Dom.IElement input1 = cut.Find("tbody tr td input");
        await cut.InvokeAsync(() => input1.Input("Alicia"));
        await cut.InvokeAsync(() => input1.Blur());

        AngleSharp.Dom.IElement saveBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Save changes");

        // The button must carry either non-empty text content or an icon (SVG child).
        string visibleText = saveBtn.TextContent.Trim();
        bool hasIcon = saveBtn.QuerySelector("svg") is not null;
        (visibleText.Length > 0 || hasIcon).Should().BeTrue(
            $"Save button must render text or an icon — got text='{visibleText}', svg={hasIcon}. " +
            "Check that _BOBInBtn parameters (Icon, ChildContent) are used instead of the non-existent LeadingIcon/Text.");
    }
}
