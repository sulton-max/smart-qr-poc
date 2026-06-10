using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Domain.Common.Entities;

namespace SmartQr.Common.Domain.Codes.Entities;

/// <summary>A single conditional routing rule belonging to a <see cref="CodeEntity"/>.</summary>
/// <example>routing_rules</example>
public sealed record RoutingRuleEntity : IEntity
{
    /// <inheritdoc />
    public static string TableName => "routing_rules";

    /// <summary>Gets or sets the UUID primary key of the rule.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the owning code id.</summary>
    public required Guid CodeId { get; set; }

    /// <summary>Gets or sets the evaluation order (ascending; first match wins).</summary>
    public required int Order { get; set; }

    /// <summary>Gets or sets the dimension this rule matches against.</summary>
    public required RuleConditionType ConditionType { get; set; }

    /// <summary>Gets or sets the condition operand — interpretation depends on <see cref="ConditionType"/>.</summary>
    /// <example>Ios | US | ru | 09:00-16:00</example>
    public string? ConditionValue { get; set; }

    /// <summary>Gets or sets the destination URL used when this rule matches.</summary>
    /// <example>https://apps.apple.com/app/id000000000</example>
    public required string Destination { get; set; }

    /// <summary>Gets or sets the creation timestamp (auto-set on insert).</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
