using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;

namespace SmartQr.Domain.Codes.Content.MobileApp.Models;

/// <summary>Represents one app-store link.</summary>
public sealed record MobileAppLinkContentValueObject : CodeContent
{
    /// <summary>Gets the store this link points at.</summary>
    public required MobileAppStoreType Store { get; init; }

    /// <summary>Gets the store URL.</summary>
    public required string Url { get; init; }

    /// <inheritdoc />
    public override string? Encode() => null;
}
