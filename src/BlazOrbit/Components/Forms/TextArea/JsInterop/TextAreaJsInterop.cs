using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Forms;

public interface ITextAreaJsInterop
{
    ValueTask DisposeAutoResizeAsync(string textareaId);

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