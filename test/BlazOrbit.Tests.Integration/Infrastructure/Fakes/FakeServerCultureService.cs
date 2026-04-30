using System.Globalization;

namespace BlazOrbit.Tests.Integration.Infrastructure.Fakes;

public sealed class FakeServerCultureService : ITestCultureService
{
    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

    public Task SetCultureAsync(string culture)
    {
        CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = CurrentCulture;
        CultureInfo.CurrentUICulture = CurrentCulture;
        return Task.CompletedTask;
    }
}