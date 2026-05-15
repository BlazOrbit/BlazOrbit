using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// Base class for pattern components (date-time, masked input, etc.). Intentionally inherits
/// <see cref="ComponentBase"/> (not <c>BOBComponentBase</c>) because patterns emit their own
/// custom DOM layout (a container box with span children) rather than the <c>&lt;bob-component&gt;</c>
/// root contract. Hidden from IntelliSense via <see cref="EditorBrowsableAttribute"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BOBBasePattern : ComponentBase, IPatternJsCallback, IAsyncDisposable
{
    /// <summary>Reference to the container element rendered by the derived pattern markup.</summary>
    protected ElementReference _containerBox;
    private PatternCallbacksRelay? _jsCallbacksRelay;

    /// <summary>Current pattern state (span layout + values) maintained by the derived class.</summary>
    protected PatternState _patternState = new();
    private bool _isInitialized = false;
    private string? _lastExternalText = null;
    private bool _suppressRender = false;

    /// <summary>When <see langword="true" />, editable spans accept user input; <see langword="false"/> renders the pattern read-only.</summary>
    [Parameter]
    public bool Editable { get; set; } = true;

    /// <summary>Format string that defines the pattern (delegated to derived classes for parsing).</summary>
    [Parameter]
    [EditorRequired]
    public string Format { get; set; } = string.Empty;

    /// <summary>Current pattern value, two-way bound through <c>@bind-Text</c>. <see langword="null"/> means no complete value yet.</summary>
    [Parameter]
    public string? Text { get; set; } = null;

    /// <summary>Raised when <see cref="Text"/> changes after the pattern reaches a complete state.</summary>
    [Parameter]
    public EventCallback<string?> TextChanged { get; set; }

    /// <summary>Raised whenever the dirty state of the pattern flips (any span has been edited since the last reset).</summary>
    [Parameter]
    public EventCallback<bool> OnDirtyStateChanged { get; set; }

    /// <summary>Stable identifier used to scope the JS interop side of this pattern instance.</summary>
    protected string ComponentId { get; } = $"pattern_{Guid.NewGuid():N}";

    [Inject] private IPatternJsInterop Js { get; set; } = default!;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_jsCallbacksRelay != null && _isInitialized)
        {
            try { await Js.DisposePatternAsync(ComponentId); }
            catch (Microsoft.JSInterop.JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (TaskCanceledException) { }

            _jsCallbacksRelay.Dispose();
        }

        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>Focuses the first editable span in the pattern.</summary>
    public async Task FocusAsync()
    {
        if (_isInitialized)
        {
            await Js.FocusFirstEditableAsync(ComponentId);
        }
    }

    /// <summary>JS callback for the container paste event. Accepts the pasted text when it validates as a complete value.</summary>
    public async Task OnPaste(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // Try to validate complete text
        string normalized = NormalizeSeparators(text);

        if (ValidateComplete(normalized))
        {
            InitializeFromText(normalized);
            await NotifyTextChanged();
            StateHasChanged();
        }
    }

    /// <summary>JS callback fired when an editable span loses focus.</summary>
    public async Task OnSpanBlur(int index)
    {
        if (!IsValidIndex(index))
        {
            return;
        }

        SpanState span = _patternState.Spans[index];

        if (span.IsEditable && (!span.IsComplete || string.IsNullOrEmpty(span.Value)))
        {
            span.Value = string.Empty;
            await Js.UpdateSpanValueAsync(ComponentId, index, span.Placeholder);
            _suppressRender = true;
            await NotifyTextChanged();
        }
    }

    /// <summary>JS callback fired when an editable span has been filled. Returns whether the value passed validation.</summary>
    public async Task<bool> OnSpanComplete(int index, string value)
    {
        if (!IsValidIndex(index))
        {
            return false;
        }

        SpanState span = _patternState.Spans[index];
        bool isValid = span.Validator?.Invoke(value) ?? true;

        if (!isValid)
        {
            span.Value = string.Empty;
            await Js.UpdateSpanValueAsync(ComponentId, index, span.Placeholder);
            await Js.SelectSpanContentAsync(ComponentId, index);
            _suppressRender = true;
            await NotifyTextChanged();
            return false;
        }

        _suppressRender = true;
        await NotifyTextChanged();
        return true;
    }

    /// <summary>JS callback fired when an editable span receives focus.</summary>
    public async Task OnSpanFocus(int index)
    {
        if (!IsValidIndex(index))
        {
            return;
        }
    }

    /// <summary>JS callback fired on every keystroke inside an editable span.</summary>
    public async Task OnSpanInput(int index, string value)
    {
        if (!IsValidIndex(index))
        {
            return;
        }

        SpanState span = _patternState.Spans[index];
        if (!span.IsEditable)
        {
            return;
        }

        string filtered = FilterInput(value, span.AllowedChars, span.MaxLength);

        if (filtered != value)
        {
            await Js.UpdateSpanValueAsync(ComponentId, index, filtered);
            await Js.SetCaretToEndAsync(ComponentId, index);
        }

        span.Value = filtered;

        // Suprimir render durante la notificación
        _suppressRender = true;
        await NotifyTextChanged();
    }

    /// <summary>JS callback for click events on toggle-style spans (e.g. AM/PM).</summary>
    public async Task OnToggleClick(int index)
    {
        if (!IsValidIndex(index))
        {
            return;
        }

        SpanState span = _patternState.Spans[index];
        if (!span.IsToggle)
        {
            return;
        }

        string newValue = ToggleValue(span.Value, span.Placeholder);
        span.Value = newValue;

        await Js.UpdateSpanValueAsync(ComponentId, index, newValue);

        _suppressRender = true;
        await NotifyTextChanged();
    }

    /// <summary>Builds the initial <see cref="PatternState"/> (span layout) for the derived pattern.</summary>
    protected abstract PatternState CreatePatternState();

    /// <summary>Hook for derived classes to add their own async disposal work.</summary>
    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

    /// <summary>Hydrates <see cref="_patternState"/> from a complete text value.</summary>
    protected abstract void InitializeFromText(string? text);

    /// <summary>Allows derived classes to normalize separators inside pasted text before validation.</summary>
    protected virtual string NormalizeSeparators(string text)
        // Default implementation - can be overridden
        => text;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsCallbacksRelay = new PatternCallbacksRelay(this);
            await Js.InitializePatternAsync(_containerBox, _jsCallbacksRelay.DotNetReference, ComponentId);
            _isInitialized = true;
        }
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _patternState = CreatePatternState();
        InitializeFromText(Text);
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        // Detectar si Text cambió desde el exterior
        if (_isInitialized && Text != _lastExternalText)
        {
            _lastExternalText = Text;

            // Solo actualizar si el valor es diferente al estado interno
            if (Text != _patternState.GetActualText())
            {
                InitializeFromText(Text);
                await SyncSpansToJs();
            }
        }
    }

    /// <inheritdoc />
    protected override bool ShouldRender()
    {
        if (_suppressRender)
        {
            _suppressRender = false;
            return false;
        }

        return true;
    }

    /// <summary>Cycles the value of a toggle span. Derived patterns override to provide the cycle order.</summary>
    protected virtual string ToggleValue(string currentValue, string placeholder)
        // Implementación por defecto - puede ser override
        => currentValue;

    /// <summary>Returns whether <paramref name="text"/> represents a complete value for this pattern.</summary>
    protected abstract bool ValidateComplete(string text);

    private string FilterInput(string input, string allowedChars, int maxLength)
    {
        System.Text.StringBuilder result = new();

        foreach (char c in input)
        {
            if (result.Length >= maxLength)
            {
                break;
            }

            bool valid = allowedChars switch
            {
                "d" => char.IsDigit(c),
                "w" => char.IsLetter(c),
                "a" => char.IsLetterOrDigit(c),
                _ => true
            };

            if (valid)
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    private bool IsValidIndex(int index)
        => index >= 0 && index < _patternState.Spans.Count;

    private async Task NotifyTextChanged()
    {
        string? actualText = _patternState.IsComplete ? _patternState.GetActualText() : null;

        if (Text != actualText)
        {
            Text = actualText;
            _lastExternalText = actualText;
            await TextChanged.InvokeAsync(Text);
        }

        await OnDirtyStateChanged.InvokeAsync(_patternState.IsDirty);
    }

    private async Task SyncSpansToJs()
    {
        foreach (SpanState span in _patternState.Spans)
        {
            if (span.IsEditable)
            {
                await Js.UpdateSpanValueAsync(ComponentId, span.Index, span.DisplayValue);
            }
        }
    }
}