using BlazOrbit.Components.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazOrbit.Tests.Integration.Tests.Components.FileUpload;

[Trait("Component Integration", "BOBChunkedUploader")]
public class BOBChunkedUploaderTests
{
    [Fact]
    public async Task Should_Emit_All_Chunks_For_File_Aligned_To_ChunkSize()
    {
        // Arrange — 4 chunks of exactly 16 bytes each.
        byte[] payload = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();
        FakeBrowserFile file = new("aligned.bin", "application/octet-stream", payload);

        List<BOBChunkContext> seen = [];
        BOBChunkedUploader uploader = new()
        {
            ChunkSize = 16,
            MaxAllowedSize = payload.Length,
            SendChunk = (c, _) =>
            {
                seen.Add(c);
                return Task.CompletedTask;
            }
        };

        // Act
        BOBChunkedUploadResult result = await uploader.UploadAsync(file, TestContext.Current.CancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.TotalChunks.Should().Be(4);
        seen.Should().HaveCount(4);
        seen.Select(c => c.Index).Should().Equal(0, 1, 2, 3);
        seen.Select(c => c.Offset).Should().Equal(0, 16, 32, 48);
        seen.Select(c => c.Data.Length).Should().Equal(16, 16, 16, 16);

        // Round-trip the bytes — the chunks should reassemble the source verbatim.
        byte[] reassembled = seen.SelectMany(c => c.Data.ToArray()).ToArray();
        reassembled.Should().Equal(payload);
    }

    [Fact]
    public async Task Should_Emit_Last_Chunk_Smaller_When_File_Not_Aligned()
    {
        // Arrange — 16 + 16 + 5 = 37 bytes. Final chunk is short.
        byte[] payload = Enumerable.Range(0, 37).Select(i => (byte)i).ToArray();
        FakeBrowserFile file = new("ragged.bin", "application/octet-stream", payload);

        List<BOBChunkContext> seen = [];
        BOBChunkedUploader uploader = new()
        {
            ChunkSize = 16,
            MaxAllowedSize = payload.Length,
            SendChunk = (c, _) =>
            {
                seen.Add(c);
                return Task.CompletedTask;
            }
        };

        // Act
        BOBChunkedUploadResult result = await uploader.UploadAsync(file, TestContext.Current.CancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        result.TotalChunks.Should().Be(3);
        seen.Select(c => c.Data.Length).Should().Equal(16, 16, 5);
        seen.Last().Offset.Should().Be(32);
    }

    [Fact]
    public async Task Should_Report_Progress_After_Each_Chunk()
    {
        // Arrange
        byte[] payload = new byte[48];
        FakeBrowserFile file = new("progress.bin", "application/octet-stream", payload);

        List<BOBChunkedUploadProgress> snapshots = [];
        BOBChunkedUploader uploader = new()
        {
            ChunkSize = 16,
            MaxAllowedSize = payload.Length,
            SendChunk = (_, _) => Task.CompletedTask,
            Progress = new TestProgress<BOBChunkedUploadProgress>(snapshots.Add)
        };

        // Act
        await uploader.UploadAsync(file, TestContext.Current.CancellationToken);

        // Assert — one progress snapshot per chunk, monotonic bytes.
        snapshots.Should().HaveCount(3);
        snapshots.Select(p => p.BytesSent).Should().Equal(16, 32, 48);
        snapshots.Select(p => p.ChunkIndex).Should().Equal(0, 1, 2);
        snapshots.Should().AllSatisfy(p =>
        {
            p.FileSize.Should().Be(48);
            p.TotalChunks.Should().Be(3);
        });
    }

    [Fact]
    public async Task Should_Fail_When_File_Exceeds_MaxAllowedSize()
    {
        // Arrange — 100 bytes against a 50-byte ceiling.
        byte[] payload = new byte[100];
        FakeBrowserFile file = new("oversize.bin", "application/octet-stream", payload);

        bool sendCalled = false;
        BOBChunkedUploader uploader = new()
        {
            ChunkSize = 16,
            MaxAllowedSize = 50,
            SendChunk = (_, _) =>
            {
                sendCalled = true;
                return Task.CompletedTask;
            }
        };

        // Act
        BOBChunkedUploadResult result = await uploader.UploadAsync(file, TestContext.Current.CancellationToken);

        // Assert — bailed before opening the stream.
        result.Success.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationException>();
        result.TotalChunks.Should().Be(0);
        sendCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Propagate_Exception_From_SendChunk_As_Result_Failure()
    {
        // Arrange — callback throws on the second chunk.
        byte[] payload = new byte[48];
        FakeBrowserFile file = new("explode.bin", "application/octet-stream", payload);

        int callCount = 0;
        BOBChunkedUploader uploader = new()
        {
            ChunkSize = 16,
            MaxAllowedSize = payload.Length,
            SendChunk = (_, _) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    throw new InvalidOperationException("transport down");
                }

                return Task.CompletedTask;
            }
        };

        // Act
        BOBChunkedUploadResult result = await uploader.UploadAsync(file, TestContext.Current.CancellationToken);

        // Assert — captured, not rethrown.
        result.Success.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be("transport down");
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_Stop_When_CancellationToken_Cancelled()
    {
        // Arrange — cancel after the first chunk.
        byte[] payload = new byte[48];
        FakeBrowserFile file = new("cancel.bin", "application/octet-stream", payload);

        CancellationTokenSource cts = new();
        int callCount = 0;
        BOBChunkedUploader uploader = new()
        {
            ChunkSize = 16,
            MaxAllowedSize = payload.Length,
            SendChunk = (_, _) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    cts.Cancel();
                }

                return Task.CompletedTask;
            }
        };

        // Act
        BOBChunkedUploadResult result = await uploader.UploadAsync(file, cts.Token);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeAssignableTo<OperationCanceledException>();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_Handle_Zero_Byte_File_With_No_Chunks_Emitted()
    {
        // Arrange
        FakeBrowserFile file = new("empty.bin", "application/octet-stream", []);

        bool sendCalled = false;
        BOBChunkedUploader uploader = new()
        {
            ChunkSize = 16,
            MaxAllowedSize = 0,
            SendChunk = (_, _) =>
            {
                sendCalled = true;
                return Task.CompletedTask;
            }
        };

        // Act
        BOBChunkedUploadResult result = await uploader.UploadAsync(file, TestContext.Current.CancellationToken);

        // Assert — degenerate but valid: zero chunks, success.
        result.Success.Should().BeTrue();
        result.TotalChunks.Should().Be(0);
        sendCalled.Should().BeFalse();
    }

    // ---------- helpers ----------

    private sealed class FakeBrowserFile(string name, string contentType, byte[] data) : IBrowserFile
    {
        public string Name { get; } = name;
        public DateTimeOffset LastModified { get; } = DateTimeOffset.UtcNow;
        public long Size { get; } = data.LongLength;
        public string ContentType { get; } = contentType;

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            if (Size > maxAllowedSize)
            {
                throw new IOException($"Supplied file with size {Size} exceeds max {maxAllowedSize}.");
            }

            return new MemoryStream(data, false);
        }
    }

    private sealed class TestProgress<T>(Action<T> onReport) : IProgress<T>
    {
        public void Report(T value) => onReport(value);
    }
}