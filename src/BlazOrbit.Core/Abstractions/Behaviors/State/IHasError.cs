namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports an error state.
/// </summary>
public interface IHasError
{
    /// <summary>When <see langword="true" />, forces the error state.</summary>
    bool Error { get; }

    /// <summary>Computed error state, combining <see cref="Error" /> with validation messages from <see cref="Microsoft.AspNetCore.Components.Forms.EditContext" />.</summary>
    bool IsError { get; }
}
