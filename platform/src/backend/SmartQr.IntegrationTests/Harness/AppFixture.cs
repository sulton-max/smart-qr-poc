extern alias apihost;
extern alias redirecthost;

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartQr.IntegrationTests.Harness.Containers;

// Disambiguate the two top-level `Program` types (both live in the global namespace of their assembly).
using ApiProgram = apihost::Program;
using RedirectProgram = redirecthost::Program;

// The Api's Google-verifier seam — swapped for a deterministic fake in the test host.
using IGoogleTokenVerifier = apihost::SmartQr.Api.Application.Identity.Core.Services.IGoogleTokenVerifier;

namespace SmartQr.IntegrationTests.Harness;

/// <summary>
/// Owns the single shared Postgres container and the two in-process hosts (Api and Redirect) the whole E2E run
/// drives. Both hosts point at the SAME container DB, so a write through the Api host is visible to a scan on
/// the Redirect host. Migrations auto-apply on each host's startup (<c>MigrateDatabaseAsync</c>); the
/// Respawner is built once both schemas exist and resets all data tables (except <c>migration_history</c>)
/// between tests via <see cref="ResetAsync"/>.
/// </summary>
public sealed class AppFixture : IAsyncLifetime
{
    /// <summary>Name of the identity cookie the Api host sets on guest provisioning.</summary>
    public const string UserIdCookieName = "user-id";

    /// <summary>Name of the session cookie the Api host sets on Google sign-in.</summary>
    public const string AuthCookieName = "sqr-auth";

    /// <summary>
    /// Stable redirect base used to build each code's <c>shortUrl</c>. Set via <c>SMARTQR_REDIRECT_BASE_URL</c>
    /// so assertions on <c>shortUrl</c> are deterministic regardless of the (random) Kestrel test port.
    /// </summary>
    public const string RedirectBaseUrl = "https://redirect.smartqr.test";

    private readonly PostgresFixture _postgres = new();

    private WebApiTestHost<ApiProgram>? _apiHost;
    private WebApiTestHost<RedirectProgram>? _redirectHost;

    /// <summary>The shared Postgres fixture (container and Respawn).</summary>
    public PostgresFixture Postgres => _postgres;

    /// <summary>The management Api host.</summary>
    public WebApiTestHost<ApiProgram> ApiHost =>
        _apiHost ?? throw new InvalidOperationException("Fixture not initialized — ApiHost is null.");

    /// <summary>The redirect (hot-path) host.</summary>
    public WebApiTestHost<RedirectProgram> RedirectHost =>
        _redirectHost ?? throw new InvalidOperationException("Fixture not initialized — RedirectHost is null.");

    /// <summary>A fresh anonymous client against the Api host (no identity cookie).</summary>
    public HttpClient CreateApiClient() => ApiHost.CreateClient();

    /// <summary>A fresh client against the Redirect host. Redirects are NOT followed so 302s can be asserted.</summary>
    public HttpClient CreateRedirectClient() => RedirectHost.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // The config loader's env overlay wins over in-memory config, so the env var is the authoritative
        // injection point. Process-global, but both hosts share the process and the same DB — that's intended.
        Environment.SetEnvironmentVariable("SMARTQR_DB_CONNECTION", _postgres.ConnectionString);
        Environment.SetEnvironmentVariable("SMARTQR_REDIRECT_BASE_URL", RedirectBaseUrl);
        Environment.SetEnvironmentVariable("SMARTQR_REDIS_CONNECTION", null); // ensure DbRedirectConfigStore path

        _apiHost = new WebApiTestHost<ApiProgram>
        {
            ConnectionString = _postgres.ConnectionString,
            // Swap the real Google verifier for a deterministic fake so E2E never calls Google.
            ConfigureServicesHook = services =>
            {
                services.RemoveAll<IGoogleTokenVerifier>();
                services.AddScoped<IGoogleTokenVerifier>(_ => new FakeGoogleTokenVerifier());
            },
        };
        _redirectHost = new WebApiTestHost<RedirectProgram> { ConnectionString = _postgres.ConnectionString };

