namespace SmartQr.Common.Domain.Codes.Core.Enums;

/// <summary>Defines how a code's symbol resolves — chosen at create, immutable afterwards.</summary>
public enum ContentMode
{
    /// <summary>Represents a symbol carrying the payload itself — it works offline and cannot be edited.</summary>
    Static,

    /// <summary>Represents a symbol carrying the short link; the destination stays editable after printing.</summary>
    Dynamic,
}
