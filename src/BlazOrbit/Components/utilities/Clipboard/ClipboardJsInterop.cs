using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.JSInterop;

namespace BlazOrbit.Components;

/// <summary>JS interop contract for clipboard operations.</summary>
public interface IClipboardJsInterop
{
    /// <summary>Copies <paramref name="text"/> to the system clipboard.</summary>
    ValueTask CopyTextAsync(string text);

    /// <summary>
    /// Writes both a <c>text/plain</c> and a <c>text/html</c> payload to the system clipboard.
    /// Spreadsheet apps (Excel, Sheets, Numbers) prefer the HTML envelope for cell-type
    /// inference; plain editors take the text variant. Falls back to <see cref="CopyTextAsync"/>
    /// on browsers without the async <c>ClipboardItem</c> API.
    /// </summary>
    /// <param name="text">Plain-text payload (typically TSV).</param>
    /// <param name="html">HTML payload (typically a <c>&lt;table&gt;</c> envelope).</param>
    ValueTask CopyRichAsync(string text, string html);
}

internal sealed class ClipboardJsInterop
    : ModuleJsInteropBase, IClipboardJsInterop
{
    public ClipboardJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Clipboard)
    {
    }

    public async ValueTask CopyTextAsync(string text)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync("copyText", text);
    }

    public async ValueTask CopyRichAsync(string text, string html)
    {
        IJSObjectReference? module = await TryGetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync("copyRich", text, html);
    }
}