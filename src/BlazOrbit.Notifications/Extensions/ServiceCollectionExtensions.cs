using BlazOrbit.Notifications;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>DI registration helpers for the <c>BlazOrbit.Notifications</c> package.</summary>
public static class NotificationsServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="INotificationCenter"/> + <see cref="InMemoryNotificationStore"/>
    /// (default). Pass a custom <see cref="INotificationStore"/> via
    /// <see cref="AddBlazOrbitNotifications{TStore}"/> to persist the inbox across reloads.
    /// </summary>
    public static IServiceCollection AddBlazOrbitNotifications(this IServiceCollection services)
    {
        services.AddScoped<INotificationStore, InMemoryNotificationStore>();
        services.AddScoped<INotificationCenter, NotificationCenter>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="INotificationCenter"/> with a custom
    /// <typeparamref name="TStore"/> implementation. Use to plug in localStorage,
    /// IndexedDB, or server-side persistence.
    /// </summary>
    public static IServiceCollection AddBlazOrbitNotifications<TStore>(this IServiceCollection services)
        where TStore : class, INotificationStore
    {
        services.AddScoped<INotificationStore, TStore>();
        services.AddScoped<INotificationCenter, NotificationCenter>();
        return services;
    }
}