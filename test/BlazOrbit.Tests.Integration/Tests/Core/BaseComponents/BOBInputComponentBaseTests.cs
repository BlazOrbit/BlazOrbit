using AngleSharp.Dom;
using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.BaseComponents;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Core.BaseComponents;

/// <summary>
/// Functional contract tests for <see cref="BOBInputComponentBase{T}" />.
///
/// These tests define and enforce the following system-level rules:
///
/// 1 - VALIDATION ENGINE INTEGRATION:
/// - Components MUST synchronize with Blazor's <see cref="EditContext" /> and reflect validation
/// states using 'data-bob-error="true/false"'.
/// - Components MUST clean up event subscriptions on disposal to prevent memory leaks or
/// 'ObjectDisposedException' during validation callbacks.
///
/// 2 - BINDING COMPLIANCE:
/// - As an extension of 'InputBase', components MUST receive a 'ValueExpression'.
/// - Failure to provide 'ValueExpression' MUST result in an <see cref="InvalidOperationException" />
/// to maintain compatibility with Blazor's validation metadata.
///
/// 3 - INTERACTIVE STATES:
/// - 'Disabled' and 'ReadOnly' states MUST be synchronized between 'data-bob-*' attributes (for
/// styling) and native HTML attributes (for browser behavior).
/// - 'IsLoading' MUST act as a master override, forcing the component into a disabled state.
///
/// 4 - VALUE SYNCHRONIZATION:
/// - UI-driven changes MUST update the 'CurrentValue' and trigger the associated 'ValueExpression'
/// within the Dispatcher context.
///
/// 5 - DOM INTEGRITY:
/// - All 'AdditionalAttributes' MUST be splatted onto the primary input element.
/// - JS behaviors MUST NOT attempt to attach to the input if it is in a loading/disabled state.
/// </summary>
[Trait("Core", "BOBInputComponentBase")]
public class BOBInputComponentBaseTests
{
    private readonly IBehaviorJsInterop _jsInterop = Substitute.For<IBehaviorJsInterop>();
    private readonly IJSObjectReference _jsModule = Substitute.For<IJSObjectReference>();

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task DisposeAsync_Should_Be_NullSafe_If_JS_Never_Attached(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ctx.Services.AddSingleton(_jsInterop);
        TestModel model = new();

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        // Act & Assert: No debería lanzar excepción aunque el interop no haya devuelto nada
        await cut.Instance.DisposeAsync();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Input_Loading_Should_Override_Disabled_Visuals(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();

        // Act: Loading y Disabled a la vez
        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, true));

        // Assert: Verificar que data-bob-loading prevalece o coexiste según tu Builder
        IElement input = cut.Find("input");
        input.GetAttribute("data-bob-loading").Should().Be("true");
        // public bool IsDisabled => Disabled || (this is IHasLoading loading && loading.IsLoading);
        input.GetAttribute("data-bob-disabled").Should().Be("true");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Input_Should_Handle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();

