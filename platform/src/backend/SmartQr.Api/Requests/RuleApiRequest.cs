using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Requests;

/// <summary>Represents a routing rule in the create-code and update-code request bodies.</summary>
public sealed record RuleApiRequest
{
    /// <summary>Gets the evaluation order of the rule (ascending; first match wins).</summary>
    public required int Order { get; init; }

    /// <summary>Gets the dimension the rule is matched against.</summary>
    public required RuleConditionType ConditionType { get; init; }

    /// <summary>Gets the condition operand (e.g. <c>Ios</c>, <c>US</c>, <c>ru</c>, <c>09:00-16:00</c>).</summary>
    public string? ConditionValue { get; init; }

    /// <summary>Gets the destination URL applied when the rule matches.</summary>
    public required string Destination { get; init; }
}

/// <summary>Provides mapping for <see cref="RuleApiRequest"/>.</summary>
public static class RuleApiRequestExtensions
{
    /// <summary>Maps the rule request to its <see cref="RuleDto"/>.</summary>
    public static RuleDto ToRuleDto(this RuleApiRequest request)
    {
        var dto = new RuleDto
        {
            Order = request.Order,
            ConditionType = request.ConditionType,
            ConditionValue = request.ConditionValue,
            Destination = request.Destination,
        };

        return dto;
    }

    /// <summary>Maps the rule requests to their <see cref="RuleDto"/> collection.</summary>
    public static IReadOnlyList<RuleDto> ToRuleDtos(this IReadOnlyList<RuleApiRequest> requests)
    {
        var dtos = requests.Select(rule => rule.ToRuleDto()).ToList();

        return dtos;
    }
}
