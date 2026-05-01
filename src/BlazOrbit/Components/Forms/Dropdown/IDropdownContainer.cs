using BlazOrbit.Abstractions;

namespace BlazOrbit.Components.Forms.Dropdown;

/// <summary>
/// Contract implemented by components that host dropdown options. Lets
/// individual <see cref="ISelectionOption"/> children register against the
/// container for selection coordination.
/// </summary>
public interface IDropdownContainer
{
    /// <summary>Underlying value type the container binds to (e.g. <c>typeof(string)</c>).</summary>
    Type ElementType { get; }
    /// <summary><see langword="true"/> when the container allows multiple simultaneous selections.</summary>
    bool IsMultiple { get; }

    /// <summary>Registers a child option with the container.</summary>
    void RegisterOption(ISelectionOption option);

    /// <summary>Removes a previously registered option from the container.</summary>
    void UnregisterOption(ISelectionOption option);
}