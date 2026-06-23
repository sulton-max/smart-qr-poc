extern alias apihost;
extern alias redirecthost;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Common.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Testing.Containers;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

// Disambiguate the two top-level `Program` types (both live in their assembly's global namespace).
using ApiProgram = apihost::Program;
using RedirectProgram = redirecthost::Program;
using BillingSettings = SmartQr.Api.Settings.Billing;
using BillingPrices = SmartQr.Api.Settings.BillingPrices;

namespace SmartQr.Tests.Harness;

/// <summary>Provider-switchable two-host billing test app over real HTTP — both hosts share one database (in-memory SQLite or a Postgres container), with a fake Stripe gateway and a fixed current user; no real Stripe.</summary>
/// <remarks>
/// Selected by <see cref="TestSetupOptions.Current"/> (default Postgres). Used as an <c>IClassFixture</c> so the container / connection and both hosts build once; <see cref="ResetAsync"/> isolates each test.
/// SQLite: both hosts repoint onto one shared in-memory connection and the bespoke migrator is neutered (schema via <c>EnsureCreated</c>). Postgres: a shared container, the real bespoke SQL migrator builds the schema at host startup, and Respawn truncates between tests.
/// </remarks>
public sealed class BillingWebApp : IAsyncLifetime
{
    private const string DummyPgConnection = "Host=localhost;Port=5432;Database=smartqr_test;Username=test;Password=test";

    private readonly DatabaseProvider _provider = TestSetupOptions.Current.Database;

    // SQLite mode: the single shared in-memory connection both hosts (and seeding) bind to.
    private SqliteConnection? _connection;

    // Postgres mode: the shared container + Respawn reset.
    private PostgresFixture? _postgres;

    private WebApplicationFactory<ApiProgram> _apiFactory = null!;
    private WebApplicationFactory<RedirectProgram> _redirectFactory = null!;

    /// <summary>The fake Stripe gateway wired into the Api host — set <see cref="FakeBillingGateway.NextEvent"/> to drive a webhook.</summary>
    public FakeBillingGateway Gateway { get; } = new();

    /// <summary>The fixed current user the Api host resolves every request to (owner of created codes / subscription).</summary>
    public TestCurrentUser CurrentUser { get; } = new();

    /// <summary>JSON options matching the API wire contract: camelCase and string enums.</summary>
    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>The active database connection string both hosts read (SQLite or the started container).</summary>
    private string ActiveConnectionString => _provider == DatabaseProvider.Sqlite
        ? DummyPgConnection // SQLite hosts never open it — they're repointed onto the shared connection.
        : _postgres!.ConnectionString;

    /// <summary>Brings up the database (container or shared in-memory connection), builds both hosts, and snapshots Respawn for Postgres.</summary>
    public async Task InitializeAsync()
    {
        if (_provider == DatabaseProvider.Sqlite)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Create the schema with the same snake_case convention the hosts apply, or every read 500s.
            await using (var ctx = NewDbContext())
                await ctx.Database.EnsureCreatedAsync();
        }
        else
        {
            _postgres = new PostgresFixture();
            await _postgres.StartAsync();

            // Point both hosts at the shared container the way the app reads it — env DB_CONNECTION is the authoritative
            // cross-host seam (wins over config), matching the proven E2E AppFixture; clear Redis so the DB config store is used.
            Environment.SetEnvironmentVariable("DB_CONNECTION", _postgres.ConnectionString);
            Environment.SetEnvironmentVariable("REDIS_CONNECTION", null);
        }

        _apiFactory = BuildApiHost();
        _redirectFactory = BuildRedirectHost();

