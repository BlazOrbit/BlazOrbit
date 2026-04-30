using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Integration", "BOBDialog")]
public class BOBDialogIntegrationTests
{
    private sealed class RecordingModalInterop : IModalJsInterop
    {
        public List<(string Op, string Id)> Calls { get; } = [];

        public ValueTask LockScrollAsync() => ValueTask.CompletedTask;
        public ValueTask UnlockScrollAsync() => ValueTask.CompletedTask;
        public ValueTask WaitForAnimationEndAsync(ElementReference element, int fallbackMs) => ValueTask.CompletedTask;

        public ValueTask TrapFocusAsync(ElementReference element, string trapId)
        {
            Calls.Add(("trap", trapId));
            return ValueTask.CompletedTask;
        }

        public ValueTask ReleaseFocusAsync(string trapId)
        {
            Calls.Add(("release", trapId));
            return ValueTask.CompletedTask;
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Drive_Trap_Release_In_LIFO_Order_For_Nested_Dialogs(BlazorScenario scenario)
    {
        // JS-03 + A11Y-05: nested modals must trap/release in LIFO order so the
        // JS-side focus-trap stack (ModalInterop.ts focusTrapStack) restores
        // parent focus correctly when the inner dialog closes. Each component
        // owns a stable per-instance trapId so the JS layer can identify which
        // entry to remove (criterion: pass an identifier to activate/release).
        // bUnit can only assert the C# orchestration; the JS stack semantics
        // and the actual focus restoration are validated by the type system,
        // the Vite bundle build, and the WCAG 2.4.3 contract documented in TS.
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RecordingModalInterop interop = new();
        ctx.Services.AddScoped<IModalJsInterop>(_ => interop);

        // Arrange — outer dialog open, inner dialog closed
        IRenderedComponent<BOBDialog> outer = ctx.Render<BOBDialog>(p => p.Add(c => c.Open, true));
        IRenderedComponent<BOBDialog> inner = ctx.Render<BOBDialog>(p => p.Add(c => c.Open, false));

        interop.Calls.Should().HaveCount(1);
        string outerId = interop.Calls[0].Id;
        outerId.Should().NotBeNullOrEmpty();
        interop.Calls[0].Op.Should().Be("trap");

        // Act — open inner (nested), then close inner, then close outer
        inner.Render(p => p.Add(c => c.Open, true));
        interop.Calls.Should().HaveCount(2);
        string innerId = interop.Calls[1].Id;
        innerId.Should().NotBeNullOrEmpty().And.NotBe(outerId, "each component must own a distinct trap identifier");
        interop.Calls[1].Op.Should().Be("trap");

        inner.Render(p => p.Add(c => c.Open, false));
        outer.Render(p => p.Add(c => c.Open, false));

        // Assert — full LIFO sequence with id pairing: open A, open B, close B, close A
        interop.Calls.Should().Equal(
            ("trap", outerId),
            ("trap", innerId),
            ("release", innerId),
            ("release", outerId));
    }
}
