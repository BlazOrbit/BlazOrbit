namespace BlazOrbit.Components.Layout.Services;

public interface IModalService
{
    event Func<Task>? OnChangeAsync;

    IReadOnlyList<ModalState> ActiveModals { get; }

    Task CloseAllAsync();

    Task CloseAsync();

    Task<TResult?> ShowDialogAsync<TComponent, TResult>(
                object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent;

    Task ShowDialogAsync<TComponent>(
        object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent;

    Task<TResult?> ShowDrawerAsync<TComponent, TResult>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent;

    Task ShowDrawerAsync<TComponent>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent;
}

public sealed class ModalService : IModalService
{
    private readonly List<ModalState> _modals = [];

    public event Func<Task>? OnChangeAsync;

    public IReadOnlyList<ModalState> ActiveModals => _modals.AsReadOnly();

    public async Task CloseAllAsync()
    {
        while (_modals.Count > 0)
        {
            await CloseAsync();
        }
    }

    public async Task CloseAsync()
    {
        if (_modals.Count == 0)
        {
            return;
        }

        ModalState current = _modals[^1];
        await CloseModalAsync(current);
    }

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
