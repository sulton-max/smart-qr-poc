using System.Text.Json.Serialization;

namespace SmartQr.IntegrationTests.Support;

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
