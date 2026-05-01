namespace BlazOrbit.Components.Layout;

/// <summary>Discriminator between modal kinds.</summary>
public enum ModalType
{
    /// <summary>Centered dialog.</summary>
    Dialog,
    /// <summary>Edge-anchored drawer.</summary>
    Drawer
}

/// <summary>Runtime state for a modal currently tracked by <see cref="Services.IModalService"/>.</summary>
public sealed class ModalState
{
    /// <summary>Concrete component type rendered inside the modal.</summary>
    public required Type ComponentType { get; init; }
    /// <summary>Stable identifier used by the JS focus trap.</summary>
    public required string Id { get; init; }
    /// <summary><see langword="true"/> while the closing animation is running.</summary>
    public bool IsAnimatingOut { get; set; }
    /// <summary><see langword="true"/> while the modal is in the visible stack.</summary>
    public bool IsVisible { get; set; } = true;
    /// <summary>Options describing rendering and behavior (dialog vs drawer specifics).</summary>
    public required ModalOptionsBase Options { get; init; }
    /// <summary>Parameter bag forwarded to the rendered component.</summary>
    public Dictionary<string, object?>? Parameters { get; init; }
    /// <summary>Reference used by the content to close and return a result.</summary>
    public required ModalReference Reference { get; init; }
    /// <summary>Modal kind.</summary>
    public ModalType Type { get; init; }
}