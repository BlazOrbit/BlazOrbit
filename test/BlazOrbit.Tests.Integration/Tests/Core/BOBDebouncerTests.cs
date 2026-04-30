using BlazOrbit.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace BlazOrbit.Tests.Integration.Tests.Core;

[Trait("Core", "BOBDebouncer")]
public class BOBDebouncerTests
{
    [Fact]
    public async Task Should_Execute_Action_After_Interval_When_Only_Invoked_Once()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        using var debouncer = new BOBDebouncer<string>(TimeSpan.FromMilliseconds(100), timeProvider);
        string? captured = null;

        // Act
        var task = debouncer.InvokeAsync("hello", v => { captured = v; return Task.CompletedTask; });
        timeProvider.Advance(TimeSpan.FromMilliseconds(150));
        await task;

        // Assert
        captured.Should().Be("hello");
    }

    [Fact]
    public async Task Should_Only_Execute_Latest_Argument_When_Invoked_Multiple_Times()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        using var debouncer = new BOBDebouncer<string>(TimeSpan.FromMilliseconds(100), timeProvider);
        var captured = new List<string>();

        // Act
        var t1 = debouncer.InvokeAsync("a", v => { captured.Add(v); return Task.CompletedTask; });
        timeProvider.Advance(TimeSpan.FromMilliseconds(50));

        var t2 = debouncer.InvokeAsync("b", v => { captured.Add(v); return Task.CompletedTask; });
        timeProvider.Advance(TimeSpan.FromMilliseconds(50));

        var t3 = debouncer.InvokeAsync("c", v => { captured.Add(v); return Task.CompletedTask; });
        timeProvider.Advance(TimeSpan.FromMilliseconds(150));

        await Task.WhenAll(t1, t2, t3);

        // Assert
        captured.Should().ContainSingle().Which.Should().Be("c");
    }

    [Fact]
    public async Task Cancel_Should_Prevent_Pending_Invocation()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        using var debouncer = new BOBDebouncer<int>(TimeSpan.FromMilliseconds(100), timeProvider);
        bool executed = false;

        // Act
        var task = debouncer.InvokeAsync(1, _ => { executed = true; return Task.CompletedTask; });
        debouncer.Cancel();
        timeProvider.Advance(TimeSpan.FromMilliseconds(150));
        await task;

        // Assert
        executed.Should().BeFalse();
    }

    [Fact]
    public async Task Dispose_Should_Prevent_Future_Invocations()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var debouncer = new BOBDebouncer<int>(TimeSpan.FromMilliseconds(100), timeProvider);
        bool executed = false;

        // Act
        debouncer.Dispose();
        await debouncer.InvokeAsync(1, _ => { executed = true; return Task.CompletedTask; });
        timeProvider.Advance(TimeSpan.FromMilliseconds(150));

        // Assert
        executed.Should().BeFalse();
    }
}
