using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Service", "ToastService")]
public class ToastServiceTests
{
    [Fact]
    public void ActiveToasts_Should_Be_Empty_By_Default()
    {
        var sut = new ToastService();

        sut.ActiveToasts.Should().BeEmpty();
        sut.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShowAsync_Should_Add_Toast()
    {
        var sut = new ToastService();
        bool notified = false;
        sut.OnChangeAsync += () => { notified = true; return Task.CompletedTask; };

        await sut.ShowAsync(_ => { });

        sut.Count.Should().Be(1);
        notified.Should().BeTrue();
    }

    [Fact]
    public async Task ShowAsync_With_Options_Should_Store_Options()
    {
        var sut = new ToastService();
        var options = new ToastOptions { Duration = TimeSpan.FromSeconds(5) };

        await sut.ShowAsync(_ => { }, options);

        sut.ActiveToasts[0].Options.Duration.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CloseAsync_Should_Mark_Toast_As_Closing()
    {
        var sut = new ToastService();
        await sut.ShowAsync(_ => { });
        Guid id = sut.ActiveToasts[0].Id;

        await sut.CloseAsync(id);

        sut.ActiveToasts[0].IsClosing.Should().BeTrue();
    }

    [Fact]
    public async Task CloseAsync_UnknownId_Should_Be_NoOp()
    {
        var sut = new ToastService();
        await sut.ShowAsync(_ => { });

        await sut.CloseAsync(Guid.NewGuid());

        sut.Count.Should().Be(1);
    }

    [Fact]
    public async Task CloseAllAsync_Should_Mark_All_As_Closing()
    {
        var sut = new ToastService();
        await sut.ShowAsync(_ => { });
        await sut.ShowAsync(_ => { });

        await sut.CloseAllAsync();

        sut.ActiveToasts.Should().OnlyContain(t => t.IsClosing);
    }

    [Fact]
    public async Task PauseAsync_Should_Set_IsPaused()
    {
        var sut = new ToastService();
        await sut.ShowAsync(_ => { }, new ToastOptions { AutoDismiss = true, Duration = TimeSpan.FromSeconds(10) });
        Guid id = sut.ActiveToasts[0].Id;

        await sut.PauseAsync(id);

        sut.ActiveToasts[0].IsPaused.Should().BeTrue();
    }

    [Fact]
    public async Task ResumeAsync_Should_Clear_IsPaused()
    {
        var sut = new ToastService();
        await sut.ShowAsync(_ => { }, new ToastOptions { AutoDismiss = true, Duration = TimeSpan.FromSeconds(10) });
        Guid id = sut.ActiveToasts[0].Id;
        await sut.PauseAsync(id);

        await sut.ResumeAsync(id);

        sut.ActiveToasts[0].IsPaused.Should().BeFalse();
    }

    [Fact]
    public async Task Dispose_Should_Clear_ActiveToasts()
    {
        var sut = new ToastService();
        await sut.ShowAsync(_ => { });

        sut.Dispose();

        sut.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShowAsync_With_ComponentType_Should_Add_Toast()
    {
        var sut = new ToastService();

        await sut.ShowAsync<EmptyComponent>();

        sut.ActiveToasts.Should().ContainSingle();
    }

    private sealed class EmptyComponent : ComponentBase { }
}
