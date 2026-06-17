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
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Common.Persistence.Migrations;

// Disambiguate the two top-level `Program` types (both live in their assembly's global namespace).
using ApiProgram = apihost::Program;
using RedirectProgram = redirecthost::Program;
using BillingSettings = SmartQr.Api.Settings.Billing;
using BillingPrices = SmartQr.Api.Settings.BillingPrices;

namespace SmartQr.Tests.Harness;

/// <summary>
/// A SQLite-backed, two-host billing test app — the WebApplicationFactory analogue of the existing direct-handler
/// billing unit tests, but driven over real HTTP. Owns ONE open in-memory SQLite connection that BOTH hosts share,
/// so a code created through the Api host is resolvable on the Redirect host's hot path. No Docker, no real Stripe:
/// the Api host's <see cref="IBillingGateway"/> is the in-test <see cref="FakeBillingGateway"/> and its
/// <see cref="ICurrentUser"/> is a fixed-id <see cref="TestCurrentUser"/>; the bespoke SQL migrator is neutered
/// (schema comes from EF <c>EnsureCreated()</c>).
/// </summary>
/// <remarks>
/// Both hosts run their startup <c>MigrateSmartQrDatabaseAsync()</c>, which targets Npgsql — the
/// <see cref="NoOpMigrationDialect"/> / <see cref="NoOpMigrationRunner"/> make that a no-op so nothing hits Postgres.
/// A parseable (but never-opened) Postgres connection string is injected only because the startup hook reads its
/// database name.
/// </remarks>
public sealed class BillingWebApp : IDisposable
{
    // A dummy, parseable Npgsql connection string. Never opened — the no-op migrator short-circuits the only path
    // that would dial Postgres; the data-source singleton is lazy and the redirect read path uses EF over SQLite.
    private const string DummyPgConnection = "Host=localhost;Port=5432;Database=smartqr_test;Username=test;Password=test";

    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<ApiProgram> _apiFactory;
    private readonly WebApplicationFactory<RedirectProgram> _redirectFactory;

    /// <summary>The fake Stripe gateway wired into the Api host — set <see cref="FakeBillingGateway.NextEvent"/> to drive a webhook.</summary>
    public FakeBillingGateway Gateway { get; } = new();

    /// <summary>The fixed current user the Api host resolves every request to (owner of created codes / subscription).</summary>
    public TestCurrentUser CurrentUser { get; } = new();

    /// <summary>JSON options matching the API wire contract: camelCase + string enums.</summary>
    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Builds both hosts over a single shared in-memory SQLite database with its schema created up front.</summary>
    public BillingWebApp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Create the schema once on the shared connection (subscriptions + codes + rules). MUST use the same
        // snake_case convention the hosts apply (AddSmartQrPersistence → UseSnakeCaseNamingConvention), otherwise the
        // created columns are PascalCase while the host queries snake_case (e.g. `created_at`) and every read 500s.
        using (var ctx = NewDbContext())
        {
            ctx.Database.EnsureCreated();
        }