        if (_provider == DatabaseProvider.Postgres)
        {
            // Force both hosts to build (and run their startup migrator), creating the schema before Respawn snapshots it.
            _apiFactory.CreateClient().Dispose();
            _redirectFactory.CreateClient().Dispose();
            await _postgres!.InitializeRespawnerAsync();
        }
    }

    /// <summary>Resets the shared database to empty between tests (SQLite: drop and recreate the in-memory schema; Postgres: Respawn truncate).</summary>
    public async Task ResetAsync()
    {
        if (_provider == DatabaseProvider.Sqlite)
        {
            await using var ctx = NewDbContext();
            await ctx.Database.EnsureDeletedAsync();
            await ctx.Database.EnsureCreatedAsync();
        }
        else
        {
            await _postgres!.ResetAsync();
        }
    }

    /// <summary>A fresh Api client (owner-scoped via the fixed <see cref="CurrentUser"/>).</summary>
    public HttpClient ApiClient() => _apiFactory.CreateClient();

    /// <summary>A Redirect client that does NOT auto-follow 302s, so the <c>Location</c> can be asserted.</summary>
    public HttpClient RedirectClient() => _redirectFactory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    /// <summary>A new <see cref="AppDbContext"/> on the shared database (snake_case convention) for seeding and assertions.</summary>
    public AppDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSnakeCaseNamingConvention();
        if (_provider == DatabaseProvider.Sqlite)
            options.UseSqlite(_connection!);
        else
            options.UseNpgsql(_postgres!.ConnectionString);
        return new AppDbContext(options.Options);
    }

    private WebApplicationFactory<ApiProgram> BuildApiHost() =>
        new WebApplicationFactory<ApiProgram>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Production);
            InjectConfig(builder);
            builder.ConfigureTestServices(services =>
            {
                UseSharedDatabase(services);

                // Swap the real Stripe gateway and cookie identity for in-test doubles.
                services.RemoveAll<IBillingGateway>();
                services.AddSingleton<IBillingGateway>(Gateway);
                services.RemoveAll<ICurrentUser>();
                services.AddSingleton<ICurrentUser>(CurrentUser);

                // Replace Billing as a DI singleton — the host loads it before the in-memory config layer lands.
                services.RemoveAll<BillingSettings>();
                services.AddSingleton(new BillingSettings
                {
                    SecretKey = "sk_test_fake",
                    WebhookSecret = "whsec_fake",
                    Prices = new BillingPrices { Solo = "price_solo", Pro = "price_pro", Agency = "price_agency" },
                    SuccessUrl = "https://app.example/ok",
                    CancelUrl = "https://app.example/cancel",
                });
            });
        });

    private WebApplicationFactory<RedirectProgram> BuildRedirectHost() =>
        new WebApplicationFactory<RedirectProgram>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Production);
            InjectConfig(builder);
            builder.ConfigureTestServices(UseSharedDatabase);
        });

    private void InjectConfig(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Read by the startup migrate hook (DB name) and the data-source builder.
            // SQLite: a dummy never opened (the host is repointed); Postgres: the live container.
            ["DatabaseOptions:ConnectionString"] = ActiveConnectionString,
        }));

    /// <summary>Repoints EF onto the shared test database: SQLite onto the one in-memory connection (migrator neutered), Postgres left on the container (real migrator builds the schema).</summary>
    private void UseSharedDatabase(IServiceCollection services)
    {
        if (_provider != DatabaseProvider.Sqlite)
            return;

        services.RepointDbContext<AppDbContext>(o => o
            .UseSqlite(_connection!)
            .UseSnakeCaseNamingConvention());
        services.DisableBespokeMigrator();
    }

    /// <summary>POSTs <paramref name="value"/> as JSON (camelCase and string enums) to <paramref name="url"/> on the given client.</summary>
    public static Task<HttpResponseMessage> PostJsonAsync(HttpClient client, string url, object value) =>
        client.PostAsync(url, JsonContent.Create(value, options: Json));

    /// <summary>POSTs a raw body with a <c>Stripe-Signature</c> header — the webhook contract (no envelope, no auth).</summary>
    public static Task<HttpResponseMessage> PostWebhookAsync(HttpClient client, string signature = "test-sig")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("Stripe-Signature", signature);
        return client.SendAsync(request);
    }

    /// <summary>GETs <paramref name="url"/> and reads the <c>ApiResponse&lt;T&gt;.Ok</c> envelope's <c>data</c> payload.</summary>
    public static async Task<T> GetEnvelopeAsync<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await ReadEnvelopeAsync<T>(response);
    }

    /// <summary>Reads the <c>data</c> payload out of an <c>ApiResponse&lt;T&gt;</c> success envelope.</summary>
    public static async Task<T> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>(Json);
        return envelope!.Data!;
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _apiFactory.DisposeAsync();
        await _redirectFactory.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
        if (_postgres is not null)
        {
            Environment.SetEnvironmentVariable("DB_CONNECTION", null);
            await _postgres.DisposeAsync();
        }
    }

    /// <summary>The success envelope every controller wraps payloads in (<c>ApiResponse&lt;T&gt;.Ok</c>).</summary>
    /// <typeparam name="T">Payload type.</typeparam>
    public sealed record Envelope<T>
    {
        /// <summary>The wrapped payload.</summary>
        [JsonPropertyName("data")]
        public T? Data { get; init; }
    }
}
