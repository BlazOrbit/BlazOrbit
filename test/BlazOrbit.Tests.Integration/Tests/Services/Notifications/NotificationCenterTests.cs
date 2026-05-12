using BlazOrbit.Notifications;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Services.Notifications;

[Trait("Service", "BOBNotificationCenter")]
public class BOBNotificationCenterTests
{
    private static INotificationCenter Build() => new NotificationCenter(new InMemoryNotificationStore());

    [Fact]
    public async Task Should_Push_And_List_Newest_First()
    {
        INotificationCenter center = Build();

        await center.PushAsync(new BOBNotification
        {
            Title = "first", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        });
        await center.PushAsync(new BOBNotification
        {
            Title = "second", CreatedAt = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)
        });

        center.All.Should().HaveCount(2);
        center.All[0].Title.Should().Be("second");
        center.All[1].Title.Should().Be("first");
    }

    [Fact]
    public async Task Should_Track_UnreadCount()
    {
        INotificationCenter center = Build();

        await center.PushAsync(new BOBNotification { Title = "a" });
        await center.PushAsync(new BOBNotification { Title = "b" });
        await center.PushAsync(new BOBNotification { Title = "c", IsRead = true });

        center.UnreadCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_Mark_Single_As_Read()
    {
        INotificationCenter center = Build();
        BOBNotification entry = new() { Title = "x" };
        await center.PushAsync(entry);

        await center.MarkReadAsync(entry.Id);

        center.UnreadCount.Should().Be(0);
        center.All.Single().IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Mark_All_As_Read()
    {
        INotificationCenter center = Build();
        await center.PushAsync(new BOBNotification { Title = "a" });
        await center.PushAsync(new BOBNotification { Title = "b" });

        await center.MarkAllReadAsync();

        center.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_Remove_Entry()
    {
        INotificationCenter center = Build();
        BOBNotification entry = new() { Title = "x" };
        await center.PushAsync(entry);

        await center.RemoveAsync(entry.Id);

        center.All.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Clear_Inbox()
    {
        INotificationCenter center = Build();
        await center.PushAsync(new BOBNotification { Title = "a" });
        await center.PushAsync(new BOBNotification { Title = "b" });

        await center.ClearAsync();

        center.All.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Fire_OnChangeAsync_On_Mutations()
    {
        INotificationCenter center = Build();
        int hits = 0;
        center.OnChangeAsync += () =>
        {
            hits++;
            return Task.CompletedTask;
        };

        BOBNotification entry = new() { Title = "x" };
        await center.PushAsync(entry);
        await center.MarkReadAsync(entry.Id);
        await center.RemoveAsync(entry.Id);

        // Push + MarkRead + Remove = 3 successful mutations.
        hits.Should().Be(3);
    }

    [Fact]
    public async Task Should_Not_Fire_OnChangeAsync_On_Unknown_Id()
    {
        INotificationCenter center = Build();
        int hits = 0;
        center.OnChangeAsync += () =>
        {
            hits++;
            return Task.CompletedTask;
        };

        await center.MarkReadAsync("missing");
        await center.RemoveAsync("missing");

        hits.Should().Be(0);
    }
}