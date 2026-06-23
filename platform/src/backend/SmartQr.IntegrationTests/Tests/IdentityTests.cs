using System.Net;
using AwesomeAssertions;
using SmartQr.IntegrationTests.Harness;
using SmartQr.IntegrationTests.Support;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;

namespace SmartQr.IntegrationTests.Tests;

/// <summary>E2E identity flow — anonymous, mint guest, then guest.</summary>
[Collection(AppCollection.Name)]
public sealed class IdentityTests(AppFixture fixture) : E2EBase(fixture)
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
        cookies!.Should().Contain(c => c.StartsWith($"{AppFixture.UserIdCookieName}=", StringComparison.Ordinal));
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
