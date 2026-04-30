using BlazOrbit.Utilities;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Core;

[Trait("Core", "BOBThrottler")]
public class BOBThrottlerTests
{
    [Fact]
    public async Task Should_Run_Action_Immediately_When_Idle()
    {
        // Arrange
        using var throttler = new BOBThrottler<string>(TimeSpan.FromMilliseconds(100));
        string? captured = null;

        // Act
        await throttler.InvokeAsync("first", v => { captured = v; return Task.CompletedTask; });

        // Assert
        captured.Should().Be("first");
    }

    [Fact]
    public async Task Should_Coalesce_Subsequent_Calls_During_Cooldown()
    {
        // Arrange — use a short real interval so the test remains fast.
        using var throttler = new BOBThrottler<string>(TimeSpan.FromMilliseconds(10));
        var captured = new List<string>();

        // Act — first runs immediately and enters cooldown
        Task t1 = throttler.InvokeAsync("a", v => { captured.Add(v); return Task.CompletedTask; });

        // These two arrive while cooling down; they are coalesced.
        _ = throttler.InvokeAsync("b", v => { captured.Add(v); return Task.CompletedTask; });
        _ = throttler.InvokeAsync("c", v => { captured.Add(v); return Task.CompletedTask; });

        // Wait for the first cooldown + trailing cooldown to expire
        await t1;

        // Assert — only the first and the latest coalesced argument executed
        captured.Should().Equal("a", "c");
    }

    [Fact]
    public async Task Cancel_Should_Reset_Cooldown_And_Discard_Pending()
    {
        // Arrange
        using var throttler = new BOBThrottler<int>(TimeSpan.FromMilliseconds(100));
        var captured = new List<int>();

        // Act — start first invocation (enters cooldown)
        Task t1 = throttler.InvokeAsync(1, v => { captured.Add(v); return Task.CompletedTask; });

        // Queue a second invocation while cooling down
        _ = throttler.InvokeAsync(2, v => { captured.Add(v); return Task.CompletedTask; });

        // Cancel discards the pending call and aborts the cooldown
        throttler.Cancel();
        await t1;

        // Assert
        captured.Should().ContainSingle().Which.Should().Be(1);
    }

    [Fact]
    public async Task Dispose_Should_Prevent_Future_Invocations()
    {
        // Arrange
        var throttler = new BOBThrottler<int>(TimeSpan.FromMilliseconds(100));
        bool executed = false;

        // Act
        throttler.Dispose();
        await throttler.InvokeAsync(1, _ => { executed = true; return Task.CompletedTask; });

        // Assert
        executed.Should().BeFalse();
    }
}
