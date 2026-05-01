namespace BlazOrbit.Components.Layout.Services;

/// <summary>Coordinates dialog and drawer modals — opening, closing, and stack ordering.</summary>
public interface IModalService
{
    /// <summary>Raised whenever the active-modals list changes; awaited sequentially.</summary>
    event Func<Task>? OnChangeAsync;

    /// <summary>Currently open modals, in stacking order (last is topmost).</summary>
    IReadOnlyList<ModalState> ActiveModals { get; }

    /// <summary>Closes every active modal in reverse order.</summary>
    Task CloseAllAsync();

    /// <summary>Closes the topmost modal.</summary>
    Task CloseAsync();

    /// <summary>Opens a dialog and awaits a typed result.</summary>
    Task<TResult?> ShowDialogAsync<TComponent, TResult>(
                object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent;

    /// <summary>Opens a dialog without awaiting a result.</summary>
    Task ShowDialogAsync<TComponent>(
        object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent;

    /// <summary>Opens a drawer and awaits a typed result.</summary>
    Task<TResult?> ShowDrawerAsync<TComponent, TResult>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent;

    /// <summary>Opens a drawer without awaiting a result.</summary>
    Task ShowDrawerAsync<TComponent>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent;
}

/// <summary>Default <see cref="IModalService"/> implementation registered by <c>AddBlazOrbit()</c>.</summary>
public sealed class ModalService : IModalService
{
    private readonly List<ModalState> _modals = [];

    /// <inheritdoc />
    public event Func<Task>? OnChangeAsync;

    /// <inheritdoc />
    public IReadOnlyList<ModalState> ActiveModals => _modals.AsReadOnly();

    /// <inheritdoc />
    public async Task CloseAllAsync()
    {
        while (_modals.Count > 0)
        {
            await CloseAsync();
        }
    }

    /// <inheritdoc />
    public async Task CloseAsync()
    {
        if (_modals.Count == 0)
        {
            return;
        }

        ModalState current = _modals[^1];
        await CloseModalAsync(current);
    }

    /// <inheritdoc />
    public async Task<TResult?> ShowDialogAsync<TComponent, TResult>(
                object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Dialog,
            parameters,
            options ?? new DialogOptions());

        return await ShowAndWaitAsync<TResult>(state);
    }

    /// <inheritdoc />
    public Task ShowDialogAsync<TComponent>(
        object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Dialog,
            parameters,
            options ?? new DialogOptions());

        return ShowAsync(state);
    }

    /// <inheritdoc />
    public async Task<TResult?> ShowDrawerAsync<TComponent, TResult>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Drawer,
            parameters,
            options ?? new DrawerOptions());

        return await ShowAndWaitAsync<TResult>(state);
    }

    /// <inheritdoc />
    public Task ShowDrawerAsync<TComponent>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Drawer,
            parameters,
            options ?? new DrawerOptions());

        return ShowAsync(state);
    }

    private async Task CloseModalAsync(ModalState state)
    {
        state.IsAnimatingOut = true;
        await NotifyChangeAsync();

        await Task.Delay(200);

        _modals.Remove(state);
        ShowPreviousModal();
        await NotifyChangeAsync();
    }

    private ModalState CreateModalState<TComponent>(
            ModalType type,
        object? parameters,
        ModalOptionsBase options)
        where TComponent : IModalContent
    {
        string id = $"modal-{Guid.NewGuid():N}";
        ModalReference reference = new(id, OnModalCloseAsync);

        Dictionary<string, object?>? paramDict = null;
        if (parameters != null)
        {
            paramDict = parameters.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));
        }

        return new ModalState
        {
            Id = id,
            Type = type,
            ComponentType = typeof(TComponent),
            Reference = reference,
            Options = options,
            Parameters = paramDict
        };
    }

    private void HideCurrentModal()
    {
        if (_modals.Count > 0)
        {
            _modals[^1].IsVisible = false;
        }
    }

    // Awaits each subscriber sequentially. Multiple subscribers run in registration
    // order, and an exception from one stops propagation — by design: a misbehaving
    // host should surface, not get swallowed.
    private async Task NotifyChangeAsync()
    {
        Func<Task>? handler = OnChangeAsync;
        if (handler is null)
        {
            return;
        }

        foreach (Func<Task> subscriber in handler.GetInvocationList().Cast<Func<Task>>())
        {
            await subscriber();
        }
    }

    private async Task OnModalCloseAsync(ModalReference reference)
    {
        ModalState? state = _modals.FirstOrDefault(m => m.Reference == reference);
        if (state != null)
        {
            await CloseModalAsync(state);
        }
    }

    private async Task<TResult?> ShowAndWaitAsync<TResult>(ModalState state)
    {
        HideCurrentModal();
        _modals.Add(state);
        await NotifyChangeAsync();

        try
        {
            object? result = await state.Reference.Result;
            return result is TResult typed ? typed : default;
        }
        catch (TaskCanceledException)
        {
            return default;
        }
    }

    private async Task ShowAsync(ModalState state)
    {
        HideCurrentModal();
        _modals.Add(state);
        await NotifyChangeAsync();
    }

    private void ShowPreviousModal()
    {
        if (_modals.Count > 0)
        {
            _modals[^1].IsVisible = true;
        }
    }
}
