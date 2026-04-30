namespace BlazOrbit.Components.Layout;

public enum ModalType
{
    Dialog,
    Drawer
}

public sealed class ModalState
{
    public required Type ComponentType { get; init; }
    public required string Id { get; init; }
    public bool IsAnimatingOut { get; set; }
    public bool IsVisible { get; set; } = true;
    public required ModalOptionsBase Options { get; init; }
    public Dictionary<string, object?>? Parameters { get; init; }
    public required ModalReference Reference { get; init; }
    public ModalType Type { get; init; }
}