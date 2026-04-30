using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace BlazOrbit.Utilities;

/// <summary>
/// Helper for fire-and-forget async operations in Blazor UI contexts.
///
/// Use <see cref="SafeFireAndForget"/> when an async operation must be started
/// from a synchronous callback (e.g. an <c>event Action?</c> handler) where
/// awaiting is impossible. The helper suppresses the four canonical teardown
/// exceptions and logs anything unexpected so it does not end up in
/// <see cref="TaskScheduler.UnobservedTaskException"/>.
///
/// Prefer awaiting directly whenever the calling context is already async;
/// this helper is a last resort for sync→async bridges.
/// </summary>
public static class BOBAsyncHelper
{
    /// <summary>
    /// Fires <paramref name="action"/> without awaiting, catching the four
    /// non-actionable teardown exceptions silently and logging anything else.
    /// </summary>
    /// <param name="action">Async work to run.</param>
    /// <param name="logger">Optional logger for unexpected failures.</param>
    public static void SafeFireAndForget(Func<Task> action, ILogger? logger = null) => _ = RunSafeAsync(action, logger);

    private static async Task RunSafeAsync(Func<Task> action, ILogger? logger)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            // Expected when the owning component/service is torn down before
            // the task completes.
        }
        catch (JSDisconnectedException)
        {
            // Expected when the Blazor circuit disconnects mid-call.
        }
        catch (ObjectDisposedException)
        {
            // Expected when the component or CTS is disposed while the task
            // is still scheduled or running.
        }
        catch (InvalidOperationException)
        {
            // Expected during prerender or when the renderer is no longer
            // attached (e.g. InvokeAsync after unmount).
        }
        catch (Exception ex)
        {
            if (logger != null)
            {
                logger.LogError(ex, "Fire-and-forget task failed");
            }
            else
            {
                Debug.Fail($"Fire-and-forget task failed: {ex}");
            }
        }
    }
}
