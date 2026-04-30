using Microsoft.JSInterop;

namespace BlazOrbit.Abstractions;

/// <summary>
/// Represents a base class for JavaScript interop modules that implement the IAsyncDisposable interface.
/// </summary>
public abstract class ModuleJsInteropBase : IAsyncDisposable
{
    /// <summary>
    /// Represents an interface for invoking JavaScript code from .NET code.
    /// </summary>
    protected readonly IJSRuntime JsRuntime;

    /// <summary>
    /// A lazy asynchronous task that represents a JavaScript object reference.
    /// </summary>
    protected readonly Lazy<Task<IJSObjectReference>> ModuleTask;

    /// <summary>
    /// Initializes a new instance of the ModuleJsInterop class.
    /// </summary>
    /// <param name="jsRuntime">
    /// The JavaScript runtime.
    /// </param>
    /// <param name="jsModuleContentPath">
    /// The path to the JavaScript module content.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when jsRuntime is null.
    /// </exception>
    public ModuleJsInteropBase(IJSRuntime jsRuntime, string jsModuleContentPath)
    {
        JsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        ModuleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", jsModuleContentPath).AsTask());
    }

    /// <summary>
    /// Asynchronously disposes of the resources used by the module.
    /// The following exceptions are swallowed intentionally — all four are raised on non-actionable
    /// teardown paths (prerender without a circuit, circuit shutdown, runtime disposal, or cancellation
    /// during the awaited module dispose). See CLAUDE.md §"Async / JS interop conventions".
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask" /> representing the asynchronous operation.
    /// </returns>
    public virtual async ValueTask DisposeAsync()
    {
        if (!ModuleTask.IsValueCreated)
        {
            return;
        }

        try
        {
            IJSObjectReference module = await ModuleTask.Value;
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // (Blazor Server) Circuit disconnected, module already disposed by browser.
        }
        catch (ObjectDisposedException)
        {
            // Runtime already disposed.
        }
        catch (InvalidOperationException)
        {
            // No active JS runtime (prerender / server shutdown).
        }
        catch (TaskCanceledException)
        {
            // Disposal raced with an in-flight invoke.
        }
    }
}