using BlazOrbit.Abstractions;
using BlazOrbit.Components.Forms.Dropdown;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components.Forms;

/// <summary>
/// Registration-only descriptor for a <c>BOBInputDropdown</c> option. Intentionally inherits
/// <see cref="ComponentBase"/> (not <c>BOBComponentBase</c>) because it emits no DOM of its own —
/// the parent dropdown container consumes the registered option and renders the menu items.
/// </summary>
public class DropdownOption<TOption> : ComponentBase, ISelectionOption, IDisposable
{
    /// <summary>
    /// The content to render for this option.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The parent dropdown container this option is registered with.
    /// </summary>
    [CascadingParameter] public IDropdownContainer? Container { get; set; }

    /// <summary>
    /// The rendered content for this option.
    /// </summary>
    RenderFragment? ISelectionOption.Content => ChildContent ?? (builder => builder.AddContent(0, Text ?? Value?.ToString()));

    /// <summary>
    /// When <see langword="true"/>, forces the option to be disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The display text for this option.
    /// </summary>
    string ISelectionOption.DisplayText => Text ?? Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Computed disabled state, combining the <see cref="Disabled"/> parameter with internal conditions.
    /// </summary>
    bool ISelectionOption.IsDisabled => Disabled;

    /// <summary>
    /// The display text for this option.
    /// </summary>
    [Parameter] public string? Text { get; set; }

    /// <summary>
    /// The value associated with this option.
    /// </summary>
    [Parameter, EditorRequired] public TOption? Value { get; set; }

    /// <summary>
    /// The raw value of this option.
    /// </summary>
    object? ISelectionOption.Value => Value;

    /// <summary>
    /// Unregisters this option from the parent container.
    /// </summary>
    public void Dispose() => Container?.UnregisterOption(this);

    protected override void OnInitialized()
    {
        if (Container == null)
        {
            throw new InvalidOperationException($"{nameof(DropdownOption<TOption>)} must be used inside a BOBInputDropdown.");
        }

        ValidateOptionType();
        Container.RegisterOption(this);
    }

    private void ValidateOptionType()
    {
        Type optionType = typeof(TOption);
        Type expectedType = Container!.ElementType;

        if (!expectedType.IsAssignableFrom(optionType))
        {
            throw new InvalidOperationException(
                $"DropdownOption<{optionType.Name}> is not compatible with the dropdown's element type {expectedType.Name}. " +
                $"For single selection, use DropdownOption<{expectedType.Name}>. " +
                $"For multiple selection (arrays/lists), use DropdownOption<{expectedType.Name}> (the element type, not the collection type).");
        }
    }
}
