using Microsoft.Extensions.DependencyInjection;
using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;
using SmartQr.Redirect.Api.Infrastructure.Routing;
using SmartQr.Redirect.Api.Settings;
using SmartQr.Tests.Integration.Harness;

namespace SmartQr.Tests.Integration;

/// <summary>End-to-end redirect data path against the provider-switchable test database — seed a code, the cached config store reads it, the evaluator resolves the destination.</summary>
public class RedirectResolutionTests(SmartQrTestDb db) : RepositoryTestBase(db)
{
    /// <summary>Builds the redirect routing services over the shared test database — the config store resolves <see cref="AppDbContext"/> per scope from the fixture, so it reads the same data the seeder writes (both providers).</summary>
    private ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Db.NewContext());
        services.AddMemoryCache();
        services.AddSingleton(new RedirectSettings { ConfigCacheSeconds = 30 });
        services.AddSingleton<IRoutingService, RoutingService>();
        services.AddSingleton<IRedirectConfigRepository, CachedRedirectConfigRepository>();
        return services.BuildServiceProvider();
    }

    private async Task SeedCodeAsync(string slug)
    {
        await using var ctx = NewContext();
        var id = Guid.NewGuid();
        ctx.Codes.Add(new CodeEntity
        {
            Id = id,
            Slug = slug,
            UserId = Guid.NewGuid(),
            Name = "App",
            CodeType = CodeType.Qr,
            BarcodeFormat = BarcodeFormat.QrCode,
            StyleJson = "{}",
            IsActive = true,
            NeverExpires = true,
            Content = new UrlContent { Url = "https://fallback.example" },
            Rules =
            [
                new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = id, Order = 1, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = "https://apple.example" },
                // The catch-all is a trailing Default rule — it replaces the retired fallback_url column.
                new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = id, Order = 2, ConditionType = RuleConditionType.Default, Destination = "https://fallback.example" },
            ],
        });
        await ctx.SaveChangesAsync();
    }

    private static ScanContext Scan(string slug, DeviceType device) => new()
    {
        Slug = slug,
        Device = device,
        NowUtc = DateTimeOffset.UnixEpoch,
    };

    [Fact]
    public async Task Ios_scan_routes_to_rule()
    {
        await SeedCodeAsync("route123");
        await using var sp = BuildProvider();

        var config = await sp.GetRequiredService<IRedirectConfigRepository>().GetAsync("route123", default);
        Assert.NotNull(config);

        var decision = sp.GetRequiredService<IRoutingService>().Evaluate(config!, Scan("route123", DeviceType.Ios));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://apple.example", decision.DestinationUrl);
    }

    [Fact]
    public async Task Desktop_scan_hits_the_default_rule()
    {
        await SeedCodeAsync("route123");
        await using var sp = BuildProvider();

        var config = await sp.GetRequiredService<IRedirectConfigRepository>().GetAsync("route123", default);
        var decision = sp.GetRequiredService<IRoutingService>().Evaluate(config!, Scan("route123", DeviceType.Desktop));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://fallback.example", decision.DestinationUrl);
        Assert.NotNull(decision.MatchedRuleId); // the Default rule matched — it is a rule like any other
    }

    [Fact]
    public async Task Unknown_slug_resolves_to_null()
    {
        await using var sp = BuildProvider();

        var config = await sp.GetRequiredService<IRedirectConfigRepository>().GetAsync("missing", default);

        Assert.Null(config); // endpoint maps this to 404
    }

    /// <summary>Never-deactivate-on-downgrade — a code whose owner is far over their plan cap still resolves, since the redirect path is plan-agnostic.</summary>
    [Fact]
    public async Task Over_cap_owners_code_still_resolves()
    {
        var owner = Guid.NewGuid();

        // Seed many codes for one owner (Free cap is 3) — all owned by the same over-cap user.
        await using (var ctx = NewContext())
        {
            for (var i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                ctx.Codes.Add(new CodeEntity
                {
                    Id = id,
                    Slug = i == 0 ? "overcap1" : $"oc{i:D5}",
                    UserId = owner,
                    Name = $"code-{i}",
                    CodeType = CodeType.Qr,
                    BarcodeFormat = BarcodeFormat.QrCode,
                    StyleJson = "{}",
                    IsActive = true,
                    NeverExpires = true,
                    Content = new UrlContent { Url = "https://still-works.example" },
                    // A plain url code carries its destination as a Default catch-all rule.
                    Rules =
                    [
                        new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = id, Order = 1, ConditionType = RuleConditionType.Default, Destination = "https://still-works.example" },
                    ],
                });
            }

            await ctx.SaveChangesAsync();
        }

        await using var sp = BuildProvider();

        // The 1st code (way past the cap, no subscription row ⇒ Free) resolves like any other.
        var config = await sp.GetRequiredService<IRedirectConfigRepository>().GetAsync("overcap1", default);
        Assert.NotNull(config);

        var decision = sp.GetRequiredService<IRoutingService>().Evaluate(config!, Scan("overcap1", DeviceType.Desktop));
        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://still-works.example", decision.DestinationUrl);
    }
}
