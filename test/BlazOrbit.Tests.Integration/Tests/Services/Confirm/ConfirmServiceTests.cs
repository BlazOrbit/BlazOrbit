using BlazOrbit.Components;
using FluentAssertions;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Services.Confirm;

[Trait("Service", "BOBConfirmService")]
public class BOBConfirmServiceTests
{
    [Fact]
    public async Task Should_Resolve_True_When_Modal_Returns_True()
    {
        IModalService modal = Substitute.For<IModalService>();
        modal.ShowDialogAsync<BOBConfirmDialog, bool>(
                Arg.Any<object?>(),
                Arg.Any<DialogOptions?>())
            .Returns(Task.FromResult(true));

        IConfirmService svc = new ConfirmService(modal);

        bool result = await svc.AskAsync("title", "message");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Resolve_False_When_Modal_Returns_Default()
    {
        IModalService modal = Substitute.For<IModalService>();
        // Treats overlay/escape dismissals (modal returns default(bool) = false) as "no".
        modal.ShowDialogAsync<BOBConfirmDialog, bool>(
                Arg.Any<object?>(),
                Arg.Any<DialogOptions?>())
            .Returns(Task.FromResult(false));

        IConfirmService svc = new ConfirmService(modal);

        bool result = await svc.AskAsync("title", "message");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Resolve_False_On_Timeout()
    {
        IModalService modal = Substitute.For<IModalService>();
        // Dialog never resolves; the timeout race wins and the service should call CloseAsync
        // to drop the orphan modal.
        TaskCompletionSource<bool> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        modal.ShowDialogAsync<BOBConfirmDialog, bool>(
                Arg.Any<object?>(),
                Arg.Any<DialogOptions?>())
            .Returns(tcs.Task);

        IConfirmService svc = new ConfirmService(modal);

        bool result = await svc.AskAsync(
            "title",
            "message",
            timeout: TimeSpan.FromMilliseconds(20));

        result.Should().BeFalse();
        await modal.Received(1).CloseAsync();
    }

    [Fact]
    public async Task Should_Forward_Severity_And_Labels_To_Dialog()
    {
        IModalService modal = Substitute.For<IModalService>();
        object? capturedParams = null;
        modal.ShowDialogAsync<BOBConfirmDialog, bool>(
                Arg.Do<object?>(p => capturedParams = p),
                Arg.Any<DialogOptions?>())
            .Returns(Task.FromResult(true));

        IConfirmService svc = new ConfirmService(modal);

        await svc.AskAsync(
            "Delete?",
            "This cannot be undone.",
            ConfirmSeverity.Danger,
            "Delete",
            "Keep");

        capturedParams.Should().NotBeNull();
        // Anonymous-object property bag - verify via reflection.
        Type type = capturedParams!.GetType();
        type.GetProperty("Title")!.GetValue(capturedParams).Should().Be("Delete?");
        type.GetProperty("Message")!.GetValue(capturedParams).Should().Be("This cannot be undone.");
        type.GetProperty("Severity")!.GetValue(capturedParams).Should().Be(ConfirmSeverity.Danger);
        type.GetProperty("YesLabel")!.GetValue(capturedParams).Should().Be("Delete");
        type.GetProperty("NoLabel")!.GetValue(capturedParams).Should().Be("Keep");
    }

    [Fact]
    public async Task Should_Configure_Dialog_To_Block_Overlay_Click()
    {
        IModalService modal = Substitute.For<IModalService>();
        DialogOptions? capturedOpts = null;
        modal.ShowDialogAsync<BOBConfirmDialog, bool>(
                Arg.Any<object?>(),
                Arg.Do<DialogOptions?>(o => capturedOpts = o))
            .Returns(Task.FromResult(false));

        IConfirmService svc = new ConfirmService(modal);

        await svc.AskAsync("t", "m");

        capturedOpts.Should().NotBeNull();
        // Overlay-click dismissal would feel like an accidental "no" - better to force the
        // user to use the explicit Cancel button.
        capturedOpts!.CloseOnOverlayClick.Should().BeFalse();
        capturedOpts.CloseOnEscape.Should().BeTrue();
        capturedOpts.Title.Should().Be("t");
    }
}
