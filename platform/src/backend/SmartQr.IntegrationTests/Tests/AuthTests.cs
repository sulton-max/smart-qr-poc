using System.Net;
using AwesomeAssertions;
using SmartQr.IntegrationTests.Harness;
using SmartQr.IntegrationTests.Support;

namespace SmartQr.IntegrationTests.Tests;

/// <summary>
/// E2E auth flow: Google sign-in (find-or-create), guest-code claim, cross-device ownership, and sign-out.
/// Google verification is stubbed by <see cref="FakeGoogleTokenVerifier"/> — token form <c>fake:{sub}:{email}:{name}</c>.
/// </summary>
[Collection(AppCollection.Name)]
public sealed class AuthTests(AppFixture fixture) : E2EBase(fixture)
{
    private static string Token(string subject, string email, string name) => $"fake:{subject}:{email}:{name}";

    [Fact]
    public async Task SignIn_NewAccount_ReturnsUserAndSetsAuthCookie()
    {
        var response = await AnonymousClient.PostJsonAsync(
            "/api/auth/google", new { idToken = Token("sub-new", "alice@example.com", "Alice") });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeResponseDto>();
        me.Kind.Should().Be("User");

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c => c.StartsWith($"{AppFixture.AuthCookieName}=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SignIn_InvalidToken_Returns401()
    {
        var response = await AnonymousClient.PostJsonAsync("/api/auth/google", new { idToken = "not-a-valid-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_AfterSignIn_IsUserWithProfile()
    {
        var (_, authed) = await SignInAsync(Token("sub-bob", "bob@example.com", "Bob"));

        var response = await authed.GetAsync("/api/identity/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeWithUserDto>();
        me.Kind.Should().Be("User");
        me.User!.Email.Should().Be("bob@example.com");
        me.User.Name.Should().Be("Bob");
    }

    [Fact]
    public async Task SignIn_ClaimsGuestCodes_SameDevice()
    {
        // A guest creates a code...
        var guest = await CreateGuestClientAsync();
        var created = await (await guest.Client.PostJsonAsync(
                "/api/codes", CodeRequests.Code("Guest menu", "https://example.com/menu")))
            .ReadEnvelopeAsync<CodeDtoModel>();

        // ...then signs in on the SAME device (the guest cookie rides along on the request).
        var (response, authed) = await SignInAsync(Token("sub-carol", "carol@example.com", "Carol"), guest.Client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The account now owns the code the guest made.
        var codes = await (await authed.GetAsync("/api/codes")).ReadEnvelopeAsync<IReadOnlyList<CodeDtoModel>>();
        codes.Should().ContainSingle(c => c.Id == created.Id);
    }

    [Fact]
    public async Task SignedIn_CodesFollowAccount_AcrossDevices()
    {
        // Device A: sign in, create a code.
        var (_, deviceA) = await SignInAsync(Token("sub-dave", "dave@example.com", "Dave"));
        var created = await (await deviceA.PostJsonAsync(
                "/api/codes", CodeRequests.Code("Shop", "https://example.com/shop")))
            .ReadEnvelopeAsync<CodeDtoModel>();

        // Device B: a brand-new client signs in to the SAME account (no shared cookie) and sees the code.
        var (_, deviceB) = await SignInAsync(Token("sub-dave", "dave@example.com", "Dave"));
        var codes = await (await deviceB.GetAsync("/api/codes")).ReadEnvelopeAsync<IReadOnlyList<CodeDtoModel>>();

        codes.Should().Contain(c => c.Id == created.Id);
    }

    [Fact]
    public async Task Logout_ClearsTheSessionCookie()
    {
        var (_, authed) = await SignInAsync(Token("sub-erin", "erin@example.com", "Erin"));

        var logout = await authed.PostAsync("/api/auth/logout", content: null);

        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);
        logout.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        // Sign-out deletes the cookie by re-issuing it empty (expired).
        cookies!.Should().Contain(c => c.StartsWith($"{AppFixture.AuthCookieName}=;", StringComparison.Ordinal));
    }

    /// <summary>Signs in with the fake Google token from a given client (default: a fresh anonymous one) and returns a client carrying the issued session cookie.</summary>
    private async Task<(HttpResponseMessage Response, HttpClient Authed)> SignInAsync(string idToken, HttpClient? from = null)
    {
        var client = from ?? AnonymousClient;
        var response = await client.PostJsonAsync("/api/auth/google", new { idToken });

        var authed = Fixture.CreateApiClient();
        if (AppFixture.ExtractCookie(response, AppFixture.AuthCookieName) is { } cookie)
            authed.DefaultRequestHeaders.Add("Cookie", $"{AppFixture.AuthCookieName}={cookie}");

        return (response, authed);
    }
}