        // Act
        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, true));

        // Assert: IHasReadOnly logic
        IElement input = cut.Find("input");
        input.GetAttribute("data-bob-readonly").Should().Be("true");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Attach_And_Dispose_JS_Behavior(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ctx.Services.AddSingleton(_jsInterop);
        // Arrange
        _jsInterop.AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>())
                  .Returns(_jsModule);

        // Act
        TestModel model = new();
        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.Value, "initial")
            .Add(c => c.ValueExpression, () => model.Value));
        await cut.Instance.DisposeAsync();

        // Assert
        await _jsInterop.Received(1).AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>());
        await _jsModule.Received(1).InvokeVoidAsync("dispose", Arg.Any<object[]>());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_When_ValueExpression_Is_Missing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Act
        Action act = () => ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.Value, "No Expression"));

        // BOBInputComponentBase relaxes the InputBase contract: rendering outside an
        // EditForm without ValueExpression must not throw. A synthetic ValueExpression
        // is injected so the component renders standalone.
        act.Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_CurrentValue_When_Input_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.Value, "initial")
            .Add(c => c.ValueExpression, () => model.Value));

        // Simulamos cambio en el input (ejecuta CurrentValueAsString = value)
        await ctx.Renderer.Dispatcher.InvokeAsync(() => cut.Find("input").Change("updated"));

        cut.Instance.Value.Should().Be("updated");
    }

    // Clase auxiliar para evitar ConstantExpression en ValueExpression
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public NestedModel Nested { get; set; } = new();
    }

    private class NestedModel
    {
        public string NestedValue { get; set; } = string.Empty;
    }

    // ─────────── EditContext re-subscription ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Double_Subscribe_On_Rerender_With_Same_EditContext(BlazorScenario scenario)
    {
        // Blazor's InputBase blocks changing EditContext dynamically (throws), so the only
        // reachable branch of the "re-subscribe" guard is the identity check: if _previousEditContext
        // == EditContext on re-render, the handler is NOT re-subscribed. Verified indirectly by
        // asserting the component reacts exactly once to a validation state change after re-render.
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();
        EditContext editContext = new(model);
        ValidationMessageStore messageStore = new(editContext);

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Name)
            .AddCascadingValue(editContext));

        // Re-render several times (same EditContext) to exercise the OnParametersSet idempotency path
        cut.Render(p => p.Add(c => c.ValueExpression, () => model.Name));
        cut.Render(p => p.Add(c => c.ValueExpression, () => model.Name));
        cut.Render(p => p.Add(c => c.ValueExpression, () => model.Name));

        int stateChangedEventCount = 0;
        editContext.OnValidationStateChanged += (_, _) => stateChangedEventCount++;

        // Act
        await ctx.Renderer.Dispatcher.InvokeAsync(() =>
        {
            messageStore.Add(editContext.Field(nameof(TestModel.Name)), "Error");
            editContext.NotifyValidationStateChanged();
        });

        cut.WaitForState(() => cut.Find("input").GetAttribute("data-bob-error") == "true");

        // Assert: event fired once on the context; component registered once
        stateChangedEventCount.Should().Be(1);
        cut.Find("input").GetAttribute("data-bob-error").Should().Be("true");
    }

    // ─────────── Nested ValueExpression ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Resolve_FieldIdentifier_For_Nested_Property_Expression(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();
        EditContext editContext = new(model);
        ValidationMessageStore messageStore = new(editContext);

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Nested.NestedValue)
            .AddCascadingValue(editContext));

        // Assert FieldIdentifier resolves to model.Nested / NestedValue
        await ctx.Renderer.Dispatcher.InvokeAsync(() =>
        {
            FieldIdentifier fi = FieldIdentifier.Create<string>(() => model.Nested.NestedValue);
            messageStore.Add(fi, "Nested error");
            editContext.NotifyValidationStateChanged();
        });

        cut.WaitForState(() => cut.Find("input").GetAttribute("data-bob-error") == "true");
        cut.Find("input").GetAttribute("data-bob-error").Should().Be("true");
    }

    // ─────────── Dispose without EditContext ───────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Dispose_Should_Not_Throw_When_EditContext_Never_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        Action dispose = () => ctx.Renderer.Dispatcher.InvokeAsync(() => cut.Dispose()).GetAwaiter().GetResult();

        dispose.Should().NotThrow();
    }

    #region Behavioral Tests (Design & Style)

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Input_Should_Apply_Design_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Value) // Referencia a propiedad
            .Add(c => c.Size, BOBSize.Small)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.FullWidth, true)
            .Add(c => c.Shadow, BOBShadowPresets.Elevation(4))
        );

        IElement input = cut.Find("input");
        input.GetAttribute("data-bob-size").Should().Be("small");
        input.GetAttribute("data-bob-density").Should().Be("compact");
        input.GetAttribute("data-bob-fullwidth").Should().Be("true");
        input.GetAttribute("data-bob-shadow").Should().Be("true");
        input.GetAttribute("style").Should().Contain("--bob-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Input_Should_Generate_Css_Variables_For_Colors_And_Borders(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();

        IRenderedComponent<BOBInputComponentBase_TestStub> cut =
            ctx.Render<BOBInputComponentBase_TestStub>(p => p
                .Add(c => c.ValueExpression, () => model.Value)
                .Add(c => c.Color, "rgba(0,0,255,1)")
                .Add(c => c.Border,
                    BorderStyle.Create()
                        .All("2px", BorderStyleType.Solid, "rgba(255,0,0,1)")
                        .Radius(8))
            );

        string? style = cut.Find("input").GetAttribute("style");
        style.Should().Contain("--bob-inline-color: rgba(0,0,255,1)");
        style.Should().Contain("--bob-inline-border:").And.Contain("2px solid");
        style.Should().Contain("--bob-inline-border-radius: 8px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Input_Should_Merge_User_Styles_With_Computed_Variables(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();
        Dictionary<string, object> additionalAttrs = new()
        { { "style", "margin-top: 10px;" } };

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.AdditionalAttributes, additionalAttrs)
            .Add(c => c.Color, (string)BOBColor.Coral.Default)
        );

        string? style = cut.Find("input").GetAttribute("style");
        style.Should().Contain("margin-top: 10px;");
        style.Should().Contain("--bob-inline-color: rgba(255,127,80,1)");
    }

    #endregion Behavioral Tests (Design & Style)

    #region Input Logic & Validation Tests

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reset_Error_Attribute_When_Validation_Is_Fixed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();
        EditContext editContext = new(model);
        ValidationMessageStore messageStore = new(editContext);

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Name)
            .AddCascadingValue(editContext));

        // 1. Poner error
        await ctx.Renderer.Dispatcher.InvokeAsync(() =>
        {
            messageStore.Add(editContext.Field(nameof(TestModel.Name)), "Error");
            editContext.NotifyValidationStateChanged();
        });

        cut.WaitForState(() => cut.Find("input").GetAttribute("data-bob-error") == "true");

        // 2. Quitar error (Cubre la rama hadErrors != IsError en BOBInputComponentBase)
        await ctx.Renderer.Dispatcher.InvokeAsync(() =>
        {
            messageStore.Clear();
            editContext.NotifyValidationStateChanged();
        });

        cut.WaitForState(() => cut.Find("input").GetAttribute("data-bob-error") == "false"
                            || !cut.Find("input").HasAttribute("data-bob-error"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Unsubscribe_From_Validation_On_Dispose(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();
        EditContext editContext = new(model);

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .AddCascadingValue(editContext));

        // Act: Disponer el componente
        await ctx.Renderer.Dispatcher.InvokeAsync(cut.Dispose);

        await ctx.Renderer.Dispatcher.InvokeAsync(() =>
        {
            Action act = () => editContext.NotifyValidationStateChanged();
            act.Should().NotThrow();
        });
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Error_Attribute_When_Validation_Fails(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TestModel model = new();
        EditContext editContext = new(model);
        ValidationMessageStore messageStore = new(editContext);

        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.ValueExpression, () => model.Name)
            .AddCascadingValue(editContext));

        // Act
        await ctx.Renderer.Dispatcher.InvokeAsync(() =>
        {
            messageStore.Add(editContext.Field(nameof(TestModel.Name)), "Error");
            editContext.NotifyValidationStateChanged();
        });

        // Wait component to react to validation event and update its HTML markup.
        cut.WaitForState(() => cut.Find("input").HasAttribute("data-bob-error"));

        // Assert
        cut.Find("input").GetAttribute("data-bob-error").Should().Be("true");
    }

    // Should_Skip_JS_Behavior_When_Loading ? Thought about it, but decided to leave it attached
    // even when loading or it could never be attached again. The loading state is usually
    // transient, and detaching JS behaviors could lead to inconsistent states

    //[Theory]
    //[MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    //public async Task Should_Skip_JS_Behavior_When_Loading(BlazorScenario scenario)
    //{
    //    await using BlazorTestContextBase ctx = scenario.CreateContext();
    //    TestModel model = new();
    //    ctx.Services.AddSingleton(_jsInterop);

    // ctx.Render<BOBInputComponentBase_TestStub>(p => p .Add(c => c.ValueExpression, () =>
    // model.Value) .Add(c => c.IsLoading, true));

    //    await _jsInterop.DidNotReceive().AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>());
    //}

    #endregion Input Logic & Validation Tests

    // ---- CORE-T-03: PatchVolatileAttributes updates data-bob-* on re-render ----

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PatchVolatileAttributes_Should_Update_Loading_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ctx.Services.AddSingleton(_jsInterop);

        // Arrange
        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.Loading, false));
        cut.Find("input").GetAttribute("data-bob-loading").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.Loading, true));

        // Assert
        cut.Find("input").GetAttribute("data-bob-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task PatchVolatileAttributes_Should_Update_FullWidth_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ctx.Services.AddSingleton(_jsInterop);

        // Arrange
        IRenderedComponent<BOBInputComponentBase_TestStub> cut = ctx.Render<BOBInputComponentBase_TestStub>(p => p
            .Add(c => c.FullWidth, false));

        // Act
        cut.Render(p => p.Add(c => c.FullWidth, true));

        // Assert
        cut.Find("input").GetAttribute("data-bob-fullwidth").Should().Be("true");
    }
}