using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;

namespace SmartQr.Domain.Codes.Content.MobileApp.Models;

/// <summary>One app-store link — carried by a rule, which supplies the device condition that selects it.</summary>
public sealed record MobileAppLinkContent : CodeContent
{
    /// <summary>The store this link points at.</summary>
    public required MobileAppStoreType Store { get; init; }

    /// <summary>The store URL.</summary>
    public required string Url { get; init; }

    /// <inheritdoc />
    public override string? Encode() => null;
}
