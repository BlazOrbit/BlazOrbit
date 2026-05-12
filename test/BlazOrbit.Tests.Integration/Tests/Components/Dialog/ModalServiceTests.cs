using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Service", "ModalService")]
public class ModalServiceTests
{
    private sealed class TestModalContent : IModalContent
    {
        public ModalReference ModalReference { get; set; } = default!;
    }

    [Fact]
    public void ActiveModals_Should_Be_Empty_By_Default()
    {
        ModalService sut = new();

        sut.ActiveModals.Should().BeEmpty();
    }

    [Fact]
    public async Task ShowDialogAsync_Should_Add_Modal()
    {
        ModalService sut = new();
        bool notified = false;
        sut.OnChangeAsync += () =>
        {
            notified = true;
            return Task.CompletedTask;
        };

        await sut.ShowDialogAsync<TestModalContent>();

        sut.ActiveModals.Should().ContainSingle();
        sut.ActiveModals[0].Type.Should().Be(ModalType.Dialog);
        notified.Should().BeTrue();
    }

    [Fact]
    public async Task ShowDrawerAsync_Should_Add_Modal()
    {
        ModalService sut = new();

        await sut.ShowDrawerAsync<TestModalContent>();

        sut.ActiveModals.Should().ContainSingle();
        sut.ActiveModals[0].Type.Should().Be(ModalType.Drawer);
    }

    [Fact]
    public async Task CloseAsync_Should_Remove_Topmost_Modal()
    {
        ModalService sut = new();
        await sut.ShowDialogAsync<TestModalContent>();

        await sut.CloseAsync();

        sut.ActiveModals.Should().BeEmpty();
    }

    [Fact]
    public async Task CloseAllAsync_Should_Remove_All_Modals()
    {
        ModalService sut = new();
        await sut.ShowDialogAsync<TestModalContent>();
        await sut.ShowDrawerAsync<TestModalContent>();

        await sut.CloseAllAsync();

        sut.ActiveModals.Should().BeEmpty();
    }

    [Fact]
    public async Task ShowDialogAsync_With_Parameters_Should_Pass_Parameters()
    {
        ModalService sut = new();
        var parameters = new { Key = "Value" };

        await sut.ShowDialogAsync<TestModalContent>(parameters);

        sut.ActiveModals[0].Parameters.Should().ContainKey("Key").WhoseValue.Should().Be("Value");
    }

    [Fact]
    public async Task ShowDialogAsync_Typed_Should_Return_Result_When_Closed()
    {
        ModalService sut = new();
        Task<string?> showTask = sut.ShowDialogAsync<TestModalContent, string>();

        ModalState modal = sut.ActiveModals.Single();
        _ = Task.Run(async () => await modal.Reference.CloseAsync("ok"), TestContext.Current.CancellationToken);

        string? result = await showTask.WaitAsync(TestContext.Current.CancellationToken);

        result.Should().Be("ok");
    }
}