using Microsoft.AspNetCore.Components.Forms;

namespace BlazOrbit.Components.Forms;

/// <summary>
/// Streams an <see cref="IBrowserFile"/> to a consumer-supplied transport in fixed-size
/// chunks. Opt-in helper used alongside <see cref="BOBInputFile"/> when a single
/// <c>InputFileChangeEventArgs.GetMultipleFiles</c> round-trip is too large to buffer.
/// </summary>
/// <remarks>
/// The helper is transport-agnostic — the consumer provides <see cref="SendChunk"/> and
/// decides whether the destination is HTTP, SignalR, the file system, or something else.
/// Chunks are emitted sequentially in index order; the helper awaits each
/// <see cref="SendChunk"/> invocation before reading the next chunk, so a slow transport
/// applies back-pressure on the read stream automatically.
/// </remarks>
public sealed class BOBChunkedUploader
{
    /// <summary>Chunk size in bytes. Defaults to <c>1 MB</c>.</summary>
    public int ChunkSize { get; init; } = 1 * 1024 * 1024;

    /// <summary>
    /// Hard ceiling for <see cref="IBrowserFile.Size"/> in bytes. Files larger than this
    /// fail before any chunk is read. Also forwarded as the <c>maxAllowedSize</c> argument
    /// to <see cref="IBrowserFile.OpenReadStream"/>. Defaults to <c>100 MB</c>.
    /// </summary>
    public long MaxAllowedSize { get; init; } = 100L * 1024 * 1024;

    /// <summary>
    /// Callback invoked for every chunk in index order. Awaited before the next chunk is
    /// read, so a slow consumer back-pressures the upload.
    /// </summary>
    public required Func<BOBChunkContext, CancellationToken, Task> SendChunk { get; init; }

    /// <summary>Optional progress sink invoked after each chunk is delivered.</summary>
    public IProgress<BOBChunkedUploadProgress>? Progress { get; init; }

    /// <summary>
    /// Reads <paramref name="file"/> chunk by chunk and forwards each to
    /// <see cref="SendChunk"/>. Zero-byte files emit no chunks and return a successful
    /// result with <c>TotalChunks = 0</c>. Errors thrown by the read stream, the callback,
    /// or cancellation are captured in <see cref="BOBChunkedUploadResult.Error"/> — the
    /// method itself does not rethrow.
    /// </summary>
    /// <param name="file">The browser file to stream.</param>
    /// <param name="ct">Cancellation token honoured between chunks and inside reads.</param>
    public async Task<BOBChunkedUploadResult> UploadAsync(IBrowserFile file, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (ChunkSize <= 0)
        {
            return new BOBChunkedUploadResult(file.Name, file.Size, 0, false,
                new ArgumentOutOfRangeException(nameof(ChunkSize), ChunkSize, "ChunkSize must be positive."));
        }

        if (file.Size > MaxAllowedSize)
        {
            return new BOBChunkedUploadResult(file.Name, file.Size, 0, false,
                new InvalidOperationException(
                    $"File '{file.Name}' is {file.Size} bytes; exceeds MaxAllowedSize ({MaxAllowedSize})."));
        }

        // Math.Ceiling on doubles loses precision for huge files; do integer arithmetic.
        int totalChunks = file.Size == 0
            ? 0
            : (int)((file.Size + ChunkSize - 1) / ChunkSize);

        if (totalChunks == 0)
        {
            return new BOBChunkedUploadResult(file.Name, 0, 0, true, null);
        }

        try
        {
            // OpenReadStream's default maxAllowedSize is 512 KB; without an explicit value
            // any non-trivial file would throw before the first chunk. Use MaxAllowedSize
            // so callers configure both ceilings in one place.
            using Stream stream = file.OpenReadStream(MaxAllowedSize, ct);

            long bytesSent = 0;

            for (int index = 0; index < totalChunks; index++)
            {
                ct.ThrowIfCancellationRequested();

                long offset = (long)index * ChunkSize;
                int length = (int)Math.Min(ChunkSize, file.Size - offset);

                // Per-chunk allocation keeps the contract simple: the consumer may safely
                // park the ReadOnlyMemory<byte> in a queue if needed (no buffer reuse
                // footgun). Pooling can land in v2 if benchmarks demand it.
                byte[] buffer = new byte[length];
                await stream.ReadExactlyAsync(buffer, 0, length, ct);

                BOBChunkContext chunk = new(
                    file.Name,
                    file.ContentType,
                    file.Size,
                    index,
                    totalChunks,
                    offset,
                    buffer);

                await SendChunk(chunk, ct);

                bytesSent += length;
                Progress?.Report(new BOBChunkedUploadProgress(
                    file.Name,
                    bytesSent,
                    file.Size,
                    index,
                    totalChunks));
            }

            return new BOBChunkedUploadResult(file.Name, file.Size, totalChunks, true, null);
        }
        catch (Exception ex)
        {
            return new BOBChunkedUploadResult(file.Name, file.Size, totalChunks, false, ex);
        }
    }
}