extern alias apihost;
extern alias redirecthost;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Common.Persistence.DataContexts;
using Testcontainers.PostgreSql;
// The Google-verifier seam (SDK type, global namespace) — swapped for a deterministic fake in the test host.
using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google;
using WoW.Two.Sdk.Backend.Beta.Testing;
using WoW.Two.Sdk.Backend.Beta.Testing.Containers;
using WoW.Two.Sdk.Backend.Beta.Testing.MultiHost;

// Disambiguate the two top-level `Program` types (both live in the global namespace of their assembly).
using ApiProgram = apihost::Program;
using RedirectProgram = redirecthost::Program;
// Billing config type — alias to the API settings class (vs the unrelated SDK Billing namespace).
using BillingSettings = SmartQr.Api.Settings.Billing;
using BillingPrices = SmartQr.Api.Settings.BillingPrices;

namespace SmartQr.Tests.E2E.Harness;

/// <summary>Boots the Api and Redirect hosts over one shared Postgres container via the SDK <see cref="MultiHostFixture"/>; Respawn resets data tables (except <c>migration_history</c>) between tests.</summary>
public sealed class AppFixture : MultiHostFixture, IAsyncLifetime
{
    /// <summary>Name of the identity cookie the Api host sets on guest provisioning.</summary>
    public const string UserIdCookieName = "user-id";

    /// <summary>Name of the session cookie the Api host sets on Google sign-in.</summary>
    public const string AuthCookieName = "sqr-auth";

    /// <summary>Stable redirect base for each code's <c>shortUrl</c> (set via <c>REDIRECT_BASE_URL</c>) so assertions are port-independent.</summary>
    public const string RedirectBaseUrl = "https://redirect.smartqr.test";

    /// <summary>Fake Stripe price id wired into the Api host for the Solo plan (the inverse <c>Billing:Prices</c> map resolves it back to <see cref="WoW.Two.Sdk.Backend.Beta"/>-free Solo).</summary>
    public const string PriceSolo = "price_solo";

    /// <summary>Fake Stripe price id wired into the Api host for the Pro plan.</summary>
    public const string PricePro = "price_pro";

    /// <summary>Fake Stripe price id wired into the Api host for the Agency plan.</summary>
    public const string PriceAgency = "price_agency";

    private readonly PostgresFixture _postgres;

    /// <summary>The fake Stripe gateway wired into the Api host — set <see cref="FakeBillingGateway.NextEvent"/> to drive a webhook, read its captured last-call fields after a checkout/portal. No real Stripe.</summary>
    public FakeBillingGateway Gateway { get; } = new();

    /// <summary>The shared Postgres fixture (container and Respawn).</summary>
    public PostgresFixture Postgres => _postgres;

    /// <summary>The management Api host.</summary>
    public WebApiTestHost<ApiProgram> ApiHost { get; }

    /// <summary>The redirect (hot-path) host.</summary>
    public WebApiTestHost<RedirectProgram> RedirectHost { get; }

    /// <summary>Registers the shared container and both hosts; they build during <see cref="MultiHostFixture.StartAsync"/>.</summary>
    public AppFixture()
    {
        _postgres = AddSharedFixture(new PostgresFixture(new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build()));

        ApiHost = AddHost(new WebApiTestHost<ApiProgram>
        {
            // The SDK host has no connection-string knob — inject it the way the app reads it (DatabaseOptions:ConnectionString).
            // The hook runs at build time (after the container has started), so the connection string is available; it mirrors the DB_CONNECTION env seam.
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

                // Fake billing gateway (no network) shared across the run so a test can stage a webhook event.
                services.RemoveAll<IBillingGateway>();
                services.AddSingleton<IBillingGateway>(Gateway);

                // Fake Stripe settings with known price ids — the host loads the real (empty) Billing singleton
                // from config before this hook, so replace it. Webhook plan resolution reads Billing:Prices.
                services.RemoveAll<BillingSettings>();
                services.AddSingleton(new BillingSettings
                {
                    SecretKey = "sk_test_fake",
                    WebhookSecret = "whsec_fake",
                    Prices = new BillingPrices { Solo = PriceSolo, Pro = PricePro, Agency = PriceAgency },
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

    /// <summary>A fresh anonymous client against the Api host (no identity cookie).</summary>
    public HttpClient CreateApiClient() => ApiHost.CreateClient();

    /// <summary>A fresh client against the Redirect host. Redirects are NOT followed so 302s can be asserted.</summary>
    public HttpClient CreateRedirectClient() => RedirectHost.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    /// <summary>A new <see cref="AppDbContext"/> on the shared container (snake_case convention) for seeding rows the API can't create directly — e.g. an existing subscription before a <c>subscription.updated</c> / cancel webhook.</summary>
    public AppDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .UseSnakeCaseNamingConvention();
        return new AppDbContext(options.Options);
    }

    /// <summary>POSTs a raw body with a <c>Stripe-Signature</c> header to the webhook endpoint — its contract (no envelope, no auth); the staged <see cref="Gateway"/> event drives the outcome.</summary>
    public static Task<HttpResponseMessage> PostWebhookAsync(HttpClient client, string signature = "test-sig")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("Stripe-Signature", signature);
        return client.SendAsync(request);
    }

    /// <summary>Points both hosts at the shared container before they build (env is the authoritative cross-host seam).</summary>
    protected override void ConfigureEnvironment()
    {
        Environment.SetEnvironmentVariable("DB_CONNECTION", _postgres.ConnectionString);
        Environment.SetEnvironmentVariable("REDIRECT_BASE_URL", RedirectBaseUrl);
        Environment.SetEnvironmentVariable("REDIS_CONNECTION", null); // ensure DbRedirectConfigStore path
    }

    /// <summary>Snapshots the post-migration schema for Respawn, after both hosts have applied migrations.</summary>
    protected override ValueTask InitializeStateAsync(CancellationToken cancellationToken = default) =>
        _postgres.InitializeRespawnerAsync(cancellationToken);

    /// <summary>Provisions a guest via <c>POST /api/identity/guest</c> and returns an Api client carrying the <c>user-id</c> cookie.</summary>
    /// <remarks>The cookie is <c>Secure</c> and won't round-trip over the test host's <c>http://</c>, so it's lifted from <c>Set-Cookie</c> and attached as a raw header.</remarks>
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

    /// <summary>Starts the topology for xUnit — container up, both hosts built and migrated, Respawn snapshotted.</summary>
    Task IAsyncLifetime.InitializeAsync() => StartAsync().AsTask();

    /// <summary>Disposes the hosts then the container for xUnit's <see cref="IAsyncLifetime"/> contract.</summary>
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
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

/// <summary>Convenience base for E2E tests — wires the shared fixture, resets the DB per test, and exposes fresh clients.</summary>
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

    /// <summary>Resets the DB and clears any billing webhook event / captured calls staged on the shared fake gateway.</summary>
    public async Task InitializeAsync()
    {
        await Fixture.ResetAsync();
        Fixture.Gateway.Reset();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
