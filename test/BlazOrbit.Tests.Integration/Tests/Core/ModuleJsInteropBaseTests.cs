using BlazOrbit.Abstractions;
using FluentAssertions;
using Microsoft.JSInterop;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Core;

[Trait("Core", "ModuleJsInteropBase")]
public class ModuleJsInteropBaseTests
{
    private sealed class TestModuleJsInterop : ModuleJsInteropBase
    {
        public TestModuleJsInterop(IJSRuntime jsRuntime, string path)
            : base(jsRuntime, path)
        {
        }

        public Lazy<Task<IJSObjectReference>> ModuleAccessor => ModuleTask;
    }

    [Fact]
    public void Constructor_Should_Throw_When_JsRuntime_Is_Null()
    {
        Action act = () => new TestModuleJsInterop(null!, "./test.js");
        act.Should().Throw<ArgumentNullException>().WithParameterName("jsRuntime");
    }

    [Fact]
    public async Task DisposeAsync_Should_NoOp_When_Module_Was_Never_Imported()
    {
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        TestModuleJsInterop sut = new(jsRuntime, "./test.js");

        await sut.DisposeAsync();

        await jsRuntime.DidNotReceive().InvokeAsync<IJSObjectReference>(
            Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    public async Task DisposeAsync_Should_Dispose_Module_When_Imported()
    {
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();
        jsRuntime.InvokeAsync<IJSObjectReference>("import", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<IJSObjectReference>(module));

        TestModuleJsInterop sut = new(jsRuntime, "./test.js");
        _ = await sut.ModuleAccessor.Value; // force import

        await sut.DisposeAsync();

        await module.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_Should_Swallow_JSDisconnectedException()
    {
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();
        jsRuntime.InvokeAsync<IJSObjectReference>("import", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<IJSObjectReference>(module));
        module.DisposeAsync()
            .Returns(_ => throw new JSDisconnectedException("circuit gone"));

        TestModuleJsInterop sut = new(jsRuntime, "./test.js");
        _ = await sut.ModuleAccessor.Value;

        Func<Task> act = async () => await sut.DisposeAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_Should_Swallow_ObjectDisposedException()
    {
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();
        jsRuntime.InvokeAsync<IJSObjectReference>("import", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<IJSObjectReference>(module));
        module.DisposeAsync()
            .Returns(_ => throw new ObjectDisposedException("runtime"));

        TestModuleJsInterop sut = new(jsRuntime, "./test.js");
        _ = await sut.ModuleAccessor.Value;

        Func<Task> act = async () => await sut.DisposeAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_Should_Swallow_InvalidOperationException()
    {
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();
        jsRuntime.InvokeAsync<IJSObjectReference>("import", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<IJSObjectReference>(module));
        module.DisposeAsync()
            .Returns(_ => throw new InvalidOperationException("no runtime"));

        TestModuleJsInterop sut = new(jsRuntime, "./test.js");
        _ = await sut.ModuleAccessor.Value;

        Func<Task> act = async () => await sut.DisposeAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_Should_Swallow_TaskCanceledException()
    {
        IJSRuntime jsRuntime = Substitute.For<IJSRuntime>();
        IJSObjectReference module = Substitute.For<IJSObjectReference>();
        jsRuntime.InvokeAsync<IJSObjectReference>("import", Arg.Any<object[]>())
            .Returns(_ => new ValueTask<IJSObjectReference>(module));
        module.DisposeAsync()
            .Returns(_ => throw new TaskCanceledException());

        TestModuleJsInterop sut = new(jsRuntime, "./test.js");
        _ = await sut.ModuleAccessor.Value;

        Func<Task> act = async () => await sut.DisposeAsync();
        await act.Should().NotThrowAsync();
    }
}
