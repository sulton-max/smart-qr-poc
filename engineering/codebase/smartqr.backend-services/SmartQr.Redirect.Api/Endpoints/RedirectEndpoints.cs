using System.Security.Cryptography;
using System.Text;
using SmartQr.Redirect.Api.Application.Analytics.Models;
using SmartQr.Redirect.Api.Application.Analytics.Services;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Endpoints;

/// <summary>The hot path: <c>GET /{slug}</c> → resolve config → evaluate rules → log async → 302.</summary>
public static class RedirectEndpoints
{
    /// <summary>Maps the slug redirect endpoint.</summary>
    public static void MapRedirect(this WebApplication app) => app.MapGet("/{slug}", HandleAsync);

    private static async Task<IResult> HandleAsync(
        string slug,
        HttpContext http,
        IRedirectConfigRepository store,
        IRoutingService routingService,
        IDeviceResolver deviceResolver,
        IGeoResolver geoResolver,
        IScanRecorder recorder,
        CancellationToken ct)
    {
        var config = await store.GetAsync(slug, ct);
        if (config is null)
            return Results.NotFound();

        var userAgent = http.Request.Headers.UserAgent.ToString();
        var ip = http.Connection.RemoteIpAddress?.ToString();

        var context = new ScanContext
        {
            Slug = slug,
            Device = deviceResolver.Resolve(userAgent),
            CountryCode = geoResolver.ResolveCountry(ip),
            Language = ParsePrimaryLanguage(http.Request.Headers.AcceptLanguage.ToString()),
            NowUtc = DateTimeOffset.UtcNow,
            Referrer = NullIfEmpty(http.Request.Headers.Referer.ToString()),
            UserAgent = userAgent,
            IpAddress = ip,
        };

        var decision = routingService.Evaluate(config, context);

        switch (decision.Outcome)
        {
            case RouteOutcome.NotFound:
                return Results.NotFound();
            case RouteOutcome.Gone:
                return Results.StatusCode(StatusCodes.Status410Gone);
        }

        // Fire-and-forget — the redirect never waits on analytics.
        recorder.Enqueue(new ScanRecord
        {
            CodeId = config.CodeId,
            ScannedAt = context.NowUtc,
            Device = context.Device,
            CountryCode = context.CountryCode,
            Os = OsFromUserAgent(userAgent),
            Referrer = context.Referrer,
            UserAgentHash = HashUserAgent(userAgent),
            MatchedRuleId = decision.MatchedRuleId,
            DestinationUrl = decision.DestinationUrl!,
        });

        return Results.Redirect(decision.DestinationUrl!, permanent: false); // 302
    }

    private static string? ParsePrimaryLanguage(string acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return null;

        // "ru-RU,ru;q=0.9,en;q=0.8" → "ru"
        var first = acceptLanguage.Split(',')[0].Split(';')[0].Trim();
        var primary = first.Split('-')[0].ToLowerInvariant();
        return NullIfEmpty(primary);
    }

    private static string? OsFromUserAgent(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return null;

        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("android")) return "Android";
        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod")) return "iOS";
        if (ua.Contains("windows")) return "Windows";
        if (ua.Contains("mac os") || ua.Contains("macintosh")) return "macOS";
        if (ua.Contains("linux")) return "Linux";
        return null;
    }

    private static string? HashUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return null;

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(userAgent));
        return Convert.ToHexString(hash)[..16];
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
