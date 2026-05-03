using BlazOrbit.Localization.Server;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Trait("Localization", "CultureEndpointSecurity")]
public class CultureEndpointSecurityTests
{
    private const string DefaultEndpointPath = "/BlazOrbit/Culture/Set";
    private const string DefaultCookieName = ".BlazOrbit.Culture";

    private static async Task<IHost> CreateHostAsync(Action<ServerLocalizationSettings>? configure = null)
    {
        return await new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                    .UseTestServer()
                    .ConfigureServices(services => services.AddBlazOrbitLocalizationServer(configure))
                    .Configure(app => app.Run(async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsync("OK", TestContext.Current.CancellationToken);
                        })))
            .StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Set_Culture_Cookie_With_Secure_HttpOnly_And_SameSiteLax()
    {
        // Arrange
        using IHost host = await CreateHostAsync();
        using HttpClient client = host.GetTestServer().CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            $"{DefaultEndpointPath}?culture=es-ES&redirectUri=/docs",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        string setCookie = response.Headers.GetValues("Set-Cookie").Single();
        setCookie.Should().StartWith($"{DefaultCookieName}=");
        setCookie.Should().Contain("secure");
        setCookie.Should().Contain("httponly");
        setCookie.Should().Contain("samesite=lax");
    }

    [Fact]
    public async Task Should_Set_Culture_Cookie_With_Expires_One_Year()
    {
        // Arrange
        using IHost host = await CreateHostAsync();
        using HttpClient client = host.GetTestServer().CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            $"{DefaultEndpointPath}?culture=es-ES&redirectUri=/docs",
            TestContext.Current.CancellationToken);

        // Assert
        string setCookie = response.Headers.GetValues("Set-Cookie").Single();
        setCookie.Should().Contain("expires=");

        // Extract expires value for rough verification
        int start = setCookie.IndexOf("expires=", StringComparison.OrdinalIgnoreCase) + "expires=".Length;
        int end = setCookie.IndexOf(';', start);
        string expiresValue = end == -1 ? setCookie[start..] : setCookie[start..end];
        DateTimeOffset expires = DateTimeOffset.Parse(expiresValue.Trim());
        expires.Should().BeCloseTo(DateTimeOffset.UtcNow.AddYears(1), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Should_Redirect_To_Valid_RootRelative_Path()
    {
        // Arrange
        using IHost host = await CreateHostAsync();
        using HttpClient client = host.GetTestServer().CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            $"{DefaultEndpointPath}?culture=es-ES&redirectUri=/docs",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be("/docs");
    }

    [Theory]
    [InlineData("https://evil.com")]          // absolute URL
    [InlineData("//evil.com")]                // scheme-relative
    [InlineData("http:evil.com")]             // colon trick — parsed as relative by Uri but absolute by browser
    [InlineData("/\\evil.com")]               // backslash trick — IE/Edge quirk
    [InlineData("")]                          // empty
    public async Task Should_Fallback_To_Root_For_Untrusted_RedirectUri(string redirectUri)
    {
        // Arrange
        using IHost host = await CreateHostAsync();
        using HttpClient client = host.GetTestServer().CreateClient();

        // Act
        string query = string.IsNullOrEmpty(redirectUri)
            ? $"{DefaultEndpointPath}?culture=es-ES&redirectUri="
            : $"{DefaultEndpointPath}?culture=es-ES&redirectUri={Uri.EscapeDataString(redirectUri)}";

        HttpResponseMessage response = await client.GetAsync(
            query, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be("/");
    }

    [Fact]
    public async Task Should_Not_Set_Cookie_When_Culture_Is_Missing()
    {
        // Arrange
        using IHost host = await CreateHostAsync();
        using HttpClient client = host.GetTestServer().CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            $"{DefaultEndpointPath}?redirectUri=/docs",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Contains("Set-Cookie").Should().BeFalse();
    }

    [Fact]
    public async Task Should_Write_Culture_Value_Into_Cookie()
    {
        // Arrange
        using IHost host = await CreateHostAsync();
        using HttpClient client = host.GetTestServer().CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            $"{DefaultEndpointPath}?culture=fr-FR&redirectUri=/",
            TestContext.Current.CancellationToken);

        // Assert
        string setCookie = response.Headers.GetValues("Set-Cookie").Single();
        string valuePart = setCookie.Split(';')[0].Split('=')[1];
        string decoded = Uri.UnescapeDataString(valuePart);

        decoded.Should().Contain("c=fr-FR");
        decoded.Should().Contain("uic=fr-FR");
    }
}
