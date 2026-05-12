namespace BlazOrbit.Components.Forms;

/// <summary>
/// Lifecycle status of a selected file inside <see cref="BOBInputFile"/>. Mutated by the
/// consumer via <see cref="BOBInputFile.SetStatus"/> while an upload is in flight.
/// </summary>
public enum BOBFileStatus
{
    /// <summary>Selection passed client-side validation; awaiting any consumer-driven action.</summary>
    Accepted,
    /// <summary>Upload in progress — progress bar visible while <c>ProgressPercent</c> &gt; 0.</summary>
    Uploading,
    /// <summary>Terminal success — green check glyph rendered.</summary>
    Uploaded,
    /// <summary>Terminal failure — error glyph rendered; tooltip carries the status message.</summary>
    Failed
}

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
