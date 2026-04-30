namespace BlazOrbit.Components.Layout;

/// <summary>
/// Represents a reference to an open modal, allowing programmatic control over its lifecycle.
/// </summary>
public sealed class ModalReference
{
    private readonly Func<ModalReference, Task> _onCloseAsync;
    // RunContinuationsAsynchronously: prevent awaiter continuations from running inline on the
    // CloseAsync caller thread. Otherwise a costly awaiter of Result would block whoever closed
    // the modal.
    private readonly TaskCompletionSource<object?> _resultSource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal ModalReference(string id, Func<ModalReference, Task> onCloseAsync)
    {
        Id = id;
        _onCloseAsync = onCloseAsync;
    }

    /// <summary>
    /// The unique identifier of the modal.
    /// </summary>
    public string Id { get; }

    internal Task<object?> Result => _resultSource.Task;

    /// <summary>
    /// Closes the modal without a result.
    /// </summary>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    public async Task CloseAsync()
    {
        _resultSource.TrySetResult(null);
        await _onCloseAsync(this);
    }

    /// <summary>
    /// Closes the modal and returns the specified result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="result">The result to return.</param>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    public async Task CloseAsync<TResult>(TResult result)
    {
        _resultSource.TrySetResult(result);
        await _onCloseAsync(this);
    }

    internal void Cancel() => _resultSource.TrySetCanceled();
}
