using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components.Forms;

/// <summary>
/// Registration-only descriptor for a <c>BOBInputRadio</c> option. Intentionally inherits
/// <see cref="ComponentBase"/> (not <c>BOBComponentBase</c>) because it emits no DOM of its own —
/// the parent radio container renders the options from the registered list.
/// </summary>
public class RadioOption<TOption> : ComponentBase, IRadioOption, IDisposable
{
    /// <summary>
    /// The content to render for this option.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The parent radio container this option is registered with.
    /// </summary>
    [CascadingParameter] public IRadioContainer? Container { get; set; }

    /// <summary>
    /// The rendered content for this option.
    /// </summary>
    RenderFragment IRadioOption.Content => ChildContent ?? (builder => { });

    /// <summary>
    /// When <see langword="true"/>, forces the option to be disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Computed disabled state, combining the <see cref="Disabled"/> parameter with internal conditions.
    /// </summary>
    bool IRadioOption.IsDisabled => Disabled;

    /// <summary>
    /// The raw value of this option.
    /// </summary>
    object? IRadioOption.RawValue => Value;

    /// <summary>
    /// The value associated with this option.
    /// </summary>
    [Parameter, EditorRequired] public TOption? Value { get; set; }

    /// <summary>
    /// Unregisters this option from the parent container.
    /// </summary>
    public void Dispose() => Container?.UnregisterOption(this);

    protected override void OnInitialized()
    {
        if (Container == null)
        {
            throw new InvalidOperationException(
                $"{nameof(RadioOption<TOption>)} must be used inside a BOBInputRadio.");
        }

        ValidateOptionType();
        Container.RegisterOption(this);
    }

    private void ValidateOptionType()
    {
        Type optionType = typeof(TOption);
        Type expectedType = Container!.ElementType;

        Type underlyingExpected = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
        Type underlyingOption = Nullable.GetUnderlyingType(optionType) ?? optionType;

        if (!underlyingExpected.IsAssignableFrom(underlyingOption))
        {
            throw new InvalidOperationException(
                $"RadioOption<{optionType.Name}> is not compatible with the radio's value type {expectedType.Name}.");
        }
    }
}