        // Force each host to build (and run its startup migration) before snapshotting the DB with Respawn.
        _ = _apiHost.Services;
        _ = _redirectHost.Services;

        await _postgres.InitializeRespawnerAsync();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        _apiHost?.Dispose();
        _redirectHost?.Dispose();
        await _postgres.DisposeAsync();
    }

    /// <summary>Truncates all data tables between tests (keeps <c>migration_history</c>).</summary>
    public ValueTask ResetAsync() => _postgres.ResetAsync();

    /// <summary>
    /// Provisions a guest via <c>POST /api/identity/guest</c>, captures the <c>user-id</c> cookie, and returns an
    /// Api client that carries it on every request.
    /// </summary>
    /// <remarks>
    /// The cookie is set with <c>Secure=true</c>, which a default <see cref="CookieContainer"/> will not round-trip
    /// over the test host's <c>http://</c> base address. We therefore lift the id out of the <c>Set-Cookie</c> header
    /// and attach it as a raw <c>Cookie</c> request header.
    /// </remarks>
    public async Task<GuestClient> CreateGuestClientAsync()
    {
        var client = ApiHost.CreateClient();

        var response = await client.PostAsync("/api/identity/guest", content: null);
        response.EnsureSuccessStatusCode();

        var userId = ExtractUserId(response)
            ?? throw new InvalidOperationException("POST /api/identity/guest did not set the user-id cookie.");

        var authed = ApiHost.CreateClient();
        authed.DefaultRequestHeaders.Add("Cookie", $"{UserIdCookieName}={userId}");

        return new GuestClient(authed, userId);
    }

    private static string? ExtractUserId(HttpResponseMessage response) => ExtractCookie(response, UserIdCookieName);

    /// <summary>Lifts a cookie value out of the response's <c>Set-Cookie</c> headers by name, or null when absent.</summary>
    public static string? ExtractCookie(HttpResponseMessage response, string name)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        var prefix = $"{name}=";
        foreach (var cookie in cookies)
        {
            // e.g. "sqr-auth=<value>; path=/; secure; httponly; ..."
            var head = cookie.Split(';', 2)[0].Trim();
            if (head.StartsWith(prefix, StringComparison.Ordinal))
                return head[prefix.Length..];
        }

        return null;
    }
}

/// <summary>An authenticated guest: the <see cref="HttpClient"/> carrying the identity cookie plus its raw id.</summary>
/// <param name="Client">Api client carrying the <c>user-id</c> cookie.</param>
/// <param name="UserId">The provisioned guest id (string form of the cookie value).</param>
public sealed record GuestClient(HttpClient Client, string UserId);

/// <summary>xUnit collection that shares one <see cref="AppFixture"/> across the whole E2E run.</summary>
[CollectionDefinition(AppCollection.Name)]
public sealed class AppCollection : ICollectionFixture<AppFixture>
{
    /// <summary>The collection name — every E2E test class joins this so they share the container and run serially.</summary>
    public const string Name = "smart-qr-e2e";
}

/// <summary>
/// Convenience base for E2E tests — wires the shared fixture, resets the DB before each test, and exposes
/// fresh clients. Concrete test classes carry <c>[Collection(AppCollection.Name)]</c>.
/// </summary>
public abstract class E2EBase(AppFixture fixture) : IAsyncLifetime
{
    /// <summary>The shared app fixture (container and two hosts).</summary>
    protected AppFixture Fixture { get; } = fixture;

    /// <summary>An anonymous Api client (no identity cookie).</summary>
    protected HttpClient AnonymousClient => Fixture.CreateApiClient();

    /// <summary>A redirect client that does NOT auto-follow 302s.</summary>
    protected HttpClient RedirectClient => Fixture.CreateRedirectClient();

    /// <summary>Provisions a guest and returns a client carrying its cookie.</summary>
    protected Task<GuestClient> CreateGuestClientAsync() => Fixture.CreateGuestClientAsync();

    /// <inheritdoc />
    public async Task InitializeAsync() => await Fixture.ResetAsync();

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
