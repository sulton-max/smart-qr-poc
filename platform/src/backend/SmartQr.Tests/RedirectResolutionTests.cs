using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Redirect.Application.Routing.Models;
using SmartQr.Redirect.Application.Routing.Services;
using SmartQr.Redirect.Infrastructure.Routing;
using SmartQr.Redirect.Settings;

namespace SmartQr.Tests;

/// <summary>
/// End-to-end redirect data path against SQLite: seed a code → the cached config store reads it →
/// the evaluator resolves the destination. Exercises the real DI wiring used by the redirect service.
/// </summary>
public class RedirectResolutionTests
{
    private static ServiceProvider BuildProvider(SqliteTestDb db)
    {
        var services = new ServiceCollection();
        services.AddDbContext<SmartQrDbContext>(o => o.UseSqlite(db.Connection));
        services.AddMemoryCache();
        services.AddSingleton(new RedirectSettings { ConfigCacheSeconds = 30 });
        services.AddSingleton<IRoutingEvaluator, RoutingEvaluator>();
        services.AddSingleton<IRedirectConfigStore, CachedRedirectConfigStore>();
        return services.BuildServiceProvider();
    }

    private static async Task SeedCodeAsync(SqliteTestDb db, string slug)
    {
        await using var ctx = db.NewContext();
        var id = Guid.NewGuid();
        ctx.Codes.Add(new CodeEntity
        {
            Id = id,
            Slug = slug,
            OwnerId = Guid.NewGuid(),
            Name = "App",
            CodeType = CodeType.Qr,
            BarcodeFormat = BarcodeFormat.QrCode,
            FallbackUrl = "https://fallback.example",
            Rules =
            [
                new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = id, Order = 1, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = "https://apple.example" },
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
        using var db = new SqliteTestDb();
        await SeedCodeAsync(db, "route123");
        await using var sp = BuildProvider(db);

        var config = await sp.GetRequiredService<IRedirectConfigStore>().GetAsync("route123", default);
        Assert.NotNull(config);

        var decision = sp.GetRequiredService<IRoutingEvaluator>().Evaluate(config!, Scan("route123", DeviceType.Ios));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://apple.example", decision.DestinationUrl);
    }

    [Fact]
    public async Task Desktop_scan_falls_back()
    {
        using var db = new SqliteTestDb();
        await SeedCodeAsync(db, "route123");
        await using var sp = BuildProvider(db);

        var config = await sp.GetRequiredService<IRedirectConfigStore>().GetAsync("route123", default);
        var decision = sp.GetRequiredService<IRoutingEvaluator>().Evaluate(config!, Scan("route123", DeviceType.Desktop));

        Assert.Equal("https://fallback.example", decision.DestinationUrl);
        Assert.Null(decision.MatchedRuleId);
    }

    [Fact]
    public async Task Unknown_slug_resolves_to_null()
    {
        using var db = new SqliteTestDb();
        await using var sp = BuildProvider(db);

        var config = await sp.GetRequiredService<IRedirectConfigStore>().GetAsync("missing", default);

        Assert.Null(config); // endpoint maps this to 404
    }
}
