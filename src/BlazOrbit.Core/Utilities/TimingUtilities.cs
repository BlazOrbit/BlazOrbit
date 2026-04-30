namespace BlazOrbit.Abstractions;

internal static class TimingUtilities
{
    public static Action<T> Debounce<T>(Action<T> action, TimeSpan interval, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        timeProvider ??= TimeProvider.System;
        int last = 0;
        return arg =>
        {
            int current = Interlocked.Increment(ref last);
            Task.Delay(interval, timeProvider).ContinueWith(_ =>
            {
                if (current == last)
                {
                    action(arg);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        };
    }

    public static Action Debounce(Action action, TimeSpan interval, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        timeProvider ??= TimeProvider.System;
        int last = 0;
        return () =>
        {
            int current = Interlocked.Increment(ref last);
            Task.Delay(interval, timeProvider).ContinueWith(_ =>
            {
                if (current == last)
                {
                    action();
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        };
    }
}
