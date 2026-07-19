namespace SmartQr.Common.Domain.Codes.Core.Enums;

/// <summary>Defines how a code's symbol resolves — chosen at create and immutable afterwards, since the two bake different bytes.</summary>
public enum ContentMode
{
    /// <summary>Represents a symbol carrying the payload itself; it never reaches the server, so it works offline and cannot be edited.</summary>
    Static,

    /// <summary>Represents a symbol carrying the redirect short link; the destination stays editable after printing.</summary>
    Dynamic,
}
