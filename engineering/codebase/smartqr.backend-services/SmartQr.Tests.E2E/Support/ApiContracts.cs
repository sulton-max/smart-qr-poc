using System.Text.Json.Serialization;

namespace SmartQr.Tests.E2E.Support;

// Local mirrors of the API response shapes for E2E assertions — independent of the extern-aliased
// production types, so only the wire contract is asserted. The JSON options + success envelope live in
// the SDK testing package (WoW.Two.Sdk.Backend.Beta.Testing.Web: TestJson, ApiEnvelope<T>).

/// <summary>Represents the wire shape of <c>MeResponse</c>.</summary>
public sealed record MeResponseDto
{
    /// <summary>Gets the caller's identity kind (Anonymous, Guest, or User).</summary>
    public string Kind { get; init; } = "";
}

/// <summary>Represents the wire shape of <c>CurrentUserDto</c>, including the user profile.</summary>
public sealed record MeWithUserDto
{
    /// <summary>Gets the caller's identity kind (Anonymous, Guest, or User).</summary>
    public string Kind { get; init; } = "";

    /// <summary>Gets the signed-in account profile; null for guest or anonymous.</summary>
    public UserSummaryDtoModel? User { get; init; }
}

/// <summary>Represents the wire shape of <c>UserSummaryDto</c>.</summary>
public sealed record UserSummaryDtoModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
}

/// <summary>Represents the wire shape of <c>CodeDto</c>.</summary>
public sealed record CodeDtoModel
{
    public Guid Id { get; init; }
    public string? Slug { get; init; }
    public string? ShortUrl { get; init; }
    public string Name { get; init; } = "";
    public string BarcodeFormat { get; init; } = "";

    /// <summary>Gets how the symbol resolves, either <c>static</c> or <c>dynamic</c>.</summary>
    public string Mode { get; init; } = "";

    /// <summary>Gets the kind of content every rule of this code carries (e.g. <c>url</c>, <c>wifi</c>).</summary>
    public string ContentType { get; init; } = "";

    public bool IsActive { get; init; }
    public long ScanCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets the routing rules, each carrying the content it serves.</summary>
    public IReadOnlyList<RuleDtoModel> Rules { get; init; } = [];
}

/// <summary>Represents the wire shape of <c>CodeRuleValueObject</c>.</summary>
public sealed record RuleDtoModel
{
    public string Type { get; init; } = "";
    public int? Order { get; init; }
    public string? Condition { get; init; }
    public string? ConditionValue { get; init; }
    public int? TargetOrder { get; init; }
    public ContentDtoModel? Content { get; init; }
}

/// <summary>Represents the wire shape of the content.</summary>
public sealed record ContentDtoModel
{
    public string Type { get; init; } = "";
    public string? Url { get; init; }
    public string? Text { get; init; }
    public string? Ssid { get; init; }
    public string? Password { get; init; }
    public string? FirstName { get; init; }
    public string? Email { get; init; }
}

/// <summary>Represents the wire shape of <c>BillingStatusDto</c>.</summary>
public sealed record BillingStatusDtoModel
{
    /// <summary>Gets the caller's plan name (e.g. <c>Free</c>, <c>Pro</c>).</summary>
    public string Plan { get; init; } = "";

    /// <summary>Gets the subscription status (e.g. <c>active</c>, <c>canceled</c>).</summary>
    public string Status { get; init; } = "";

    /// <summary>Gets the plan's limits.</summary>
    public LimitsDtoModel Limits { get; init; } = new();

    /// <summary>Gets the caller's current usage.</summary>
    public UsageDtoModel Usage { get; init; } = new();
}

/// <summary>Represents the wire shape of <c>LimitsDto</c>.</summary>
public sealed record LimitsDtoModel
{
    /// <summary>Gets the maximum code count, or <c>-1</c> when unlimited.</summary>
    public int MaxCodes { get; init; }
}

/// <summary>Represents the wire shape of <c>UsageDto</c>.</summary>
public sealed record UsageDtoModel
{
    public int CodeCount { get; init; }
}

/// <summary>Represents the wire shape of the checkout and portal session DTOs.</summary>
public sealed record SessionUrlDtoModel
{
    public string Url { get; init; } = "";
}
