namespace BlazOrbit.Components.Layout;

/// <summary>Contract implemented by components hosted inside a dialog or drawer modal.</summary>
public interface IModalContent
{
    /// <summary>Cascaded reference used by the content to close the modal and supply a result.</summary>
    ModalReference ModalReference { get; set; }
}