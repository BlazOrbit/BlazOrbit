using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components.Forms;

/// <summary>Contract implemented by radio-group containers that coordinate child <see cref="IRadioOption"/> selection.</summary>
public interface IRadioContainer
{
    /// <summary>Underlying value type the radio group binds to.</summary>
    Type ElementType { get; }

    /// <summary>Returns <see langword="true"/> when <paramref name="value"/> matches the currently selected option.</summary>
    bool IsOptionSelected(object? value);

    /// <summary>Registers a child option with the container.</summary>
    void RegisterOption(IRadioOption option);

    /// <summary>Sets the currently selected option to the one whose value matches <paramref name="value"/>.</summary>
    Task SelectOptionAsync(object? value);

    /// <summary>Removes a previously registered option from the container.</summary>
    void UnregisterOption(IRadioOption option);
}

/// <summary>Contract for a single radio option child rendered inside an <see cref="IRadioContainer"/>.</summary>
public interface IRadioOption
{
    /// <summary>Content rendered inside the option (label and any custom markup).</summary>
    RenderFragment? Content { get; }
    /// <summary><see langword="true"/> when the option cannot be selected.</summary>
    bool IsDisabled { get; }
    /// <summary>Underlying value carried by the option, boxed to <see cref="object"/>.</summary>
    object? RawValue { get; }
}