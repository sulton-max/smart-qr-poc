namespace SmartQr.Common.Domain.Codes.Core.Enums;

/// <summary>Defines how a code's symbol resolves.</summary>
public enum ContentMode
{
    /// <summary>Represents a symbol carrying the payload itself.</summary>
    Static,

    /// <summary>Represents a symbol carrying the short link.</summary>
    Dynamic,
}
