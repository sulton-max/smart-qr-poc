namespace SmartQr.Domain.Codes.Rules.Enums;

/// <summary>Defines the role a rule plays in a code's routing.</summary>
public enum CodeRuleType
{
    /// <summary>Represents a rule matched against a scan signal, in order.</summary>
    Conditional,

    /// <summary>Represents the catch-all serving its own content when no conditional rule matches.</summary>
    Default,

    /// <summary>Represents the catch-all delegating to another rule's content.</summary>
    DefaultPointer,
}
