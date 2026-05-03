using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Infrastructure.Fakes;

public sealed class FakeNavigationManager : NavigationManager
{
    public FakeNavigationManager() => Initialize("http://localhost/", "http://localhost/");

    public string? LastNavigationUri { get; private set; }

    public bool? LastForceLoad { get; private set; }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        LastNavigationUri = uri;
        LastForceLoad = forceLoad;
        Uri = ToAbsoluteUri(uri).ToString();
    }
}