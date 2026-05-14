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
    /// First-access module loader hardened against the 5-tuple of terminal failures:
    /// <see cref="JSDisconnectedException"/>, <see cref="ObjectDisposedException"/>,
    /// <see cref="InvalidOperationException"/>, <see cref="TaskCanceledException"/>, and the
    /// load-specific <see cref="JSException"/> (404, parse error, module-side throw). Returns
    /// <see langword="null"/> on failure so call sites can early-out cleanly instead of
    /// duplicating the try/catch boilerplate. Use this in preference to <c>await ModuleTask.Value</c>
    /// for non-disposal call paths; the dispose path intentionally swallows only the 4-tuple per
    /// the contract in <see cref="DisposeAsync"/>.
    /// </summary>
    protected async ValueTask<IJSObjectReference?> TryGetModuleAsync()
    {
        try
        {
            return await ModuleTask.Value;
        }
        catch (JSDisconnectedException) { return null; }
        catch (ObjectDisposedException) { return null; }
        catch (InvalidOperationException) { return null; }
        catch (TaskCanceledException) { return null; }
        catch (JSException) { return null; }
    }

    /// <summary>
    /// Asynchronously disposes of the resources used by the module.
    /// The following exceptions are swallowed intentionally — all four are raised on non-actionable
    /// teardown paths (prerender without a circuit, circuit shutdown, runtime disposal, or cancellation
    /// during the awaited module dispose).
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