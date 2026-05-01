using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Forms;

/// <summary>JS interop contract for the textarea auto-resize module.</summary>
public interface ITextAreaJsInterop
{
    /// <summary>Detaches and disposes the auto-resize behavior previously attached via <see cref="InitializeAutoResizeAsync"/>.</summary>
    ValueTask DisposeAutoResizeAsync(string textareaId);

    /// <summary>Attaches an auto-resize behavior to the given textarea element.</summary>
    ValueTask InitializeAutoResizeAsync(ElementReference textarea, string textareaId);
}

internal sealed class TextAreaJsInterop : ModuleJsInteropBase, ITextAreaJsInterop
{
    public TextAreaJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.TextArea)
    {
    }

    public async ValueTask DisposeAutoResizeAsync(string textareaId)
    {
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("dispose", textareaId);
    }

    public async ValueTask InitializeAutoResizeAsync(ElementReference textarea, string textareaId)
    {
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("initialize", textarea, textareaId);
    }
}