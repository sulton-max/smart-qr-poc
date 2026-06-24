using System.Text.Json.Serialization;

namespace SmartQr.Tests.E2E.Support;

// Local mirrors of the API response shapes for E2E assertions — independent of the extern-aliased
// production types, so only the wire contract is asserted. The JSON options + success envelope live in
// the SDK testing package (WoW.Two.Sdk.Backend.Beta.Testing.Web: TestJson, ApiEnvelope<T>).

/// <summary>Wire shape of <c>MeResponse</c>.</summary>
public sealed record MeResponseDto
{
    /// <summary>The caller's identity kind (Anonymous / Guest / User).</summary>
    public string Kind { get; init; } = "";
}

/// <summary>Wire shape of <c>CurrentUserDto</c> including the registered-user profile.</summary>
public sealed record MeWithUserDto
{
    /// <summary>The caller's identity kind (Anonymous / Guest / User).</summary>
    public string Kind { get; init; } = "";

    /// <summary>The signed-in account profile; null for guest / anonymous.</summary>
    public UserSummaryDtoModel? User { get; init; }
}

/// <summary>Wire shape of <c>UserSummaryDto</c>.</summary>
public sealed record UserSummaryDtoModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
}

/// <summary>Wire shape of <c>CodeDto</c>.</summary>
public sealed record CodeDtoModel
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = "";
    public string ShortUrl { get; init; } = "";
    public string Name { get; init; } = "";
    public string CodeType { get; init; } = "";
    public string BarcodeFormat { get; init; } = "";
    public string FallbackUrl { get; init; } = "";
    public bool IsActive { get; init; }
    public bool NeverExpires { get; init; }
    public long ScanCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<RuleDtoModel> Rules { get; init; } = [];
}

/// <summary>Wire shape of <c>RuleDto</c>.</summary>
public sealed record RuleDtoModel
{
    public int Order { get; init; }
    public string ConditionType { get; init; } = "";
    public string? ConditionValue { get; init; }
    public string Destination { get; init; } = "";
}

/// <summary>Wire shape of <c>BillingStatusDto</c> (GET <c>/api/billing/me</c>) — <c>Plan</c> is the enum name (string-enum JSON).</summary>
public sealed record BillingStatusDtoModel
{
    /// <summary>The caller's plan name (e.g. <c>Free</c>, <c>Pro</c>).</summary>
    public string Plan { get; init; } = "";

    /// <summary>The subscription status, lower-cased to mirror Stripe (e.g. <c>active</c>, <c>canceled</c>).</summary>
    public string Status { get; init; } = "";

    /// <summary>The plan's limits.</summary>
    public LimitsDtoModel Limits { get; init; } = new();

    /// <summary>The caller's current usage.</summary>
    public UsageDtoModel Usage { get; init; } = new();
}

/// <summary>Wire shape of <c>LimitsDto</c> — <c>MaxCodes</c> is <c>-1</c> for the unlimited (Agency) sentinel.</summary>
public sealed record LimitsDtoModel
{
    public int MaxCodes { get; init; }
}

/// <summary>Wire shape of <c>UsageDto</c>.</summary>
public sealed record UsageDtoModel
{
    public int CodeCount { get; init; }
}

/// <summary>Wire shape of the Checkout / Portal session DTOs — both carry a single <c>url</c>.</summary>
public sealed record SessionUrlDtoModel
{
    public string Url { get; init; } = "";
}