        _apiFactory = BuildApiHost();
        _redirectFactory = BuildRedirectHost();
    }

    /// <summary>A fresh Api client (owner-scoped via the fixed <see cref="CurrentUser"/>).</summary>
    public HttpClient ApiClient() => _apiFactory.CreateClient();

    /// <summary>A Redirect client that does NOT auto-follow 302s, so the <c>Location</c> can be asserted.</summary>
    public HttpClient RedirectClient() => _redirectFactory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    /// <summary>
    /// A new <see cref="SmartQrDbContext"/> on the shared DB — for schema creation and direct seeding / assertions.
    /// Applies the same snake_case convention as the hosts so column names line up across schema, seeds, and queries.
    /// </summary>
    public SmartQrDbContext NewDbContext() =>
        new(new DbContextOptionsBuilder<SmartQrDbContext>()
            .UseSqlite(_connection)
            .UseSnakeCaseNamingConvention()
            .Options);

    private WebApplicationFactory<ApiProgram> BuildApiHost() =>
        new WebApplicationFactory<ApiProgram>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Production);
            InjectDummyConfig(builder);
            builder.ConfigureTestServices(services =>
            {
                UseSharedSqlite(services);
                NeuterMigrator(services);

                // No network: swap the real Stripe gateway + the cookie identity for in-test doubles.
                services.RemoveAll<IBillingGateway>();
                services.AddSingleton<IBillingGateway>(Gateway);
                services.RemoveAll<ICurrentUser>();
                services.AddSingleton<ICurrentUser>(CurrentUser);

                // Replace the Billing settings instance directly. The host loads it via ConfigurationLoader.Load<Billing>
                // at registration time (inside Program), which reads config BEFORE WithWebHostBuilder's in-memory layer
                // lands — so a config-only price map wouldn't reach it. The price map MUST be populated so the webhook's
                // price→Plan inverse resolves `price_pro` → Pro.
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
            InjectDummyConfig(builder);
            builder.ConfigureTestServices(services =>
            {
                UseSharedSqlite(services);
                NeuterMigrator(services);
            });
        });

    private static void InjectDummyConfig(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Read by the startup migrate hook (for the DB name) and the data-source builder; never actually opened.
            // (Billing settings are replaced as a DI singleton in ConfigureTestServices, not via config — see there.)
            ["SmartQrDbSettings:ConnectionString"] = DummyPgConnection,
        }));

    /// <summary>Repoints EF off Npgsql onto the single shared SQLite connection so both hosts see the same data.</summary>
    /// <remarks>
    /// <c>AddDbContext(UseNpgsql…)</c> registers the provider through an <c>IDbContextOptionsConfiguration&lt;SmartQrDbContext&gt;</c>
    /// that ACCUMULATES — so a second <c>AddDbContext(UseSqlite…)</c> would leave BOTH providers applied to the same
    /// context's options (EF throws "Only a single database provider can be registered"). Strip every EF descriptor for
    /// this context (the options, its accumulating configuration, and the context registration) before re-adding SQLite.
    /// </remarks>
    private void UseSharedSqlite(IServiceCollection services)
    {
        services.RemoveAllForDbContext<SmartQrDbContext>();

        services.AddDbContext<SmartQrDbContext>(o => o
            .UseSqlite(_connection)
            .UseSnakeCaseNamingConvention());
    }

    /// <summary>Replaces the Postgres SQL migrator with no-ops so the startup migrate hook touches nothing.</summary>
    private static void NeuterMigrator(IServiceCollection services)
    {
        services.RemoveAll<IMigrationDialect>();
        services.AddSingleton<IMigrationDialect, NoOpMigrationDialect>();
        services.RemoveAll<IMigrationRunnerService>();
        services.AddSingleton<IMigrationRunnerService, NoOpMigrationRunner>();
    }

    /// <summary>POSTs <paramref name="value"/> as JSON (camelCase + string enums) to <paramref name="url"/> on the given client.</summary>
    public static Task<HttpResponseMessage> PostJsonAsync(HttpClient client, string url, object value) =>
        client.PostAsync(url, JsonContent.Create(value, options: Json));

    /// <summary>POSTs a raw body with a <c>Stripe-Signature</c> header — the webhook contract (no envelope, no auth).</summary>
    /// <remarks>The request is NOT disposed here — disposing it would tear down its content while <c>SendAsync</c> is still reading it.</remarks>
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
    public void Dispose()
    {
        _apiFactory.Dispose();
        _redirectFactory.Dispose();
        _connection.Dispose();
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

/// <summary>Service-collection helpers for the billing host tests — strip a host's EF registration so SQLite can replace it.</summary>
internal static class BillingWebAppServiceCollectionExtensions
{
    /// <summary>
    /// Removes every descriptor that binds <typeparamref name="TContext"/> to its (Npgsql) provider: the context itself,
    /// its <c>DbContextOptions</c>/<c>DbContextOptions&lt;T&gt;</c>, and the accumulating
    /// <c>IDbContextOptionsConfiguration&lt;T&gt;</c> that carries <c>UseNpgsql(…)</c>. Leaves EF's provider-agnostic
    /// infrastructure intact so a fresh <c>AddDbContext&lt;T&gt;(UseSqlite…)</c> wires cleanly.
    /// </summary>
    public static IServiceCollection RemoveAllForDbContext<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        // IDbContextOptionsConfiguration<T> is internal (EF 9) — match it by open-generic name rather than a type ref.
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var serviceType = services[i].ServiceType;
            var isContext = serviceType == typeof(TContext);
            var isOptions = serviceType == typeof(DbContextOptions)
                || serviceType == typeof(DbContextOptions<TContext>);
            var isOptionsConfig = serviceType.IsGenericType
                && serviceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal)
                && serviceType.GetGenericArguments() is [var arg] && arg == typeof(TContext);

            if (isContext || isOptions || isOptionsConfig)
                services.RemoveAt(i);
        }

        return services;
    }
}
