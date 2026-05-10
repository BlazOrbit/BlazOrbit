using BlazOrbit.Hotkeys;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Services.Hotkey;

[Trait("Service", "BOBHotkeyService")]
public class BOBHotkeyServiceTests
{
    [Fact]
    public async Task Should_Invoke_Handler_For_Matching_Combo()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        int hits = 0;
        using IDisposable _ = svc.Register("ctrl+s", "Save", () => { hits++; return Task.CompletedTask; });

        bool prevented = await svc.DispatchAsync("ctrl+s");

        hits.Should().Be(1);
        prevented.Should().BeTrue(); // preventDefault is true by default
    }

    [Fact]
    public async Task Should_Skip_When_No_Match()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        int hits = 0;
        using IDisposable _ = svc.Register("ctrl+s", "Save", () => { hits++; return Task.CompletedTask; });

        bool prevented = await svc.DispatchAsync("ctrl+k");

        hits.Should().Be(0);
        prevented.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Stop_Firing_After_Dispose()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        int hits = 0;
        IDisposable reg = svc.Register("escape", "Close", () => { hits++; return Task.CompletedTask; });

        await svc.DispatchAsync("escape");
        reg.Dispose();
        await svc.DispatchAsync("escape");

        hits.Should().Be(1);
    }

    [Fact]
    public async Task Should_Normalize_Modifier_Order()
    {
        // The service should accept both "Shift+Ctrl+S" and "ctrl+shift+s" interchangeably
        // because the JS bridge always emits the canonical "ctrl+shift+key" sequence.
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        int hits = 0;
        using IDisposable _ = svc.Register("Shift + Ctrl + S", "Save", () => { hits++; return Task.CompletedTask; });

        await svc.DispatchAsync("ctrl+shift+s");
        hits.Should().Be(1);
    }

    [Fact]
    public async Task Should_Not_Prevent_Default_When_Opted_Out()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        using IDisposable _ = svc.Register(
            "ctrl+k",
            "Palette",
            () => Task.CompletedTask,
            preventDefault: false);

        bool prevented = await svc.DispatchAsync("ctrl+k");

        prevented.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fire_Multiple_Handlers_For_Same_Combo()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        int a = 0, b = 0;
        using IDisposable _1 = svc.Register("ctrl+s", "A", () => { a++; return Task.CompletedTask; });
        using IDisposable _2 = svc.Register("ctrl+s", "B", () => { b++; return Task.CompletedTask; });

        await svc.DispatchAsync("ctrl+s");

        a.Should().Be(1);
        b.Should().Be(1);
    }

    [Fact]
    public void Should_List_Registered_Hotkeys_For_Cheat_Sheet()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        using IDisposable _1 = svc.Register("ctrl+s", "Save", () => Task.CompletedTask);
        using IDisposable _2 = svc.Register(
            "ctrl+k",
            "Command palette",
            () => Task.CompletedTask,
            scope: HotkeyScope.Page);

        svc.RegisteredHotkeys.Should().HaveCount(2);
        svc.RegisteredHotkeys.Should().Contain(d => d.Combo == "ctrl+s" && d.Description == "Save" && d.Scope == HotkeyScope.Global);
        svc.RegisteredHotkeys.Should().Contain(d => d.Combo == "ctrl+k" && d.Description == "Command palette" && d.Scope == HotkeyScope.Page);
    }

    [Fact]
    public async Task Should_Swallow_Handler_Exceptions()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        using IDisposable _ = svc.Register("ctrl+s", "Save", () => throw new InvalidOperationException("boom"));

        // Must not throw — we don't want a buggy hotkey handler to crash the app shell.
        Func<Task> act = () => svc.DispatchAsync("ctrl+s");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Should_Reject_Empty_Combo()
    {
        IHotkeyService svc = new BlazOrbit.Hotkeys.HotkeyService();
        Action act = () => svc.Register("", "x", () => Task.CompletedTask);
        act.Should().Throw<ArgumentException>();
    }
}
