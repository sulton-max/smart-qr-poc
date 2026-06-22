using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartQr.IntegrationTests.Support;

/// <summary>Local mirrors of the API response shapes for E2E assertions — independent of the extern-aliased production types, so only the wire contract is asserted.</summary>
public static class TestJson
{
    /// <summary>JSON options matching the API: camelCase and string enums.</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}

/// <summary>The success envelope every controller wraps payloads in (<c>ApiResponse&lt;T&gt;.Ok</c>).</summary>
/// <typeparam name="T">Payload type.</typeparam>
public sealed record ApiEnvelope<T>
{
    /// <summary>The wrapped payload.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }
}

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
