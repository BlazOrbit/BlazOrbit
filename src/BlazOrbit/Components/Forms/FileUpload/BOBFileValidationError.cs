namespace BlazOrbit.Components.Forms;

/// <summary>
/// Reason a file was rejected by <see cref="BOBInputFile"/>'s client-side filters.
/// </summary>
public enum BOBFileValidationErrorKind
{
    /// <summary>The file exceeded the configured <c>MaxSize</c>.</summary>
    TooLarge,
    /// <summary>The file's MIME type or extension did not satisfy the <c>Accept</c> filter.</summary>
    DisallowedType,
    /// <summary>The selection would push the total above the <c>MaxFiles</c> ceiling.</summary>
    TooManyFiles
}

/// <summary>
/// One rejected file emitted by <see cref="BOBInputFile.OnInvalid"/>.
/// </summary>
/// <param name="FileName">The file's display name.</param>
/// <param name="Size">Reported size in bytes.</param>
/// <param name="Kind">Reason the file was rejected.</param>
public readonly record struct BOBFileValidationError(string FileName, long Size, BOBFileValidationErrorKind Kind);
