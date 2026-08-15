extern alias apihost;
extern alias redirecthost;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartQr.Application.Billing.Core.Services;
using SmartQr.Persistence.DataContexts;
using Testcontainers.PostgreSql;
// The Google-verifier seam (SDK type, global namespace) — swapped for a deterministic fake in the test host.
using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google;
using WoW.Two.Sdk.Backend.Beta.Testing;
using WoW.Two.Sdk.Backend.Beta.Testing.Containers;
using WoW.Two.Sdk.Backend.Beta.Testing.Containers.Postgres;
using WoW.Two.Sdk.Backend.Beta.Testing.MultiHost;

// Disambiguate the two top-level `Program` types (both live in the global namespace of their assembly).
using ApiProgram = apihost::Program;
using RedirectProgram = redirecthost::Program;
// Billing config types — aliased to the API settings classes (vs the unrelated SDK Billing namespace).
using BillingSettings = SmartQr.Application.Settings.BillingSettings;
using BillingPricesSettings = SmartQr.Application.Settings.BillingPricesSettings;

namespace SmartQr.Tests.E2E.Harness;

/// <summary>Boots the Api and Redirect hosts over a shared Postgres container, Respawn-reset between tests.</summary>
public sealed class AppFixture : MultiHostFixture, IAsyncLifetime
{
    /// <summary>Holds the name of the identity cookie the Api host sets on guest provisioning.</summary>
    public const string UserIdCookieName = "user-id";

    /// <summary>Holds the name of the session cookie the Api host sets on Google sign-in.</summary>
    public const string AuthCookieName = "sqr-auth";

    /// <summary>Holds the redirect base for each code's <c>shortUrl</c>, set via <c>REDIRECT_BASE_URL</c>.</summary>
    public const string RedirectBaseUrl = "https://redirect.smartqr.test";

    /// <summary>Holds the fake Stripe price id for the Solo plan.</summary>
    public const string PriceSolo = "price_solo";

    /// <summary>Holds the fake Stripe price id for the Pro plan.</summary>
    public const string PricePro = "price_pro";

    /// <summary>Holds the fake Stripe price id for the Agency plan.</summary>
    public const string PriceAgency = "price_agency";

    private readonly PostgresFixture _postgres;

    /// <summary>Gets the fake Stripe broker wired into the Api host.</summary>
    public FakeBillingBroker Gateway { get; } = new();

    /// <summary>Gets the shared Postgres fixture.</summary>
    public PostgresFixture Postgres => _postgres;

    /// <summary>Gets the management Api host.</summary>
    public WebApiTestHost<ApiProgram> ApiHost { get; }

    /// <summary>Gets the redirect (hot-path) host.</summary>
    public WebApiTestHost<RedirectProgram> RedirectHost { get; }

