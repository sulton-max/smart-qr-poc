using SmartQr.Domain.Codes.Content;

namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>The catch-all serving its own content — carries no order or condition, since it is never matched.</summary>
public sealed record DefaultRule : CodeRule
{
    /// <summary>The content served when no conditional rule matches.</summary>
    public required CodeContent Content { get; init; }
}
