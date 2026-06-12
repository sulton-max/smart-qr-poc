using System.Net;
using AwesomeAssertions;
using SmartQr.IntegrationTests.Harness;
using SmartQr.IntegrationTests.Support;

namespace SmartQr.IntegrationTests.Tests;

/// <summary>
/// E2E identity flow (mirrors requests #1–#3 of <c>SmartQr.Api.http</c>): anonymous → mint guest → guest.
/// </summary>
[Collection(SmartQrCollection.Name)]
public sealed class IdentityTests(SmartQrAppFixture fixture) : SmartQrE2EBase(fixture)
{
    [Fact]
    public async Task Me_WithoutCookie_IsAnonymous()
    {
        var response = await AnonymousClient.GetAsync("/api/identity/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeResponseDto>();
        me.Kind.Should().Be("Anonymous");
    }

    [Fact]
    public async Task Guest_SetsUserIdCookie()
    {
        var client = AnonymousClient;

        var response = await client.PostAsync("/api/identity/guest", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c => c.StartsWith($"{SmartQrAppFixture.UserIdCookieName}=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Me_AfterGuestProvisioning_IsGuest()
    {
        var guest = await CreateGuestClientAsync();

        var response = await guest.Client.GetAsync("/api/identity/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeResponseDto>();
        me.Kind.Should().Be("Guest");
    }
}
