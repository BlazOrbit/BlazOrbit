using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

internal interface IPatternJsInterop
{
    ValueTask DisposePatternAsync(string componentId);

    ValueTask FocusFirstEditableAsync(string componentId);

    ValueTask FocusSpanAsync(string componentId, int index);

    ValueTask InitializePatternAsync(
        ElementReference containerBox,
        DotNetObjectReference<PatternCallbacksRelay> dotnetReference,
        string componentId);

    /// <summary>
    /// Returns <see langword="true"/> when <c>document.activeElement</c> is inside the
    /// pattern container identified by <paramref name="componentId"/>. Used by blur handling
    /// to differentiate "focus left the container" from "focus moved to a sibling span".
    /// </summary>
    ValueTask<bool> IsFocusInsideAsync(string componentId);

    ValueTask SelectSpanContentAsync(string componentId, int index);

    ValueTask SetCaretToEndAsync(string componentId, int index);

    ValueTask UpdateSpanValueAsync(string componentId, int index, string value);
}

internal sealed class PatternJsInterop
    : ModuleJsInteropBase, IPatternJsInterop
{
    public PatternJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.TextPattern)
    {
    }

    public async ValueTask DisposePatternAsync(string componentId)
    {
        // Dispose path: 4-tuple only — JSException is not swallowed here by AGENTS.md
        // contract. Failure during teardown signals an installation bug worth surfacing.
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "dispose",
            componentId);
    }

    public async ValueTask FocusFirstEditableAsync(string componentId)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync("focusFirstEditable", componentId);
    }

    public async ValueTask FocusSpanAsync(string componentId, int index)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            "focusSpan",
            componentId,
            index);
    }

    public async ValueTask InitializePatternAsync(
        ElementReference containerBox,
        DotNetObjectReference<PatternCallbacksRelay> dotnetReference,
        string componentId)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            "initialize",
            containerBox,
            dotnetReference,
            componentId);
    }

    public ValueTask<bool> IsFocusInsideAsync(string componentId)
    {
        // Direct `eval` rather than a TS export — keeping the JS bundle stable. The
        // expression is parameterised by `componentId` which is a Guid-derived literal
        // generated server-side, so quote injection is not a concern.
        return JsRuntime.InvokeAsync<bool>(
            "eval",
            $"document.activeElement?.closest('[data-bob-pattern-id=\"{componentId}\"]') !== null");
    }

    public async ValueTask SelectSpanContentAsync(string componentId, int index)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            "selectSpanContent",
            componentId,
            index);
    }

    public async ValueTask SetCaretToEndAsync(string componentId, int index)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            "setCaretToEnd",
            componentId,
            index);
    }

    public async ValueTask UpdateSpanValueAsync(string componentId, int index, string value)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            "updateSpanValue",
            componentId,
            index,
            value);
    }
}