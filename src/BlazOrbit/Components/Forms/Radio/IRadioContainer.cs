using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components.Forms;

public interface IRadioContainer
{
    Type ElementType { get; }

    bool IsOptionSelected(object? value);

    void RegisterOption(IRadioOption option);

    Task SelectOptionAsync(object? value);

    void UnregisterOption(IRadioOption option);
}

public interface IRadioOption
{
    RenderFragment? Content { get; }
    bool IsDisabled { get; }
    object? RawValue { get; }
}