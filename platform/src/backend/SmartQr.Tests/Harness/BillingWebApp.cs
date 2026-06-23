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
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

// Disambiguate the two top-level `Program` types (both live in their assembly's global namespace).
using ApiProgram = apihost::Program;
using RedirectProgram = redirecthost::Program;
using BillingSettings = SmartQr.Api.Settings.Billing;
using BillingPrices = SmartQr.Api.Settings.BillingPrices;

namespace SmartQr.Tests.Harness;

/// <summary>SQLite-backed two-host billing test app over real HTTP, both hosts sharing one in-memory connection; no Docker, no real Stripe.</summary>
public sealed class BillingWebApp : IDisposable
{
    private const string DummyPgConnection = "Host=localhost;Port=5432;Database=smartqr_test;Username=test;Password=test";

    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<ApiProgram> _apiFactory;
    private readonly WebApplicationFactory<RedirectProgram> _redirectFactory;

    /// <summary>The fake Stripe gateway wired into the Api host — set <see cref="FakeBillingGateway.NextEvent"/> to drive a webhook.</summary>
    public FakeBillingGateway Gateway { get; } = new();

    /// <summary>The fixed current user the Api host resolves every request to (owner of created codes / subscription).</summary>
    public TestCurrentUser CurrentUser { get; } = new();

    /// <summary>JSON options matching the API wire contract: camelCase and string enums.</summary>
    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Builds both hosts over a single shared in-memory SQLite database with its schema created up front.</summary>
    public BillingWebApp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Create the schema with the same snake_case convention the hosts apply, or every read 500s.
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

    /// <summary>A new <see cref="AppDbContext"/> on the shared DB (snake_case convention) for seeding and assertions.</summary>
    public AppDbContext NewDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
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
            // Read by the startup migrate hook (DB name) and the data-source builder; never opened.
            ["DatabaseOptions:ConnectionString"] = DummyPgConnection,
        }));

    /// <summary>Repoints EF off Npgsql onto the single shared SQLite connection so both hosts see the same data.</summary>
    private void UseSharedSqlite(IServiceCollection services) =>
        services.RepointDbContext<AppDbContext>(o => o
            .UseSqlite(_connection)
            .UseSnakeCaseNamingConvention());

    /// <summary>Disables the bespoke SQL migrator so the startup migrate hook touches nothing.</summary>
    private static void NeuterMigrator(IServiceCollection services) => services.DisableBespokeMigrator();

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
