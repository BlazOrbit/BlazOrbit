using BlazOrbit.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazOrbit.Tests.Integration.Tests.Core;

[Trait("Core", "BOBAsyncHelper")]
public class BOBAsyncHelperTests
{
    private sealed class FakeLogger : ILogger
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, exception, formatter(state, exception)));
        }
    }

    [Fact]
    public async Task SafeFireAndForget_Should_Complete_Without_Exception_For_Successful_Work()
    {
        bool executed = false;

        BOBAsyncHelper.SafeFireAndForget(async () =>
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
            executed = true;
        });

        await Task.Delay(50, TestContext.Current.CancellationToken);
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task SafeFireAndForget_Should_Swallow_TaskCanceledException()
    {
        bool executed = false;
        var logger = new FakeLogger();

        BOBAsyncHelper.SafeFireAndForget(async () =>
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
            executed = true;
            throw new TaskCanceledException();
        }, logger);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        executed.Should().BeTrue();
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task SafeFireAndForget_Should_Swallow_JSDisconnectedException()
    {
        bool executed = false;
        var logger = new FakeLogger();

        BOBAsyncHelper.SafeFireAndForget(async () =>
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
            executed = true;
            throw new JSDisconnectedException("circuit gone");
        }, logger);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        executed.Should().BeTrue();
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task SafeFireAndForget_Should_Swallow_ObjectDisposedException()
    {
        bool executed = false;
        var logger = new FakeLogger();

        BOBAsyncHelper.SafeFireAndForget(async () =>
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
            executed = true;
            throw new ObjectDisposedException("component");
        }, logger);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        executed.Should().BeTrue();
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task SafeFireAndForget_Should_Swallow_InvalidOperationException()
    {
        bool executed = false;
        var logger = new FakeLogger();

        BOBAsyncHelper.SafeFireAndForget(async () =>
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
            executed = true;
            throw new InvalidOperationException("no renderer");
        }, logger);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        executed.Should().BeTrue();
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task SafeFireAndForget_Should_Log_Unexpected_Exception_When_Logger_Provided()
    {
        bool executed = false;
        var logger = new FakeLogger();
        var unexpected = new InvalidCastException("boom");

        BOBAsyncHelper.SafeFireAndForget(async () =>
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
            executed = true;
            throw unexpected;
        }, logger);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        executed.Should().BeTrue();
        logger.Entries.Should().ContainSingle()
            .Which.Should().BeEquivalentTo((
                LogLevel.Error,
                (Exception?)unexpected,
                "Fire-and-forget task failed"));
    }
}
