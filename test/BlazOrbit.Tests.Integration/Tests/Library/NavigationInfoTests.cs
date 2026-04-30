using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// SEC-03: NavigationInfo rejects unsafe URI schemes so BOBTreeMenu cannot emit
/// anchors with <c>javascript:</c> / <c>data:</c> href values when consumers feed
/// Href from untrusted sources.
/// </summary>
[Trait("Library", "NavigationInfo")]
public class NavigationInfoTests
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com")]
    [InlineData("mailto:user@example.com")]
    [InlineData("tel:+34900000000")]
    [InlineData("/absolute/path")]
    [InlineData("./relative")]
    [InlineData("../parent")]
    [InlineData("page")]
    [InlineData("page?q=1")]
    [InlineData("#fragment")]
    [InlineData("?query=1")]
    public void HasNavigation_Should_Be_True_For_Safe_Href(string href)
    {
        NavigationInfo info = new() { Href = href };
        info.HasNavigation.Should().BeTrue();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("JavaScript:alert(1)")]
    [InlineData("JAVASCRIPT:alert(1)")]
    [InlineData("vbscript:msgbox(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://example.com")]
    public void HasNavigation_Should_Be_False_For_Unsafe_Scheme(string href)
    {
        NavigationInfo info = new() { Href = href };
        info.HasNavigation.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HasNavigation_Should_Be_False_For_Empty_Href(string? href)
    {
        NavigationInfo info = new() { Href = href };
        info.HasNavigation.Should().BeFalse();
    }

    [Fact]
    public void IsSafeHref_Relative_Path_With_Colon_In_Query_Should_Pass()
        // A scheme-less path that happens to contain `:` after a `/` — e.g. `/search?q=a:b`.
        => NavigationInfo.IsSafeHref("/search?q=foo:bar").Should().BeTrue();
}
