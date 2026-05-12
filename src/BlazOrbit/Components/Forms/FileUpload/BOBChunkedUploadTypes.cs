namespace BlazOrbit.Components.Forms;

/// <summary>
/// One chunk emitted by <see cref="BOBChunkedUploader"/>. The <see cref="Data"/> buffer is
/// owned by the uploader and only guaranteed valid for the duration of the
/// <c>SendChunk</c> callback's awaitable — do not retain it past the callback's return.
/// </summary>
/// <param name="FileName">The originating file's display name.</param>
/// <param name="ContentType">The originating file's MIME type.</param>
/// <param name="FileSize">Total size of the originating file in bytes.</param>
/// <param name="Index">Zero-based chunk index.</param>
/// <param name="TotalChunks">Total chunks the uploader will emit for this file.</param>
/// <param name="Offset">Byte offset within the file where <see cref="Data"/> starts.</param>
/// <param name="Data">The chunk payload. Length is always <c>&lt;= ChunkSize</c>; the final
/// chunk is short whenever <c>FileSize</c> is not a multiple of <c>ChunkSize</c>.</param>
public readonly record struct BOBChunkContext(
    string FileName,
    string ContentType,
    long FileSize,
    int Index,
    int TotalChunks,
    long Offset,
    ReadOnlyMemory<byte> Data);

/// <summary>
/// Progress snapshot reported via <see cref="BOBChunkedUploader.Progress"/> after each
/// successfully delivered chunk.
/// </summary>
/// <param name="FileName">The originating file's display name.</param>
/// <param name="BytesSent">Cumulative bytes delivered so far.</param>
/// <param name="FileSize">Total size of the originating file in bytes.</param>
/// <param name="ChunkIndex">Index of the most recently delivered chunk.</param>
/// <param name="TotalChunks">Total chunks the uploader will emit for this file.</param>
public readonly record struct BOBChunkedUploadProgress(
    string FileName,
    long BytesSent,
    long FileSize,
    int ChunkIndex,
    int TotalChunks);

/// <summary>
/// Terminal outcome returned by <see cref="BOBChunkedUploader.UploadAsync"/>. Exceptions
/// thrown by the read stream, the consumer-supplied <c>SendChunk</c> callback, or
/// cancellation are captured in <see cref="Error"/> rather than rethrown.
/// </summary>
/// <param name="FileName">The originating file's display name.</param>
/// <param name="FileSize">Total size of the originating file in bytes.</param>
/// <param name="TotalChunks">Total chunks the uploader emitted (or would have emitted).</param>
/// <param name="Success"><see langword="true"/> when every chunk completed without error.</param>
/// <param name="Error">The captured exception when <see cref="Success"/> is <see langword="false"/>;
/// <see langword="null"/> otherwise.</param>
public sealed record BOBChunkedUploadResult(
    string FileName,
    long FileSize,
    int TotalChunks,
    bool Success,
    Exception? Error);
