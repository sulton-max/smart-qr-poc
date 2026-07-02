using SmartQr.Domain.Codes.Enums;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Domain.Codes.Entities;

/// <summary>Represents a single conditional routing rule belonging to a <see cref="CodeEntity"/>.</summary>
public sealed record RoutingRuleEntity : IKeyedEntity<Guid>, IHasTableName, ICreationAuditable
{
    /// <summary>Gets the storage table name for the routing-rule entity — the single source of truth for hand-written SQL.</summary>
    public static string TableName => "routing_rules";

    /// <summary>Gets or sets the UUID primary key of the rule.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the id of the owning code of the rule.</summary>
    /// <remarks>FK to <c>codes.id</c> with <c>ON DELETE CASCADE</c>.</remarks>
    public required Guid CodeId { get; set; }

    /// <summary>Gets or sets the evaluation order of the rule (ascending; first match wins).</summary>
    public required int Order { get; set; }

    /// <summary>Gets or sets the dimension this rule matches against.</summary>
    public required RuleConditionType ConditionType { get; set; }

    /// <summary>Gets or sets the condition operand of the rule — interpretation depends on <see cref="ConditionType"/>.</summary>
    /// <remarks>Per <see cref="ConditionType"/>: device class, ISO country, language tag, or <c>HH:mm-HH:mm</c> window.</remarks>
    public string? ConditionValue { get; set; }

    /// <summary>Gets or sets the destination URL of the rule, used when this rule matches.</summary>
    public required string Destination { get; set; }

    /// <summary>Gets or sets the creation timestamp of the rule.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