    /// <summary>Registers the shared container and both hosts.</summary>
    public AppFixture()
    {
        _postgres = AddSharedFixture(
            new PostgresFixture(new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build()));

        ApiHost = AddHost(new WebApiTestHost<ApiProgram>
        {
            // The SDK host has no connection-string knob — inject it the way the app reads it
            // (DatabaseOptions:ConnectionString).
            // The hook runs at build time (after the container has started), so the connection string is available; it
            // mirrors the DB_CONNECTION env seam.
            ConfigureHostHook = builder => builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DatabaseOptions:ConnectionString"] = _postgres.ConnectionString,
                })),
            // Swap the real external seams for deterministic fakes so E2E never calls Google or Stripe.
            ConfigureServicesHook = services =>
            {
                services.RemoveAll<IGoogleIdTokenVerifier>();
                services.AddScoped<IGoogleIdTokenVerifier>(_ => new FakeGoogleTokenVerifier());

                // Fake billing broker (no network) shared across the run so a test can stage a webhook event.
                services.RemoveAll<IBillingBroker>();
                services.AddSingleton<IBillingBroker>(Gateway);

                // Fake Stripe settings with known price ids — the host loads the real (empty) Billing singleton
                // from config before this hook, so replace it. Webhook plan resolution reads Billing:Prices.
                services.RemoveAll<BillingSettings>();
                services.AddSingleton(new BillingSettings
                {
                    SecretKey = "sk_test_fake",
                    WebhookSecret = "whsec_fake",
                    Prices = new BillingPricesSettings { Solo = PriceSolo, Pro = PricePro, Agency = PriceAgency },
                    SuccessUrl = "https://app.example/ok",
                    CancelUrl = "https://app.example/cancel",
                });
            },
        });

        RedirectHost = AddHost(new WebApiTestHost<RedirectProgram>
        {
            ConfigureHostHook = builder => builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DatabaseOptions:ConnectionString"] = _postgres.ConnectionString,
                })),
        });
    }

    /// <summary>Creates a fresh anonymous client against the Api host.</summary>
    public HttpClient CreateApiClient() => ApiHost.CreateClient();

    /// <summary>Creates a fresh client against the Redirect host that does not follow redirects.</summary>
    public HttpClient CreateRedirectClient() => RedirectHost.CreateClient(
        new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    /// <summary>Creates a new database context on the shared container.</summary>
    /// <remarks>Use it to seed rows the API cannot create.</remarks>
    public AppDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .UseSnakeCaseNamingConvention();
        return new AppDbContext(options.Options);
    }

    /// <summary>POSTs a raw body with a <c>Stripe-Signature</c> header to the webhook endpoint.</summary>
    public static Task<HttpResponseMessage> PostWebhookAsync(HttpClient client, string signature = "test-sig")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("Stripe-Signature", signature);
        return client.SendAsync(request);
    }

    /// <summary>Points both hosts at the shared container before they build.</summary>
    protected override void ConfigureEnvironment()
    {
        Environment.SetEnvironmentVariable("DB_CONNECTION", _postgres.ConnectionString);
        Environment.SetEnvironmentVariable("REDIRECT_BASE_URL", RedirectBaseUrl);
        Environment.SetEnvironmentVariable("REDIS_CONNECTION", null); // ensure DbRedirectConfigRepository path
    }

    /// <summary>Snapshots the post-migration schema for Respawn.</summary>
    protected override ValueTask InitializeStateAsync(CancellationToken cancellationToken = default) =>
        _postgres.InitializeRespawnerAsync(cancellationToken);

    /// <summary>Provisions a guest and returns an Api client carrying the <c>user-id</c> cookie.</summary>
    /// <remarks>The <c>Secure</c> cookie won't round-trip over <c>http://</c>, so it's sent as a raw header.</remarks>
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

    /// <summary>Lifts a cookie value out of the response's <c>Set-Cookie</c> headers, or null when absent.</summary>
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

    /// <summary>Starts the topology for xUnit.</summary>
    Task IAsyncLifetime.InitializeAsync() => StartAsync().AsTask();

    /// <summary>Disposes the hosts, then the container.</summary>
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
}

/// <summary>Represents a provisioned guest and the Api client carrying its cookie.</summary>
/// <param name="Client">Api client carrying the <c>user-id</c> cookie.</param>
/// <param name="UserId">The provisioned guest id (string form of the cookie value).</param>
public sealed record GuestClient(HttpClient Client, string UserId);

/// <summary>Defines the xUnit collection that shares one fixture across the E2E run.</summary>
[CollectionDefinition(AppCollection.Name)]
public sealed class AppCollection : ICollectionFixture<AppFixture>
{
    /// <summary>Holds the collection name every E2E test class joins.</summary>
    public const string Name = "smart-qr-e2e";
}

/// <summary>Provides the shared fixture and per-test database reset for E2E tests.</summary>
public abstract class E2EBase(AppFixture fixture) : IAsyncLifetime
{
    /// <summary>Gets the shared app fixture.</summary>
    protected AppFixture Fixture { get; } = fixture;

    /// <summary>Gets an Api client with no identity cookie.</summary>
    protected HttpClient AnonymousClient => Fixture.CreateApiClient();

    /// <summary>Gets a redirect client that does not auto-follow 302s.</summary>
    protected HttpClient RedirectClient => Fixture.CreateRedirectClient();

    /// <summary>Provisions a guest and returns a client carrying its cookie.</summary>
    protected Task<GuestClient> CreateGuestClientAsync() => Fixture.CreateGuestClientAsync();

    /// <summary>Resets the database and the fake gateway.</summary>
    public async Task InitializeAsync()
    {
        await Fixture.ResetAsync();
        Fixture.Gateway.Reset();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
