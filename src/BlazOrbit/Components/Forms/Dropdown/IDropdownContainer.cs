using BlazOrbit.Abstractions;

namespace BlazOrbit.Components.Forms.Dropdown;

public interface IDropdownContainer
{
    Type ElementType { get; }
    bool IsMultiple { get; }

    void RegisterOption(ISelectionOption option);

    void UnregisterOption(ISelectionOption option);
}