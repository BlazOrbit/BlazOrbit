using System.Globalization;

namespace BlazOrbit.Tests.Integration.Infrastructure.Fakes;

public interface ITestCultureService
{
    CultureInfo CurrentCulture { get; }

    Task SetCultureAsync(string culture);
}