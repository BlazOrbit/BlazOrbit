using BlazOrbit.Components.Layout.Services;

namespace BlazOrbit.Components.Layout;

/// <summary>Default <see cref="IConfirmService"/> implementation auto-registered by <c>AddBlazOrbit()</c>.</summary>
public sealed class ConfirmService : IConfirmService
{
    private readonly IModalService _modal;

    /// <summary>Initializes a new <see cref="ConfirmService"/>.</summary>
    public ConfirmService(IModalService modal) => _modal = modal;

    /// <inheritdoc />
    public async Task<bool> AskAsync(
        string title,
        string message,
        ConfirmSeverity severity = ConfirmSeverity.Info,
        string yesLabel = "Yes",
        string noLabel = "Cancel",
        TimeSpan? timeout = null)
    {
        // The modal returns a non-null bool only when the user explicitly accepts or cancels;
        // overlay/escape dismissals come back as null which we treat as "no". This keeps the
        // service caller-side simple: bool ok = await Confirm.AskAsync(...); if (!ok) return.
        Task<bool> dialogTask = _modal.ShowDialogAsync<BOBConfirmDialog, bool>(
            new
            {
                Title = title,
                Message = message,
                Severity = severity,
                YesLabel = yesLabel,
                NoLabel = noLabel,
            },
            new DialogOptions
            {
                Title = title,
                CloseOnOverlayClick = false,
                CloseOnEscape = true,
                MinWidth = "22rem",
                MaxWidth = "32rem",
            });

        if (timeout is null)
        {
            return await dialogTask;
        }

        // RACE: the user finishes before the deadline OR the deadline elapses first. Whichever
        // wins, we trigger CloseAsync to drop the dialog if it is still open. Don't await the
        // timer task itself when the dialog wins; let it expire silently.
        Task delay = Task.Delay(timeout.Value);
        Task winner = await Task.WhenAny(dialogTask, delay);
        if (winner == delay)
        {
            await _modal.CloseAsync();
            return false;
        }

        return await dialogTask;
    }
}
